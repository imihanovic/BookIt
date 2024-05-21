using AutoMapper;
using BookIt.Application.Models.Reservation;
using BookIt.Core.Entities;
using BookIt.DataAccess.Repositories;

namespace BookIt.Application.Services.Impl
{
    public class ReservationService : IReservationService
    {
        private readonly IMapper _mapper;
        private readonly IReservationRepository _reservationRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;

        public ReservationService(IMapper mapper,
            IReservationRepository reservationRepository,
            ITableRepository tableRepository,
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository)
        {
            _mapper = mapper;
            _reservationRepository = reservationRepository;
            _tableRepository = tableRepository;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
        }
        public async Task<Reservation> AddAsync(ReservationModelForCreate createReservationModel)
        {
            var reservation = new Reservation();

            reservation.Customer = createReservationModel.Customer;
            reservation.NumberOfPersons = createReservationModel.NumberOfPerson;
            reservation.StartTime = createReservationModel.Date.ToUniversalTime();
            reservation.Tables = createReservationModel.Tables;
            reservation.Status = createReservationModel.Status;
            reservation.ReservationDetails = createReservationModel.ReservationDetails;

            reservation.EndTime = createReservationModel.Date.AddHours(2).ToUniversalTime();
            await _reservationRepository.AddAsync(reservation);
            return reservation;
        }

        public ReservationModel GetAreaAndSmokingForReservation (ReservationModel reservationDto, Reservation reservation)
        {
             reservationDto.TableArea = reservation.Tables.FirstOrDefault().Area.ToString();
            reservationDto.SmokingArea = reservation.Tables.FirstOrDefault().Smoking.ToString();

            return reservationDto;
        }

        public IEnumerable<ReservationModel> MapAndAddAllReservations(IEnumerable<Reservation> filteredReservations)
        {
            List<ReservationModel> reservations = new List<ReservationModel>();
            foreach (var reservation in filteredReservations)
            {
                var reservationDto = _mapper.Map<ReservationModel>(reservation);
                reservationDto.StartTime = DateTime.Parse(reservationDto.StartTime).ToString("yyyy-MM-dd  HH:mm");
                reservationDto.RestaurantName = reservation.Tables.FirstOrDefault().Restaurant.RestaurantName;
                reservationDto.Address = reservation.Tables.FirstOrDefault().Restaurant.Address;
                reservationDto = GetAreaAndSmokingForReservation(reservationDto, reservation);
                reservations.Add(reservationDto);
            }
            return reservations.AsEnumerable();
        }
        public async Task CheckReservationStatus(IEnumerable<ReservationModel> reservations)
        {
            var currentDate = DateTime.Now.AddHours(2);
            foreach (var reservation in reservations)
            {
                var endTimeDate = DateTime.Parse(reservation.EndTime);
                if (currentDate >= endTimeDate)
                {
                    var reservationInDatabase = _reservationRepository.GetReservationByIdAsync(reservation.Id).Result;
                    reservationInDatabase.Status = Core.Enums.ReservationStatus.Done;
                    await _reservationRepository.UpdateAsync(reservationInDatabase);
                }
            }
        }
        public IEnumerable<ReservationModel> GetAllReservations()
        {
            var reservationsFromDatabase = _reservationRepository.GetAllReservationsAsync().Result;
            return MapAndAddAllReservations(reservationsFromDatabase);
        }
        public IEnumerable<ReservationModel> GetAllReservationsForManager(string managerId)
        {
            var reservationsFromDatabase = _reservationRepository.GetAllReservationsAsync().Result.Where(r => r.Tables.First().Restaurant.ManagerId == managerId);
            return MapAndAddAllReservations(reservationsFromDatabase);
        }
        public IEnumerable<ReservationModel> GetAllReservationsForCustomer(string customerId)
        {
            var reservationsFromDatabase = _reservationRepository.GetAllReservationsAsync().Result.Where(r => (r.Customer != null && r.Customer.Id == customerId));
            return MapAndAddAllReservations(reservationsFromDatabase);
        }

        public IEnumerable<ReservationModel> GetAllReservationsByRestaurant(string restaurantId)
        {
            var reservationsFromDatabase = _reservationRepository.GetAllReservationsAsync().Result.Where(r => r.Tables.FirstOrDefault().Restaurant.Id.ToString() == restaurantId);
            return MapAndAddAllReservations(reservationsFromDatabase);
        }

