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
        public IActionResult Details(int id, int pageNumber = 1)
        {
            if (id != 0)
            {
                Response.Cookies.Append("cinemaId", id.ToString());
            }
            if (id == 0)
            {
                id = int.Parse(Request.Cookies["cinemaId"]);
            }

            int itemsNum = 4;
            int totalMovies = movieRepository.GetMoviesByCinema(id).Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;
            var movies = movieRepository.GetMoviesByCinema(id).Skip((pageNumber - 1) * itemsNum).Take(itemsNum);
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
            if (ModelState.IsValid)
            {
                cinemaRepository.Add(cinema);
                return RedirectToAction("CinemaCRUD", "Dashboard");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
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
            if (ModelState.IsValid)
            {
                cinemaRepository.Update(cinema);
                return RedirectToAction("CinemaCRUD", "Dashboard");
            }
            return RedirectToAction("SomeThingWrong", "Errors");

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
