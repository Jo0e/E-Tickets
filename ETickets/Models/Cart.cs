namespace ETickets.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }
}
