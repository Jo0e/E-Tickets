namespace E_Ticket.Utility
{
    public class CheckoutFormModel
    {
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public long Amount { get; set; }
        public string? Currency { get; set; }
    }
}
