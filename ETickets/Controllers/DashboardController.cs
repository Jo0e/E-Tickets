using ETickets.Models;
using ETickets.Repository.IRepository;
using ETickets.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETickets.Controllers
{

    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private readonly IRepository<Movie> movieRepository;
        private readonly IRepository<Cinema> cinemaRepository;
        private readonly IRepository<Category> categoryRepository;

        public DashboardController(IRepository<Movie> movieRepository, IRepository<Cinema> cinemaRepository, IRepository<Category> categoryRepository)
        {
            this.movieRepository = movieRepository;
            this.cinemaRepository = cinemaRepository;
            this.categoryRepository = categoryRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MovieCRUD(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<Movie> movies;
            if (Name == null)

                movies = movieRepository.GetAll("Category", "Cinema");

            else
                movies = movieRepository.GetWithIncludes(filter: e => e.Name.Contains(Name), "Category", "Cinema");

            if (!movies.Any())
            {
                TempData["NotFound"] = "Sorry we cant found the Movie try again";
                return RedirectToAction("MovieCRUD");
            }
            int itemsNum = 8;
            int totalMovies = movies.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);

            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;
            return View(movies.Skip((pageNumber - 1) * itemsNum).Take(itemsNum));
        }

        public IActionResult CinemaCRUD(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<Cinema> cinema;
            if (Name == null)
                cinema = cinemaRepository.GetAll();

            else
                cinema = cinemaRepository.GetWithIncludes(filter: e => e.Name.Contains(Name));

            if (!cinema.Any())
            {
                TempData["NotFound"] = "Sorry we cant found the cinema try again";
                return RedirectToAction("CinemaCRUD");
            }

            int itemsNum = 8;
            int totalMovies = cinema.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;
            return View(cinema.Skip((pageNumber - 1) * itemsNum).Take(itemsNum));

        }

        public IActionResult CategoryCRUD(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<Category> category;
            if (Name == null)
                category = categoryRepository.GetAll();

            else
                category = categoryRepository.GetWithIncludes(filter: e => e.Name.Contains(Name));

            if (!category.Any())
            {
                TempData["NotFound"] = "Sorry we cant found the category try again";
                return RedirectToAction("CategoryCRUD");
            }
            int itemsNum = 8;
            int totalMovies = category.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;
            return View(category.Skip((pageNumber - 1) * itemsNum).Take(itemsNum));

        }
    }
}
