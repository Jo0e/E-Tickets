using Microsoft.AspNetCore.Mvc;

namespace ETickets.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult SearchNotFound()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }
        public IActionResult SomeThingWrong() 
        {
            return View();
        }

    }
}
