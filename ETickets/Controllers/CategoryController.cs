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
        public IActionResult Details(int id)
        {
            //var movies = context.Movies.Include(e => e.Cinema).Include(m => m.Category).Where(c => c.Category.Id == id).ToList();
            var movies = movieRepository.GetMoviesByCategory(id);
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
            categoryRepository.Add(category);
            return RedirectToAction("CategoryCRUD", "Dashboard");
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
            categoryRepository.Update(category);
            return RedirectToAction("CategoryCRUD", "Dashboard");
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
