using E_Ticket.Models;
using E_Ticket.Repository;
using E_Ticket.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace E_Ticket.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMovieRepository movieRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly ICinemaRepository cinemaRepository;
        private readonly IRepository<CinemaMovie> cinemaMovieRepository;
        private readonly IRepository<Actor> actorRepository;
        private readonly IRepository<ActorMovie> actorMovieRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRepository<Wishlist> wishlistRepository;

        public HomeController(ILogger<HomeController> logger, IMovieRepository movieRepository,
            ICategoryRepository categoryRepository, ICinemaRepository cinemaRepository,
            IRepository<CinemaMovie> cinemaMovieRepository, IRepository<Actor> actorRepository,
            IRepository<ActorMovie> actorMovieRepository,
            UserManager<IdentityUser> userManager, IRepository<Wishlist> wishlistRepository)
        {
            _logger = logger;
            this.movieRepository = movieRepository;
            this.categoryRepository = categoryRepository;
            this.cinemaRepository = cinemaRepository;
            this.cinemaMovieRepository = cinemaMovieRepository;
            this.actorRepository = actorRepository;
            this.actorMovieRepository = actorMovieRepository;
            this.userManager = userManager;
            this.wishlistRepository = wishlistRepository;
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            int itemsNum = 8;
            int totalMovies = movieRepository.Get().Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            await SetUserWishlist();

            var movies = movieRepository.Get([q => q.Category]).Skip((pageNumber - 1) * itemsNum).Take(itemsNum);

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            return View(movies);
        }

        public async Task<IActionResult> Details(int movieId)
        {
            var movie = movieRepository.GetDetails(movieId);
            if (movie != null)
            {
                await SetUserWishlist();
                return View(movie);
            }
            return RedirectToAction("NotFound", "Errors");
        }

        [HttpGet]
        // To get the search list with the JS to work 
        public JsonResult SearchMovies(string name)
        {
            var movies = movieRepository.SearchMovies(name)
                       .Select(m => new { m.Id, m.Name, m.ImgUrl })
                       .Take(3) // Show only the first 3 results
                       .ToList();
            return Json(movies);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string Name)
        {
            await SetUserWishlist();

            var movies = movieRepository.SearchMovies(Name);
            if (movies.Any())
            {
                return View(movies);
            }
            else
            {
                return RedirectToAction("SearchNotFound", "Errors");
            }
        }

        public IActionResult Categories()
        {
            var categories = categoryRepository.Get();
            return View(categories);
        }

        public IActionResult CategoryMovies(int categoryId, int pageNumber = 1)
        {
            if (categoryId != 0)
            {
                Response.Cookies.Append("CategoryId", categoryId.ToString());
            }
            if (categoryId == 0)
            {
                categoryId = int.Parse(Request.Cookies["CategoryId"]);
            }

            int itemsNum = 4;
            int totalMovies = movieRepository.Get(include: [c => c.Category],
                where: e => e.CategoryId == categoryId).Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            var movies = movieRepository.Get(include: [c => c.Category],
                where: e => e.CategoryId == categoryId)
                .Skip((pageNumber - 1) * itemsNum).Take(itemsNum);

            return View(movies);
        }


        public IActionResult Cinemas()
        {
            var cinemas = cinemaRepository.Get();
            return View(cinemas);
        }

        public IActionResult CinemaMovies(int CinemaId, int pageNumber = 1)
        {
            if (CinemaId != 0)
            {
                Response.Cookies.Append("cinemaId", CinemaId.ToString());
            }
            if (CinemaId == 0)
            {
                CinemaId = int.Parse(Request.Cookies["cinemaId"]);
            }

            int itemsNum = 4;
            int totalMovies = cinemaMovieRepository.Get(include: [m => m.Movie, c => c.Cinema], 
                where: e => e.CinemaId == CinemaId).Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            var movies = cinemaMovieRepository.Get(include: [m => m.Movie, c => c.Cinema], 
                where: e => e.CinemaId == CinemaId);
            return View(movies);
        }

        public IActionResult Filter()
        {

            var categories = categoryRepository.Get(tracked: false);
            ViewBag.Categories = categories;

            var cinemas = cinemaRepository.Get(tracked: false);
            ViewBag.Cinemas = cinemas;
            var movies = new List<Movie>();

            return View(movies);
        }

        [HttpPost]
        public IActionResult Filter(int CategoryId, int CinemaId, MovieStatus AvailabilityId)
        {

            var movie = movieRepository.GetMoviesByFilter(CategoryId, CinemaId, AvailabilityId);
            var categories = categoryRepository.Get(tracked: false);
            ViewBag.Categories = categories;

            var cinemas = cinemaRepository.Get(tracked: false);
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


        public IActionResult Actor(int actorId)
        {
            var actor = actorRepository.GetOne(where: a => a.Id == actorId);
            return View(actor);
        }

        public IActionResult ActorMovies(int actorId)
        {
            var movie = movieRepository.Get(include: [a => a.Category, m => m.ActorMovies],
                where: e => e.ActorMovies.Any(a => a.ActorId == actorId));
            return View(movie);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        // Method to tell the JS what movie in the wishlist and what not
        private async Task SetUserWishlist()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var wishlist = wishlistRepository.Get(include: [m => m.Movie], where: w => w.UserId == user.Id).ToList();
                ViewBag.Wishlist = wishlist.Select(w => w.MovieId).ToList();
            }
        }


    }
}
