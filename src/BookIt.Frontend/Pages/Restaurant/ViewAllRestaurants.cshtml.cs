using BookIt.Application.Models.Restaurant;
using BookIt.Application.Models.User;
using BookIt.Application.Services;
using BookIt.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace BookIt.Frontend.Pages.Restaurant
{
    [Authorize]
    public class ViewAllRestaurantsModel : PageModel
    {
        public readonly IRestaurantService _restaurantService;
        public readonly UserManager<ApplicationUser> _userManager;
#nullable enable
        public PaginatedList<RestaurantModel> Restaurants { get; set; }

#nullable disable
        public List<string> RestaurantModelProperties;

        public int TotalPages { get; set; }

        public ViewAllRestaurantsModel(IRestaurantService restaurantService,
            UserManager<ApplicationUser> userManager)
        {
            _restaurantService = restaurantService;
            _userManager = userManager;
        }

        public void OnGet(string sort, string currentFilter, string keyword, int? pageIndex)
        {
            var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;

            if (keyword != null)
            {
                pageIndex = 1;
            }
            else
            {
                keyword = currentFilter;
            }

            ViewData["CurrentFilter"] = keyword;
            int pageSize = 4;

            var restaurants = _restaurantService.GetAllRestaurants();
            if (this.User.IsInRole("Manager"))
            {
                restaurants = restaurants.Where(r => r.ManagerId == currentUser.Id);
            }

            RestaurantModelProperties = _restaurantService.GetRestaurantModelFields();

            
            ViewData["CurrentSort"] = sort;
            restaurants = _restaurantService.RestaurantSorting(restaurants, sort);

            ViewData["Keyword"] = keyword;
            restaurants = _restaurantService.RestaurantSearch(restaurants, keyword);

            Restaurants = PaginatedList<RestaurantModel>.Create(restaurants, pageIndex ?? 1, pageSize);

            TotalPages = (int)Math.Ceiling(decimal.Divide(restaurants.Count(), pageSize));
        }
    }
}
