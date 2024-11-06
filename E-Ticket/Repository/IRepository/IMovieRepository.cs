using E_Ticket.Models;

namespace E_Ticket.Repository.IRepository
{
    public interface IMovieRepository : IRepository<Movie>
    {
        public Movie GetDetails(int id);
        ICollection<Movie> SearchMovies(string name);
        ICollection<Movie> GetMoviesByFilter(int categoryId, int cinemaId, MovieStatus availabilityId);

    }
}
