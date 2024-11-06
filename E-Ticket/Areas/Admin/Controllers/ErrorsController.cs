using Microsoft.AspNetCore.Mvc;

namespace E_Ticket.Areas.Admin.Controllers
{
    [Area("Admin")]
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
