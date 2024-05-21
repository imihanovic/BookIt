using BookIt.Application.Models.Dish;
using BookIt.Application.Models.User;
using BookIt.Application.Services;
using BookIt.Core.Entities.Identity;
using BookIt.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookIt.Frontend.Pages.Dish
{
    [Authorize(Roles = "Administrator, Manager")]
    public class ViewAllDishesModel : PageModel
    {
        public readonly IDishService _dishService;
        public readonly IDishRepository _dishRepository;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly IRestaurantRepository _restaurantRepository;
#nullable enable
        public PaginatedList<DishModel> Dishes { get; set; }
#nullable disable
        public List<string> DishModelProperties;

        public int TotalPages { get; set; }

        public ViewAllDishesModel(IDishService dishService,
            UserManager<ApplicationUser> userManager,
            IDishRepository dishRepository,
            IRestaurantRepository restaurantRepository)
        {
            _dishService = dishService;
            _userManager = userManager;
            _dishRepository = dishRepository;
            _restaurantRepository = restaurantRepository;
        }

        public void OnGet(string currentFilter, string keyword, string categoryFilter, int? pageIndex)
        {
            var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;
            var currentUserRole = _userManager.GetRolesAsync(currentUser).Result.First();

            if (keyword != null)
            {
                pageIndex = 1;
            }
            else
            {
                keyword = currentFilter;
            }

            DishModelProperties = _dishService.GetDishModelFields();

            ViewData["CurrentFilter"] = keyword;
            int pageSize = 4;

            var dishes = _dishService.GetAllDishes();

            if(currentUserRole == "Manager")
            {

                var restaurant = _restaurantRepository.GetRestaurantByManagerIdAsync(currentUser.Id).Result;
                if (restaurant != null)
                {
                    dishes = _dishService.GetAllDishesNotOnTheMenu(dishes, restaurant.Id.ToString());
                }
            }

            ViewData["Keyword"] = keyword;
            dishes = _dishService.DishSearch(dishes, keyword);

            ViewData["CategoryFilter"] = categoryFilter;
            dishes = _dishService.DishFilter(dishes, categoryFilter);

            Dishes = PaginatedList<DishModel>.Create(dishes, pageIndex ?? 1, pageSize);

            TotalPages = (int)Math.Ceiling(decimal.Divide(dishes.Count(), pageSize));

        }
        public IActionResult OnPostDelete(Guid id)
        {
            _dishService.DeleteDishAsync(id);
            return RedirectToPage("ViewAllDishes");
        }
    }
}
