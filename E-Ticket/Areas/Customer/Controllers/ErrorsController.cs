using Microsoft.AspNetCore.Mvc;

namespace E_Ticket.Areas.Customer.Controllers
{
    [Area("Customer")]
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
