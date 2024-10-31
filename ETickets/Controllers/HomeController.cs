using ETickets.Data;
using ETickets.Models;
using ETickets.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace ETickets.Controllers
{
    public class HomeController : Controller
    {
        
        public HomeController(IMovieRepository myMovieRepository, ILogger<HomeController> logger,
            IRepository<Movie> movieRepository, UserManager<ApplicationUser> userManager, IRepository<Wishlist> WishlistRepository)
        {
            
            this.myMovieRepository = myMovieRepository;
            _logger = logger;
            this.movieRepository = movieRepository;
            this.userManager = userManager;
            wishlistRepository = WishlistRepository;
        }
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Movie> movieRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepository<Wishlist> wishlistRepository;
        private readonly IMovieRepository myMovieRepository;

        

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            int itemsNum = 8;
            int totalMovies = movieRepository.GetAll().Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var wishlist = wishlistRepository.GetWithIncludes(w => w.UserId == user.Id, "Movie").ToList();
                ViewBag.Wishlist = wishlist.Select(w => w.MovieId).ToList();
            }

            
            var movies = movieRepository.GetAll("Category", "Cinema").Skip((pageNumber - 1) * itemsNum).Take(itemsNum);

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            return View(movies);

        }

        public async Task<IActionResult> Details(int id)
        {            
            var details = myMovieRepository.GetMovieDetails(id);
            if (details != null)
            {
                var user = await userManager.GetUserAsync(User);
                if (user != null)
                {
                    var wishlist = wishlistRepository.GetWithIncludes(w => w.UserId == user.Id, "Movie").ToList();
                    ViewBag.Wishlist = wishlist.Select(w => w.MovieId).ToList();
                }
                return View(details);
            }
            return RedirectToAction("NotFound", "Errors");

        }

        // This is to make the search show the first 3 results in a list without pressing the search btn 
        [HttpGet]
        public JsonResult SearchMovies(string name)
        {
            var movies = myMovieRepository.SearchMovies(name)
                       .Select(m => new { m.Id, m.Name, m.ImgUrl })
                       .Take(3) // Show only the first 3 results
                       .ToList();
            return Json(movies);
        }


        [HttpPost]
        public async Task<IActionResult> Search(string Name)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var wishlist = wishlistRepository.GetWithIncludes(w => w.UserId == user.Id, "Movie").ToList();
                ViewBag.Wishlist = wishlist.Select(w => w.MovieId).ToList();
            }
            var movies = myMovieRepository.SearchMovies(Name);
            //context.Movies.Where(m => m.Name.Contains(Name)).Include(e => e.Category).Include(e => e.Cinema).ToList();
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
