using AutoMapper;
using BookIt.Application.Models.Dish;
using BookIt.Application.Services;
using Microsoft.AspNetCore.Authorization;
using BookIt.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookIt.Frontend.Pages.Dish
{

    [Authorize(Roles = "Administrator, Manager")]
    public class CreateDishModel : PageModel
    {
        private readonly IDishRepository _dishRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IDishService _dishService;
        public CreateDishModel(IDishRepository dishRepository,
            IMapper mapper,
            IUserService userService,
            IUserRepository userRepository,
            IDishService dishService)
        {
            _dishRepository = dishRepository;
            _mapper = mapper;
            _userService = userService;
            _userRepository = userRepository;
            _dishService = dishService;
            _dishService = dishService;
        }
        [BindProperty]
        public DishModelForCreate NewDish { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { return Page(); }

            try
            {
                await _dishService.AddAsync(NewDish);
                return RedirectToPage("ViewAllDishes");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Page();
        }
    }
}