        public Reservation EditReservationAsync(ReservationModelForUpdate reservationModel)
        {
            var reservation = _mapper.Map<Reservation>(reservationModel);
            return _reservationRepository.UpdateAsync(reservation).Result;
        }
        
        public List<string> GetReservationModelFields(string userRole)
        {
            var reservationDto = new ReservationModel();
            if(userRole == "Customer" || userRole == "Administrator")
            {
                return reservationDto.GetType().GetProperties().Where(x => x.Name != "Id" && x.Name != "EndTime" && x.Name != "Customer").Select(x => x.Name).ToList();
            }
            return reservationDto.GetType().GetProperties().Where( x => x.Name != "Id" && x.Name != "EndTime" && x.Name != "Customer" && x.Name != "RestaurantName" && x.Name != "Address").Select(x => x.Name).ToList();
        }

        public IEnumerable<ReservationModel> ReservationSorting(IEnumerable<ReservationModel> reservations, string sortOrder)
        {
            IEnumerable<ReservationModel> sortedReservations = reservations;
            switch (sortOrder)
            {
                case "NumberOfPersons":
                    return reservations.OrderBy(r => r.NumberOfPersons);
                case "NumberOfPersonsDesc":
                    return reservations.OrderByDescending(r => r.NumberOfPersons);
                case "StartTime":
                    return reservations.OrderBy(r => r.StartTime);
                case "StartTimeDesc":
                    return reservations.OrderByDescending(r => r.StartTime);
                default:
                    return reservations.OrderByDescending(r => r.StartTime);
            }
        }
        public IEnumerable<ReservationModel> ReservationFilterByStatus(IEnumerable<ReservationModel> reservations, string status)
        {
            IEnumerable<ReservationModel> filteredReservations= reservations;
            if (!String.IsNullOrEmpty(status))
            {
                var statusTrim = status.ToLower().Trim();
                filteredReservations = reservations.Where(t => t.Status.ToLower() == statusTrim);
            }
            return filteredReservations;
        }

         public IEnumerable<ReservationModel> ReservationFilterByReservationDate(IEnumerable<ReservationModel> reservations, string reservationDate)
         {
            IEnumerable<ReservationModel> filteredReservations= reservations;
            if (!String.IsNullOrEmpty(reservationDate))
            {
                var reservationDateTrim = reservationDate.ToLower().Trim();
                filteredReservations = reservations.Where(t => t.StartTime.ToLower().Contains(reservationDateTrim));
            }
            return filteredReservations;
         }

        public IEnumerable<ReservationModel> ReservationFilterByReservationTime(IEnumerable<ReservationModel> reservations, string reservationTime)
        {
            IEnumerable<ReservationModel> filteredReservations = reservations;
            if (!String.IsNullOrEmpty(reservationTime))
            {
                var reservationTimeTrim = reservationTime.ToLower().Trim();
                filteredReservations = reservations.Where(t => t.StartTime.ToLower().Contains(reservationTimeTrim));
            }
            return filteredReservations;
        }

        public IEnumerable<ReservationModel> ReservationFilterByTableArea(IEnumerable<ReservationModel> reservations, string tableArea)
        {
            IEnumerable<ReservationModel> filteredReservations = reservations;
            if (!String.IsNullOrEmpty(tableArea))
            {
                var tableAreaTrim = tableArea.ToLower().Trim();
                filteredReservations = reservations.Where(t => t.TableArea.ToLower() == tableAreaTrim);
            }
            return filteredReservations;
        }

        public IEnumerable<ReservationModel> ReservationFilterBySmokingArea(IEnumerable<ReservationModel> reservations, string smokingArea)
        {
            IEnumerable<ReservationModel> filteredReservations = reservations;
            if (!String.IsNullOrEmpty(smokingArea))
            {
                var smokingAreaTrim = smokingArea.ToLower().Trim();
                filteredReservations = reservations.Where(t => t.SmokingArea.ToLower() == smokingAreaTrim);
            }
            return filteredReservations;
        }

        public IEnumerable<ReservationModel> ReservationSearch(IEnumerable<ReservationModel> reservations, string searchString)
        {
            IEnumerable<ReservationModel> searchedReservations = reservations;
            if (!String.IsNullOrEmpty(searchString))
            {
                var searchStrTrim = searchString.ToLower();
                searchedReservations = reservations.Where(s => s.NumberOfPersons == searchStrTrim
                                        || (!string.IsNullOrWhiteSpace(s.ReservationDetails) && s.ReservationDetails.ToLower().Contains(searchStrTrim)));
            }
            return searchedReservations;
        }
    }
}
