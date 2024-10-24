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
            actorRepository.CreateWithImage(actor, PhotoUrl, "cast", "ProfilePicture");
            return RedirectToAction(nameof(AllActors));
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
            var oldActor = actorRepository.GetById(actor.Id);
            actorRepository.UpdateImage(actor, PhotoUrl, oldActor.ProfilePicture, "cast", "ProfilePicture");
            return RedirectToAction("AllActors");
        }
        public IActionResult Delete(int id)
        {
            var actor = actorRepository.GetById(id);
            return View(actor);
        }

        [HttpPost]
        public IActionResult Delete(Actor actor)
        {
            actorRepository.Delete(actor);
            return RedirectToAction("AllActors");
        }


        public IActionResult AllActors()
        {
            var actors = actorRepository.GetAll();
            return View(actors);
        }

        public IActionResult AssignActors()
        {
            var actors = actorMovieRepository.GetAll("Actor", "Movie");
            return View(actors);
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
