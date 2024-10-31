using ETickets.Models;
using ETickets.Repository.IRepository;
using ETickets.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETickets.Controllers
{
    public class WishlistController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepository<Wishlist> wishlistRepository;

        public WishlistController(UserManager<ApplicationUser> userManager, IRepository<Wishlist> WishlistRepository)
        {
            this.userManager = userManager;
            wishlistRepository = WishlistRepository;

        }

        [Authorize(Roles = $"{SD.CustomerRole}, {SD.AdminRole}")]
        
        public async Task<IActionResult> AddToWishlist(int movieId)
        {
            
            var user = await userManager.GetUserAsync(User);
            var check = wishlistRepository.GetWithIncludes(e => e.MovieId == movieId && e.UserId == user.Id);
            if (!check.Any())
            {
                var wishlistItem = new Wishlist { UserId = user.Id, MovieId = movieId };
                wishlistRepository.Add(wishlistItem);
                TempData["success"] = $"Movie added to wishlist successfully!";
                return Json(new { success = true, redirectTo = Url.Action("Index", "Home") });
            }
            return Json(new { success = false, message = "This movie is already in the wishlist." });
        }

        public async Task<IActionResult> ViewWishlist()
        {
            var user = await userManager.GetUserAsync(User);
            var wishlist = wishlistRepository.GetWithIncludes(w => w.UserId == user.Id, "Movie").ToList();
            return View(wishlist);
        }
        
        public async Task<IActionResult> RemoveFromWishlist(int wishlistId)
        {
            var wishlistItem = wishlistRepository.GetById(wishlistId);
            if (wishlistItem != null)
            {
                wishlistRepository.Delete(wishlistItem);
            }

            return RedirectToAction("ViewWishlist");
        }

    }
}
