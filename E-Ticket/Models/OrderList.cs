namespace E_Ticket.Models
{
    public class OrderList
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }

    }
}
