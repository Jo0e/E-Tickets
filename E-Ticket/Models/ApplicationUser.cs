using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace E_Ticket.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string City { get; set; }
        public DateOnly Birthdate { get; set; }

        public ICollection<Wishlist> Wishlists{ get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<OrderList> OrderLists { get; set; }

    }
}
