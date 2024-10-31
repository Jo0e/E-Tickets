using ETickets.Data;
using ETickets.Models;
using ETickets.Repository.IRepository;
using ETickets.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETickets.Controllers
{
    [Authorize(Roles = SD.AdminRole)]
    public class ActorsController : Controller
    {
        private readonly IRepository<Actor> actorRepository;
        private readonly IRepository<ActorMovie> actorMovieRepository;
        private readonly IRepository<Movie> movieRepository;

        //ApplicationDbContext context= new ApplicationDbContext();
        public ActorsController(IRepository<Actor> actorRepository, IRepository<ActorMovie> actorMovieRepository, IRepository<Movie> movieRepository)
        {
            this.actorRepository = actorRepository;
            this.actorMovieRepository = actorMovieRepository;
            this.movieRepository = movieRepository;
        }
        [AllowAnonymous]
        public IActionResult Index(int id)
        {
            //  var actor = context.Actors.FirstOrDefault(x => x.Id == id);
            var actor = actorRepository.GetById(id);
            if (actor != null)
            {
                return View(actor);
            }
            return RedirectToAction("NotFound", "Errors");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Actor actor, IFormFile PhotoUrl)
        {
            ModelState.Remove(nameof(PhotoUrl));
            if (ModelState.IsValid)
            {
                actorRepository.CreateWithImage(actor, PhotoUrl, "cast", "ProfilePicture");
                return RedirectToAction(nameof(AllActors));
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }

        public IActionResult Edit(int id)
        {

            var actor = actorRepository.GetById(id);
            return View(actor);


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Actor actor, IFormFile PhotoUrl)
        {
            ModelState.Remove(nameof(PhotoUrl));
            if (ModelState.IsValid)
            {
                var oldActor = actorRepository.GetById(actor.Id);
                actorRepository.UpdateImage(actor, PhotoUrl, oldActor.ProfilePicture, "cast", "ProfilePicture");
                return RedirectToAction("AllActors");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }
        public IActionResult Delete(int id)
        {
            var actor = actorRepository.GetById(id);
            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Actor actor)
        {
            actorRepository.Delete(actor);
            return RedirectToAction("AllActors");
        }

        [AllowAnonymous]
        public IActionResult ActorMovies(int actorId)
        {
            var movie = actorMovieRepository.GetWithIncludes(e => e.ActorId == actorId, "Actor", "Movie").ToList();
            return View(movie);
        }

        public IActionResult AllActors(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<Actor> actors;
            if (Name == null)
                actors = actorRepository.GetAll();
            else
                actors = actorRepository.GetWithIncludes(filter: e => e.FirstName.Contains(Name) || e.LastName.Contains(Name));


            if (!actors.Any())
            {
                TempData["NotFound"] = "Sorry we cant found the Actor try again";
                return RedirectToAction("AllActors");
            }
            int itemsNum = 8;
            int totalMovies = actors.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            return View(actors.Skip((pageNumber - 1) * itemsNum).Take(itemsNum));

        }

        public IActionResult AssignActors(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<ActorMovie> actors;
            if (Name == null)
                actors = actorMovieRepository.GetAll("Actor", "Movie");
            else
                actors = actorMovieRepository.GetWithIncludes(filter: e => e.Actor.FirstName.Contains(Name) || e.Actor.LastName.Contains(Name) || e.Movie.Name.Contains(Name), "Actor", "Movie");

            if (!actors.Any())
            {
                TempData["NotFound"] = "Sorry we cant find a thing try again";
                return RedirectToAction("AssignActors");
            }

            int itemsNum = 8;
            int totalMovies = actors.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            return View(actors.Skip((pageNumber - 1) * itemsNum).Take(itemsNum));
        }

        public IActionResult AssignNew()
        {
            ViewBag.Movies = movieRepository.GetAll();
            return View(actorRepository.GetAll());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignNew(ActorMovie actorMovie)
        {
            actorMovieRepository.Add(actorMovie);
            return RedirectToAction("AssignActors");
        }
        public IActionResult UnAssign(ActorMovie actorMovie)
        {
            actorMovieRepository.Delete(actorMovie);
            return RedirectToAction("AssignActors");
        }

    }
}
