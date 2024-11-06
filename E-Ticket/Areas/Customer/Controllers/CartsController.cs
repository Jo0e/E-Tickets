using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using E_Ticket.Models;
using E_Ticket.Repository.IRepository;
using E_Ticket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.BillingPortal;
using Stripe.Checkout;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;


namespace ETickets.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = $"{SD.CustomerRole}, {SD.AdminRole}")]
    public class CartsController : Controller
    {
        private readonly IRepository<Cart> cartRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMovieRepository movieRepository;
        private readonly IRepository<Ticket> ticketRepository;
        private readonly IRepository<OrderList> orderListRepository;

        public CartsController(IRepository<Cart> cartRepository, UserManager<IdentityUser> userManager, 
            IMovieRepository movieRepository, IRepository<Ticket> TicketRepository
            ,IRepository<OrderList> OrderListRepository)
        {
            this.cartRepository = cartRepository;
            this.userManager = userManager;
            this.movieRepository = movieRepository;
            ticketRepository = TicketRepository;
            orderListRepository = OrderListRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            var userCart = cartRepository.GetOne(where: e=>e.UserId==user.Id);

            // If the user doesn't have a cart, it will create one
            if (userCart == null)
            {
                Cart cart = new Cart()
                {
                    UserId = user.Id,
                    Tickets = new List<Ticket>(),
                };
                cartRepository.Create(cart);
                cartRepository.Commit();
                return RedirectToAction("Index");
            }

            var ticket = ticketRepository.Get(include: [m=>m.Movie] ,where: e => e.CartId == userCart.Id);
            ViewBag.Total = ticket.Sum(e => e.Quantity * e.Movie.Price).ToString("F2");
            return View(ticket);
        }

        public IActionResult AddToCart(int movieId)
        {
            var movie = movieRepository.GetDetails(movieId);
            if (movie != null)
            {
                if (movie.MovieStatus != MovieStatus.Expired)
                {
                    return View(movie);
                }
            }
            return RedirectToAction("NotFound", "Errors");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int Count, int movieId)
        {
            var user = await userManager.GetUserAsync(User);
            var movie = movieRepository.GetDetails(movieId);

            if (movie != null && user!=null)
            {
                if (movie.TicketsRemaining >= Count && Count > 0)
                {
                    var userCart = cartRepository.GetOne(include: [e=>e.Tickets] ,where: e=>e.UserId ==user.Id);
                    if (userCart == null)
                    {
                        Cart cart = new Cart()
                        {
                            UserId = user.Id,
                            Tickets = new List<Ticket>(),
                        };
                        cartRepository.Create(cart);
                        cartRepository.Commit();
                        return RedirectToAction("Index");
                    }
                    var ticketExisting = userCart.Tickets.FirstOrDefault(m => m.MovieId == movieId);
                    if (ticketExisting != null)
                    {
                        ticketExisting.Quantity += Count;
                        ticketRepository.Update(ticketExisting);
                        ticketRepository.Commit();
                    }

                    else
                    {
                        var ticket = new Ticket
                        {
                            MovieId = movieId,
                            MovieName = movie.Name,
                            Quantity = Count,
                            CartId = userCart.Id,
                        };
                        ticketRepository.Create(ticket);
                        userCart.Tickets.Add(ticket);
                        ticketRepository.Commit();
                    }
                    return RedirectToAction("Index");
                }

                TempData["failed"] = $"The Tickets Sold out or you Entered invalid ticket count";
                return RedirectToAction("AddToCart", new { movieId });

            }
            return RedirectToAction("NotFound", "Errors");
        }

    

        public IActionResult DeleteTicket(int id)
        {
            var item = ticketRepository.GetOne(where: e=>e.Id == id);
            if (item != null)
            {
                ticketRepository.Delete(item);
                ticketRepository.Commit();
            }
            return RedirectToAction("Index");
        }
       

        public IActionResult Increment(int Id)
        {
            var ticket = ticketRepository.GetOne(where: e => e.Id == Id);
           
            if (ticket != null) 
            {
                ticket.Quantity++;
                ticketRepository.Commit();
            }

            return RedirectToAction("Index");
        }
        
        public IActionResult Decrement(int Id)
        {

            var ticket = ticketRepository.GetOne(where: e => e.Id == Id);

           
            if (ticket != null)
            {
                ticket.Quantity--;
                if (ticket.Quantity == 0)
                {
                    ticketRepository.Delete(ticket);
                }
                ticketRepository.Commit();
            }

            return RedirectToAction("Index");
        }


        
        public async Task<IActionResult> Pay()
        {
            var user = await userManager.GetUserAsync(User);
            var userCart = cartRepository.GetOne(where: e => e.UserId == user.Id);
            var ticket = ticketRepository.Get(include: [e=>e.Movie] ,where: e => e.CartId == userCart.Id);
            if (!ticket.Any()) 
            {
                return RedirectToAction("SomeThingWrong", "Errors");
            }
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Carts/success?cartId={userCart.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/checkout/cancel",
            };

            foreach (var item in ticket)
            {
                var result = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.MovieName,
                        },
                        UnitAmount = (long)item.Movie.Price * 100,
                    },
                    Quantity = item.Quantity,
                };
                options.LineItems.Add(result);
            }


            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }

        // In case of success the data of the tickets transferred to the orderList table 
        // to send it in the sales action, and then delete the tickets to get the cart empty.
        public IActionResult success(int cartId)
        {
            var userCart = cartRepository.GetOne(where: e => e.Id == cartId);
            var ticket = ticketRepository.Get
                (include: [m=>m.Movie],where: e => e.CartId == userCart.Id);

            foreach (var item in ticket)
            {
                OrderList orderList = new OrderList()
                {
                    UserId = userCart.UserId,
                    MovieId = item.MovieId,
                    PurchaseDate = DateTime.Now,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * item.Movie.Price,
                };
                orderListRepository.Create(orderList);
                item.Movie.TicketsSold = item.Movie.TicketsSold + item.Quantity;
            }
            ticketRepository.Commit();

            foreach (var item in ticket)
            {
                ticketRepository.Delete(item);
            }
            ticketRepository.Commit();

            return View();
        }

        public IActionResult cancel()
        {
            return View();
        }

        // Show the User the tickets he bought
        public async Task<IActionResult> UserOrders(int pageNumber = 1) 
        {
            var user = await userManager.GetUserAsync(User);
            int itemsNum = 8;
            int totalMovies = orderListRepository.Get(include: [e => e.User, m => m.Movie], where: o => o.UserId == user.Id).Count();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)itemsNum);

            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                return RedirectToAction("NotFound", "Errors");

            ViewBag.pageNumber = pageNumber;
            ViewBag.totalPages = totalPages;

            var orders = orderListRepository.Get(include: [e=>e.User , m => m.Movie] ,
                where: o=>o.UserId == user.Id)
                .Skip((pageNumber - 1) * itemsNum).Take(itemsNum);

            return View(orders);
        }



    }
}
