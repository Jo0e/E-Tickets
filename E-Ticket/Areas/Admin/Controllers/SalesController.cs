using E_Ticket.Models;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class SalesController : Controller
    {
        private readonly IRepository<OrderList> orderListRepository;

        public SalesController(IRepository<OrderList> OrderListRepository)
        {
            orderListRepository = OrderListRepository;
        }

        public IActionResult Index(string? Name = null, int pageNumber = 1)
        {
            IEnumerable<OrderList> orders;
            if (Name == null)            
                orders = orderListRepository.Get(include: [m => m.Movie, u => u.User]);
            else
                orders = orderListRepository.Get(include: [m => m.Movie, u => u.User],where: e=>e.Movie.Name.Contains(Name) || e.User.UserName.Contains(Name));

            if (!orders.Any())
            {
                TempData["NotFound"] = "Sorry we cant found the Movie try again";
                return RedirectToAction("MovieCRUD");
            }
            int itemsNum = 8;
            int totalMovies = orders.Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);

            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            return View(orders.Skip((pageNumber - 1) * itemsNum).Take(itemsNum));
        }
    }
}
