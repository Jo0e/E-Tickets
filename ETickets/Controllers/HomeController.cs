using ETickets.Data;
using ETickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ETickets.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var movies = context.Movies.Include(e=>e.Category).Include(e=>e.Cinema).ToList();
            return View(movies);
        }

        public IActionResult Details(int id)
        {
            var actors =  context.ActorMovies.Where(e => e.MovieId == id).Include(e=>e.Actor).ToList();
            ViewBag.Actors = actors;

            var details = context.Movies.Include(e => e.Category).Include(e => e.Cinema).Where(w => w.Id == id).FirstOrDefault();
            return View(details);
        }
        [HttpPost]
        public IActionResult Search(string Name) 
        {
            var movies = context.Movies.Where(m=>m.Name.Contains(Name)).Include(e => e.Category).Include(e => e.Cinema).ToList();
            if (movies.Any())
            {
                return View(movies);
            }
            else 
            {
                return RedirectToAction("SearchNotFound", "Errors");
            }
        } 

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
