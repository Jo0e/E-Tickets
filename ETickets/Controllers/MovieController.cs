using ETickets.Models;
using ETickets.Repository.IRepository;
using ETickets.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETickets.Controllers
{
    [Authorize(Roles = SD.AdminRole)]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> movieRepository;
        private readonly IRepository<Category> categoryRepository;
        private readonly IRepository<Cinema> cinemaRepository;

        public MovieController(IRepository<Movie> movieRepository , IRepository<Category> categoryRepository, IRepository<Cinema> cinemaRepository)
        {
            this.movieRepository = movieRepository;
            this.categoryRepository = categoryRepository;
            this.cinemaRepository = cinemaRepository;
        }
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(categoryRepository.GetAll(), "Id", "Name");
            ViewBag.Cinemas =new SelectList( cinemaRepository.GetAll(),"Id","Name");
            ViewBag.MovieStatuses = new SelectList(Enum.GetValues(typeof(MovieStatus)));

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Movie movie, IFormFile PhotoUrl)
        {
            movieRepository.CreateWithImage(movie, PhotoUrl, "movies", "ImgUrl");
            return RedirectToAction("MovieCRUD", "Dashboard");
        }

        public IActionResult Edit(int id) 
        {
            ViewBag.Categories = new SelectList(categoryRepository.GetAll(), "Id", "Name");
            ViewBag.Cinemas = new SelectList(cinemaRepository.GetAll(), "Id", "Name");
            ViewBag.MovieStatuses = new SelectList(Enum.GetValues(typeof(MovieStatus)));
            var movie = movieRepository.GetById(id);
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Movie movie, IFormFile PhotoUrl) 
        {
            var oldActor = movieRepository.GetById(movie.Id);
            movieRepository.UpdateImage(movie, PhotoUrl, oldActor.ImgUrl, "movies", "ImgUrl");
            return RedirectToAction("MovieCRUD", "Dashboard");
        }
        public IActionResult Delete(int id) 
        {
            var movie = movieRepository.GetById(id);
            ViewBag.Categories = new SelectList(categoryRepository.GetAll());
            ViewBag.Cinemas = new SelectList(cinemaRepository.GetAll());
            return View(movie);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Movie movie) 
        {
            movieRepository.Delete(movie);
            return RedirectToAction("MovieCRUD", "Dashboard");
        }

    }

}
