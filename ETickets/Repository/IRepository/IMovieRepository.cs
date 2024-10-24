using ETickets.Models;

namespace ETickets.Repository.IRepository
{
    public interface IMovieRepository
    {
        ICollection<Movie> GetMoviesByCategory(int categoryId);
        ICollection<Movie> GetMoviesByCinema(int cinemaId);
        ICollection<Movie> GetMoviesByFilter(int categoryId, int cinemaId, MovieStatus availabilityId);
        ICollection<Movie> SearchMovies(string name);
        Movie GetMovieDetails(int id);
        ICollection<ActorMovie> GetActorsByMovie(int movieId);


    }
}
