using ETickets.Data;
using ETickets.Models;
using ETickets.Repository;
using ETickets.Repository.IRepository;
using ETickets.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Permissions;

namespace ETickets.Controllers
{
    [Authorize(Roles = SD.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> categoryRepository;
        private readonly IRepository<Cinema> cinemaRepository;
        private readonly IMovieRepository movieRepository;

        public CategoryController(IRepository<Category> categoryRepository, IMovieRepository movieRepository, IRepository<Cinema> cinemaRepository)
        {
            this.categoryRepository = categoryRepository;
            this.movieRepository = movieRepository;
            this.cinemaRepository = cinemaRepository;
        }


        //ApplicationDbContext context = new ApplicationDbContext();
        [AllowAnonymous]
        public IActionResult Index()
        {
            //  var categories = context.Categories.ToList();
            var categories = categoryRepository.GetAll();
            return View(categories);
        }
        [AllowAnonymous]
        public IActionResult Details(int id, int pageNumber = 1)
        {
            if (id != 0)
            {
                Response.Cookies.Append("CategoryId", id.ToString());
            }
            if (id == 0)
            {
                id = int.Parse(Request.Cookies["CategoryId"]);
            }

            int itemsNum = 4;
            int totalMovies = movieRepository.GetMoviesByCategory(id).Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            var movies = movieRepository.GetMoviesByCategory(id).Skip((pageNumber - 1) * itemsNum).Take(itemsNum);
            return View(movies);
        }

        //Filter for the user so he can chose a film passed on his needs
        [AllowAnonymous]
        public IActionResult Filter()
        {

            //var categories = context.Categories.ToList();
            var categories = categoryRepository.GetAll();
            ViewBag.Categories = categories;


            //var cinemas = context.Cinemas.ToList();
            var cinemas = cinemaRepository.GetAll();
            ViewBag.Cinemas = cinemas;
            var movies = new List<Movie>(); // empty list

            return View(movies);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Filter(int CategoryId, int CinemaId, MovieStatus AvailabilityId)
        {
            //var movie = context.Movies.Include(e => e.Cinema).Include(m => m.Category)
            //    .Where(c => c.Category.Id == CategoryId && c.Cinema.Id == CinemaId && c.MovieStatus == AvailabilityId)
            //    .ToList();

            var movie = movieRepository.GetMoviesByFilter(CategoryId, CinemaId, AvailabilityId);
            var categories = categoryRepository.GetAll();
            ViewBag.Categories = categories;

            var cinemas = cinemaRepository.GetAll();
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


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                categoryRepository.Add(category);
                return RedirectToAction("CategoryCRUD", "Dashboard");
            }
            return RedirectToAction("SomeThingWrong", "Errors");

        }

        public IActionResult Edit(int id)
        {
            var category = categoryRepository.GetById(id);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepository.Update(category);
                return RedirectToAction("CategoryCRUD", "Dashboard");
            }

            return RedirectToAction("SomeThingWrong", "Errors");
        }
        public IActionResult Delete(int id)
        {
            var category = categoryRepository.GetById(id);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Category category)
        {
            categoryRepository.Delete(category);
            return RedirectToAction("CategoryCRUD", "Dashboard");
        }

    }
}
