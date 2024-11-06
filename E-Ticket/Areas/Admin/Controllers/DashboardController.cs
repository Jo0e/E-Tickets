using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
namespace E_Ticket.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        [Area("Admin")]
        [Authorize(Roles = SD.AdminRole)]
        public IActionResult Index()
        {
            return View();
        }


    }
}
