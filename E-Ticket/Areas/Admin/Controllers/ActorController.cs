
using E_Ticket.Models;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class ActorController : Controller
    {
        private readonly IActorRepository actorRepository;

        public ActorController(IActorRepository actorRepository)
        {
            this.actorRepository = actorRepository;
        }

        public IActionResult Index(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<Actor> actors;
            if (Name == null)
                actors = actorRepository.Get(include: [a=>a.ActorMovies]);
            else
                actors = actorRepository.Get(include: [a => a.ActorMovies], 
                    where: e=>e.FirstName.Contains(Name) || e.LastName.Contains(Name));

            if (!actors.Any())
            {
                TempData["NotFound"] = "Sorry we cant found the Actor try again";
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }

        public IActionResult Edit(int actorId)
        {
            var actor = actorRepository.GetOne(where: e=>e.Id == actorId);
            return View(actor);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Actor actor, IFormFile PhotoUrl)
        {
            ModelState.Remove(nameof(PhotoUrl));
            if (ModelState.IsValid)
            {
                var oldActor = actorRepository.GetOne(where: e => e.Id == actor.Id , tracked: false);
                actorRepository.UpdateImage(actor, PhotoUrl, oldActor.ProfilePicture, "cast", "ProfilePicture");
                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }
        public IActionResult Delete(int actorId)
        {
            var actor = actorRepository.GetOne(where: e => e.Id == actorId);
            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Actor actor)
        {
            actorRepository.Delete(actor);
            return RedirectToAction("Index");
        }


    }
}
