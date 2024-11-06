using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace E_Ticket.Models
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public string UserId { get; set; }
        [ValidateNever]
        public virtual ApplicationUser User { get; set; }
        public int MovieId { get; set; }
        [ValidateNever]
        public virtual Movie Movie { get; set; }
    }
}

