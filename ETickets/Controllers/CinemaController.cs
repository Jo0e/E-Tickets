using ETickets.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETickets.Controllers
{
    public class Cinema : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();
        public IActionResult Index()
        {
            var cinemas = context.Cinemas.ToList();
            return View(cinemas);
        }
        public IActionResult Details(int id)
        {
            var movies = context.Movies.Include(m => m.Category).Include(e => e.Cinema).Where(c => c.Cinema.Id == id).ToList();
            return View(movies);
        }
    }
}
