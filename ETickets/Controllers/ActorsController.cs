using ETickets.Data;
using Microsoft.AspNetCore.Mvc;

namespace ETickets.Controllers
{
    public class Actors : Controller
    {
        ApplicationDbContext context= new ApplicationDbContext();
        public IActionResult Index(int id)
        {
            var actor = context.Actors.FirstOrDefault(x => x.Id == id);
            return View(actor);
        }
    }
}
