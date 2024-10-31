using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ETickets.Data;
using ETickets.Models;
using ETickets.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Stripe.Checkout;
using ETickets.Utility;
using Microsoft.AspNetCore.Authorization;

namespace ETickets.Controllers
{
    [Authorize(Roles = $"{SD.CustomerRole}, {SD.AdminRole}")]
    public class CartsController : Controller
    {
        private readonly IRepository<Cart> cartRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepository<Movie> movieRepository;
        private readonly IRepository<Ticket> ticketRepository;

        public CartsController(IRepository<Cart> cartRepository, UserManager<ApplicationUser> userManager, IRepository<Movie> movieRepository, IRepository<Ticket> TicketRepository)
        {
            this.cartRepository = cartRepository;
            this.userManager = userManager;
            this.movieRepository = movieRepository;
            ticketRepository = TicketRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            var userCart = cartRepository.GetAll().FirstOrDefault(e => e.UserId == user.Id);

            if (userCart == null)
            {
                Cart cart = new Cart()
                {
                    Date = DateTime.Now,
                    UserId = user.Id,
                    Tickets = new List<Ticket>(),
                };
                cartRepository.Add(cart);
            }


            var ticket = ticketRepository.GetWithIncludes(filter: e => e.CartId == userCart.Id, "Movie");
            ViewBag.Total = ticket.Sum(e => e.Quantity * e.Movie.Price).ToString("F2");
            return View(ticket);
        }

        public IActionResult TicketIndex(int movieId)
        {
            // var user = await userManager.GetUserAsync(User);
            var movie = movieRepository.GetWithIncludes(e => e.Id == movieId, "Cinema", "Category").FirstOrDefault();
            if (movie != null)
            {
                if (movie.MovieStatus != MovieStatus.Expired)
                {
                    return View(movie);
                }
            }
            return RedirectToAction("NotFound", "Errors");
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int Count, int movieId)
        {
            var user = await userManager.GetUserAsync(User);
            var movie = movieRepository.GetAll("Cinema", "Category").FirstOrDefault(e => e.Id == movieId);

            if (movie != null)
            {

                if (movie.TicketsRemaining >= Count && Count > 0)
                {
                    var userCart = cartRepository.GetAll("Tickets").FirstOrDefault(c => c.UserId == user.Id);
                    if (userCart == null)
                    {
                        Cart cart = new Cart()
                        {
                            Date = DateTime.Now,
                            UserId = user.Id,
                            Tickets = new List<Ticket>(),
                        };
                        cartRepository.Add(cart);
                    }
                    var ticketExisting = userCart.Tickets.FirstOrDefault(t => t.MovieId == movieId);
                    if (ticketExisting != null)
                    {
                        ticketExisting.Quantity += Count;
                        ticketRepository.Update(ticketExisting);
                    }

                    else
                    {
                        var ticket = new Ticket
                        {
                            UserId = user.Id,
                            MovieId = movieId,
                            MovieName = movie.Name,
                            Quantity = Count,
                            CartId = userCart.Id,
                        };
                        ticketRepository.Add(ticket);
                        userCart.Tickets.Add(ticket);
                    }
                    return RedirectToAction("Index");
                }

                TempData["failed"] = $"The Tickets Sold out or you Entered invalid ticket count";
                return RedirectToAction("TicketIndex", new { movieId });

            }
            return RedirectToAction("NotFound", "Errors");
        }

    

        public IActionResult DeleteTicket(int id)
        {
            var item = ticketRepository.GetById(id);
            if (item != null)
            {
                ticketRepository.Delete(item);
            }
            return RedirectToAction("Index");
        }
       

        public IActionResult Increment(int Id)
        {
            var ticket = ticketRepository.GetById(Id);
            //var user = await userManager.GetUserAsync(User);
            //var ticket = ticketRepository.GetWithIncludes(filter: e => e.UserId == user.Id && e.MovieId == movieId, "Movie")
            //    .FirstOrDefault();
            if (ticket != null) 
            {
                ticket.Quantity++;
                ticketRepository.Commit();
            }

            return RedirectToAction("Index");
        }
        
        public IActionResult Decrement(int Id)
        {

            var ticket = ticketRepository.GetById(Id);

            //var user = await userManager.GetUserAsync(User);
            //var ticket = ticketRepository.GetWithIncludes(filter: e => e.UserId == user.Id && e.MovieId == movieId, "Movie")
            //    .FirstOrDefault();
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
            var userCart = cartRepository.GetAll().FirstOrDefault(e => e.UserId == user.Id);
            var ticket = ticketRepository.GetWithIncludes(filter: e => e.CartId == userCart.Id, "Movie");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Carts/success?cartId={userCart.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/checkout/cancel",
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

  
        public IActionResult success(int cartId)
        {
            var userCart = cartRepository.GetWithIncludes(e => e.Id == cartId).FirstOrDefault();
            var ticket = ticketRepository.GetWithIncludes(e => e.CartId == userCart.Id, "Movie").ToList();

            foreach (var item in ticket)
            {
                item.Movie.TicketsSold = item.Movie.TicketsSold + item.Quantity;
            }
            ticketRepository.Commit();

            foreach (var item in ticket)
            {
                ticketRepository.Delete(item);
            }

            return View();
        }

        public IActionResult cancel()
        {
            return View();
        }

    }
}
