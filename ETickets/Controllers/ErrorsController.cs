using Microsoft.AspNetCore.Mvc;

namespace ETickets.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult SearchNotFound()
        {
            return View();
        }
    }
}
