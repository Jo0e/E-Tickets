using ETickets.Data;
using ETickets.Models;
using ETickets.Repository.IRepository;
using ETickets.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETickets.Controllers
{
    [Authorize(Roles = SD.AdminRole)]
    public class CinemaController : Controller
    {
        public CinemaController(IRepository<Cinema> cinemaRepository, IMovieRepository movieRepository)
        {
            this.cinemaRepository = cinemaRepository;
            this.movieRepository = movieRepository;
        }

        //ApplicationDbContext context = new ApplicationDbContext();
        private readonly IRepository<Cinema> cinemaRepository;
        private readonly IMovieRepository movieRepository;

        [AllowAnonymous]
        public IActionResult Index()
        {
            //    var cinemas = context.Cinemas.Select(e=> new Cinema {Id = e.Id,Name = e.Name}).ToList();
            var cinemas = cinemaRepository.GetAll();
            return View(cinemas);
        }
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            // var movies = context.Movies.Include(m => m.Category).Include(e => e.Cinema).Where(c => c.Cinema.Id == id).ToList();
            var movies = movieRepository.GetMoviesByCinema(id);
            return View(movies);
        }



        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cinema cinema)
        {
            cinemaRepository.Add(cinema);
            return RedirectToAction("CinemaCRUD", "Dashboard");
        }

        public IActionResult Edit(int id)
        {
            var cinema = cinemaRepository.GetById(id);
            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cinema cinema)
        {
            cinemaRepository.Update(cinema);
            return RedirectToAction("CinemaCRUD", "Dashboard");

        }

        public IActionResult Delete(int id)
        {
            var cinema = cinemaRepository.GetById(id);
            return View(cinema);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Cinema cinema)
        {
            cinemaRepository.Delete(cinema);
            return RedirectToAction("CinemaCRUD", "Dashboard");
        }

    }
}
