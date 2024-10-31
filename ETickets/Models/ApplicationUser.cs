using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ETickets.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string City { get; set; }
        public ICollection<Wishlist> Wishlists{ get; set; }
        public ICollection<Ticket> Tickets{ get; set; }
        public ICollection<Cart> Carts { get; set; }

    }
}
