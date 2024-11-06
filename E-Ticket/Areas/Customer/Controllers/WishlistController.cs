using E_Ticket.Models;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticket.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = $"{SD.CustomerRole}, {SD.AdminRole}")]
    public class WishlistController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRepository<Wishlist> wishlistRepository;

        public WishlistController(UserManager<IdentityUser> userManager, IRepository<Wishlist> WishlistRepository)
        {
            this.userManager = userManager;
            wishlistRepository = WishlistRepository;

        }

        
        public async Task<IActionResult> AddToWishlist(int movieId)
        {
            
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var cartCheck = wishlistRepository.Get(where: e => e.MovieId == movieId && e.UserId == user.Id);
                if (!cartCheck.Any())
                {
                    var wishlistItem = new Wishlist { UserId = user.Id, MovieId = movieId };
                    wishlistRepository.Create(wishlistItem);
                    wishlistRepository.Commit();
                    TempData["success"] = $"Movie added to wishlist successfully!";
                    return Json(new { success = true, redirectTo = Url.Action("Index", "Home") });
                }
                return Json(new { success = false, message = "This movie is already in the wishlist." });
            }
            return NotFound();
        }

        public async Task<IActionResult> ViewWishlist()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var wishlist = wishlistRepository.Get(include: [m => m.Movie], where: w => w.UserId == user.Id);
                return View(wishlist);
            }
            return NotFound();
        }
        
        public IActionResult RemoveFromWishlist(int wishlistId)
        {
            var wishlistItem = wishlistRepository.GetOne(where: e=>e.WishlistId == wishlistId);
            if (wishlistItem != null)
            {
                wishlistRepository.Delete(wishlistItem);
            }
            wishlistRepository.Commit();

            return RedirectToAction("ViewWishlist");
        }

    }
}
