

namespace E_Ticket.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }
}
