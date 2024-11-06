using E_Ticket.Models;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class CinemaController : Controller
    {
        private readonly ICinemaRepository cinemaRepository;

        public CinemaController(ICinemaRepository cinemaRepository)
        {
            this.cinemaRepository = cinemaRepository;
        }

        public IActionResult Index()
        {
            var cinemas = cinemaRepository.Get();
            return View(cinemas);
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
                cinemaRepository.Create(cinema);
                cinemaRepository.Commit();
                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");
        }

        public IActionResult Edit(int cinemaId)
        {
            var cinema = cinemaRepository.GetOne(where: e=>e.Id == cinemaId);
            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cinema cinema)
        {
            if (ModelState.IsValid)
            {
                cinemaRepository.Update(cinema);
                cinemaRepository.Commit();
                return RedirectToAction("Index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");

        }

        public IActionResult Delete(int cinemaId)
        {
            var cinema = cinemaRepository.GetOne(where: e=>e.Id == cinemaId);
            return View(cinema);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Cinema cinema)
        {
            cinemaRepository.Delete(cinema);
            cinemaRepository.Commit();
            return RedirectToAction("Index");
        }

    }
}
