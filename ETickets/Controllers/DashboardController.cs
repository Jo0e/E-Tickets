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

        public IActionResult MovieCRUD() 
        {
            var movies = movieRepository.GetAll("Category", "Cinema");

            return View(movies);
        }

        public IActionResult CinemaCRUD()
        {
            var cinema = cinemaRepository.GetAll();

            return View(cinema);
        }

        public IActionResult CategoryCRUD()
        {
            var category = categoryRepository.GetAll();

            return View(category);
        }
    }
}
