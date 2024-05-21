using AutoMapper;
using BookIt.Application.Models.Reservation;
using BookIt.Application.Services;
using BookIt.Core.Entities;
using BookIt.Core.Enums;
using BookIt.Core.Entities.Identity;
using BookIt.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BookIt.Frontend.Pages.Reservation
{
    [Authorize(Roles = "Customer, Manager")]
    public class CreateReservationModel : PageModel
    {
        private readonly IReservationService _reservationService;
        private readonly IRestaurantService _restaurantService;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReservationRepository _reservationRepository;
        private readonly ITableRepository _tableRepository;
        private readonly ITableService _tableService;
        private readonly IMapper _mapper;
        private List<string> timeSlots = new List<string> { "12:00", "14:00", "16:00", "18:00", "20:00", "22:00"};
        public CreateReservationModel(IRestaurantRepository restaurantRepository,
            ITableRepository tableRepository,
            IMapper mapper,
            IRestaurantService restaurantService,
            UserManager<ApplicationUser> userManager,
            IReservationRepository reservationRepository,
            IReservationService reservationService,
            ITableService tableService,
            IUserRepository userRepository)
        {
            _reservationService = reservationService;
            _mapper = mapper;
            _userManager = userManager;
            _tableRepository = tableRepository;
            _restaurantService = restaurantService;
            _tableService = tableService;
            _reservationRepository = reservationRepository;
            _userRepository = userRepository;
        }

        [BindProperty]
        public ReservationModelForCreate NewReservation { get; set; }

        [BindProperty]
        public IEnumerable<Table> Tables { get; set; }


        public List<string> AvailableSlots { get; set; }


        public async Task<IActionResult> OnPostAsync(int numberOfPersons, string tableArea, string smokingArea, string reservationDate, Guid restaurantId, string reservationTime, string reservationDetails)
        {
            var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;
            var currentUserRole = _userManager.GetRolesAsync(currentUser).Result.First();

            var restaurant = _restaurantService.GetAllRestaurants().FirstOrDefault(r => r.Id == restaurantId);
            if (restaurant != null)
            {
                ViewData["RestaurantName"] = restaurant.RestaurantName;
                ViewData["Address"] = restaurant.Address;
            }

            ViewData["ReservationDate"] = reservationDate;
            ViewData["NumberOfPersons"] = numberOfPersons;
            ViewData["SmokingArea"] = smokingArea;
            ViewData["TableArea"] = tableArea;
            ViewData["RestaurantId"] = restaurantId;

            var tables = _tableService.GetAllTables(restaurantId);
            var tablesEntites = _tableRepository.GetAllTablesByRestaurantAsync(restaurantId).Result;

            var slotsToTableMap = new Dictionary<string, List<Table>>();

            var currentdate = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd"); 
            var currentHour = int.Parse(DateTime.Now.ToLocalTime().ToString("HH")) + 2;

            
            foreach (var slot in timeSlots)
            {
                var availableTableList = new List<Table>();

                var slotDivided = slot.Split(":");
                var hours = int.Parse(slotDivided[0]);

                foreach (var table in tablesEntites)
                {
                    var tableReservations = table.Reservations.Where(r => r.StartTime == DateTime.Parse(reservationDate).AddHours(hours).ToUniversalTime() && r.Status != ReservationStatus.Cancelled);                    
                    if(tableReservations.Count() == 0 && (table.Area.ToString() == tableArea || tableArea is null) && (table.Smoking.ToString() == smokingArea || smokingArea is null))
                    {
                        availableTableList.Add(table);
                    }
                }                
                if(availableTableList.Count > 0 && availableTableList.Select(t=> t.NumberOfSeats).Sum() >= numberOfPersons && (DateTime.Parse(currentdate) < DateTime.Parse(reservationDate) || (currentdate == reservationDate && currentHour < hours)))
                {
                    slotsToTableMap.Add(slot, availableTableList);
                }
            }

            AvailableSlots = slotsToTableMap.Keys.ToList();

            if(reservationTime is not null)
            {
                var slotDivided = reservationTime.Split(":");
                var hours = int.Parse(slotDivided[0]);
                
                if(currentUserRole == "Customer")
                {
                    NewReservation.Customer = currentUser;
                    NewReservation.Status = ReservationStatus.Reserved;
                    NewReservation.ReservationDetails = currentUser.FirstName + ' ' + currentUser.LastName + ' ' + currentUser.Email + ' ' + currentUser.PhoneNumber;
                }
                else
                {
                    NewReservation.ReservationDetails = reservationDetails;
                    NewReservation.Status = ReservationStatus.Affirmed;
                }
                NewReservation.NumberOfPerson = numberOfPersons;
                NewReservation.Date = DateTime.Parse(reservationDate).AddHours(hours);
                NewReservation.Tables = new List<Table>();
                var capacity = 0;
                foreach (var table in slotsToTableMap[reservationTime])
                {
                    NewReservation.Tables.Add(table);
                    capacity = capacity + table.NumberOfSeats;
                    if (capacity >= numberOfPersons)
                        break;
                }
                try
                {
                    await _reservationService.AddAsync(NewReservation);
                    return RedirectToPage("ViewAllReservation");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
            return Page();
        }
    }
}
