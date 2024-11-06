using E_Ticket.Models;
using E_Ticket.Repository;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Ticket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class MovieController : Controller
    {
        private readonly IMovieRepository movieRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly ICinemaRepository cinemaRepository;
        private readonly IRepository<CinemaMovie> cinemaMovieRepository;
        private readonly IRepository<ActorMovie> actorMovieRepository;
        private readonly IActorRepository actorRepository;

        public MovieController(IMovieRepository movieRepository, ICategoryRepository categoryRepository,
            ICinemaRepository cinemaRepository, IRepository<CinemaMovie> CinemaMovieRepository,
            IRepository<ActorMovie> actorMovieRepository, IActorRepository actorRepository)
        {
            this.movieRepository = movieRepository;
            this.categoryRepository = categoryRepository;
            this.cinemaRepository = cinemaRepository;
            cinemaMovieRepository = CinemaMovieRepository;
            this.actorMovieRepository = actorMovieRepository;
            this.actorRepository = actorRepository;
        }


        public IActionResult Index(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<Movie> movies;
            if (Name == null)
                movies = movieRepository.Get(include: [c => c.Category]);
            else
                movies = movieRepository.Get(include: [c => c.Category],where: e=>e.Name.Contains(Name));

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

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(categoryRepository.Get(), "Id", "Name");
            ViewBag.Cinemas = new SelectList(cinemaRepository.Get(), "Id", "Name");
            ViewBag.MovieStatuses = new SelectList(Enum.GetValues(typeof(MovieStatus)));

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Movie movie, List<int> CinemasList, IFormFile PhotoUrl)
        {
            ModelState.Remove(nameof(PhotoUrl));
            ModelState.Remove(nameof(CinemasList));
            if (ModelState.IsValid)
            {
                if (movie.MovieStatus == MovieStatus.Expired)
                {
                    movie.TotalTickets = 0;
                    movie.TicketsSold = 0;
                }
                movieRepository.CreateWithImage(movie, PhotoUrl, "movies", "ImgUrl");
                foreach (var item in CinemasList)
                {
                    cinemaMovieRepository.Create(new CinemaMovie { CinemaId = item, MovieId = movie.Id });
                    cinemaMovieRepository.Commit();
                }

                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }


        public IActionResult Edit(int movieId)
        {
            ViewBag.Categories = new SelectList(categoryRepository.Get(), "Id", "Name");
            ViewBag.Cinemas = new SelectList(cinemaRepository.Get(), "Id", "Name");
            ViewBag.MovieStatuses = new SelectList(Enum.GetValues(typeof(MovieStatus)));
            var movie = movieRepository.GetDetails(movieId);



            var selectedCinemas = movie.CinemaMovies.Select(cm => cm.CinemaId).ToList();
            ViewBag.SelectedCinemas = selectedCinemas;

            return View(movie);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Movie movie, List<int> CinemasList, IFormFile PhotoUrl)
        {
            ModelState.Remove(nameof(PhotoUrl));
            ModelState.Remove(nameof(CinemasList));
            if (ModelState.IsValid)
            {
                var oldMovie = movieRepository.GetOne(where: e=>e.Id == movie.Id);
                if (movie.MovieStatus == MovieStatus.Expired)
                {
                    movie.TotalTickets = 0;
                    movie.TicketsSold = 0;

                }
                movieRepository.UpdateImage(movie, PhotoUrl, oldMovie.ImgUrl, "movies", "ImgUrl");


                var toDelete = cinemaMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
                if (toDelete != null)
                {
                    foreach (var item in toDelete)
                    {
                        cinemaMovieRepository.Delete(item);
                    }
                }

                foreach (var cinemaId in CinemasList)
                {
                    cinemaMovieRepository.Create(new CinemaMovie { CinemaId = cinemaId, MovieId = movie.Id });
                }
                cinemaMovieRepository.Commit();
                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }

        public IActionResult Delete(int movieId)
        {
            var movie = movieRepository.GetDetails(movieId);
            return View(movie);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Movie movie)
        {
            var cinemaToDelete = cinemaMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
            if (cinemaToDelete != null)
            {
                foreach (var cinema in cinemaToDelete)
                {
                    cinemaMovieRepository.Delete(cinema);
                }
            }

            var actorToDelete = actorMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
            if (actorToDelete != null)
            {
                foreach (var actor in actorToDelete)
                {
                    actorMovieRepository.Delete(actor);
                }
            }
            movieRepository.Delete(movie);
            movieRepository.Commit();
            return RedirectToAction("Index");
        }



        public IActionResult AssignActor(int movieId)
        {
            var movie = movieRepository.GetDetails(movieId);

            var allActors = actorRepository.Get().ToList();
            var assignedActors = movie.ActorMovies.Select(am => am.ActorId).ToList();

            ViewBag.AllActors = new SelectList(allActors, "Id", "FullName");
            ViewBag.AssignedActors = assignedActors;

            return View(movie);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignActor(int Id, List<int> AssignedActors)
        {
            var movie = movieRepository.GetOne(where:w=>w.Id == Id);
             
            if (movie == null)
            {
                return NotFound();
            }
            var toDelete = actorMovieRepository.Get(where: e => e.MovieId == movie.Id, tracked: false);
            if (toDelete != null)
            {
                foreach (var item in toDelete)
                {
                    actorMovieRepository.Delete(item);
                }
            }

            foreach (var actorId in AssignedActors)
            {
                actorMovieRepository.Create(new ActorMovie { ActorId = actorId, MovieId = movie.Id });
            }

            actorMovieRepository.Commit();

            return RedirectToAction("Index");
        }






    }
}
