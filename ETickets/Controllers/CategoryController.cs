using ETickets.Data;
using ETickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETickets.Controllers
{
    public class CategoryController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();
        public IActionResult Index()
        {
            var categories = context.Categories.ToList();
            return View(categories);
        }
        public IActionResult Details(int id)
        {
            var movies = context.Movies.Include(e => e.Cinema).Include(m => m.Category).Where(c => c.Category.Id == id).ToList();
            return View(movies);
        }

        // Filter for the user so he can chose a film passed on his needs 
        public IActionResult Filter()
        {
            var categories = context.Categories.ToList();
            ViewBag.Categories = categories;
            var cinemas = context.Cinemas.ToList();
            ViewBag.Cinemas = cinemas;
            var movies = new List<Movie>(); // empty list
            
                return View(movies);
        }

        [HttpPost]
        public IActionResult Filter(int CategoryId, int CinemaId, MovieStatus AvailabilityId)
        {
            var movie = context.Movies.Include(e => e.Cinema).Include(m => m.Category)
                .Where(c => c.Category.Id == CategoryId && c.Cinema.Id == CinemaId && c.MovieStatus == AvailabilityId)
                .ToList();

            var categories = context.Categories.ToList();
            ViewBag.Categories = categories;

            var cinemas = context.Cinemas.ToList();
            ViewBag.Cinemas = cinemas;
            if (movie.Any())
            {
                // add the result in the same page
                return View(movie);
            }
            else
            {
                // If there is no movie have the user needs, send him the temp data
                TempData["NotFound"] = "Sorry we cant found Movie like that";
                return RedirectToAction("Filter");
            }
        }

    }
}
