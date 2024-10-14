using ETickets.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETickets.Controllers
{
    public class Category : Controller
    {
        ApplicationDbContext context =new ApplicationDbContext();
        public IActionResult Index()
        {
            var categories = context.Categories.ToList();
            return View(categories);
        }
        public IActionResult Details(int id)
        {
            var movies = context.Movies.Include(e => e.Cinema).Include(m=>m.Category).Where(c => c.Category.Id == id).ToList();
            return View(movies);
        }
    }
}
