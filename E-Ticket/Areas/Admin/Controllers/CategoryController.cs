using E_Ticket.Models;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            var categories = categoryRepository.Get();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepository.Create(category);
                categoryRepository.Commit();
                return RedirectToAction("index");
            }
            return RedirectToAction("SomeThingWrong", "Errors");

        }

        public IActionResult Edit(int categoryId)
        {
            var category = categoryRepository.GetOne(where: e=>e.Id == categoryId);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepository.Update(category);
                categoryRepository.Commit();
                return RedirectToAction("Index");
            }

            return RedirectToAction("SomeThingWrong", "Errors");
        }
        public IActionResult Delete(int categoryId)
        {
            var category = categoryRepository.GetOne(where: e => e.Id == categoryId);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Category category)
        {
            categoryRepository.Delete(category);
            categoryRepository.Commit();
            return RedirectToAction("Index");
        }


    }
}
