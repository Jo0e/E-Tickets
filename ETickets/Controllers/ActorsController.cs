using ETickets.Data;
using Microsoft.AspNetCore.Mvc;

namespace ETickets.Controllers
{
    public class ActorsController : Controller
    {
        ApplicationDbContext context= new ApplicationDbContext();
        public IActionResult Index(int id)
        {
            var actor = context.Actors.FirstOrDefault(x => x.Id == id);
            if (actor != null)
            {
                return View(actor);
            }
            return RedirectToAction("NotFound","Errors");
        }
    }
}
