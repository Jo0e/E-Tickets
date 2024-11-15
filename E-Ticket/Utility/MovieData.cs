using E_Ticket.Models;

namespace E_Ticket.Utility
{
    public class MovieData
    {
        public List<Movie> Movies { get; set; }
        public List<ActorMovie> ActorMovies { get; set; } = [];
        public List<CinemaMovie> CinemaMovies { get; set; } = [];
    }
}
