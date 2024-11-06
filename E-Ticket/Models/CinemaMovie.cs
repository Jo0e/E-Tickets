namespace E_Ticket.Models
{
    public class CinemaMovie
    {
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
