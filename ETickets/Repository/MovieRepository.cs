using ETickets.Data;
using ETickets.Models;
using ETickets.Repository;
using ETickets.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;

public class MovieRepository : Repository<Movie>, IMovieRepository
{
    public MovieRepository(ApplicationDbContext context) : base(context) { }

    public ICollection<Movie> GetMoviesByCategory(int categoryId)
    {
        return GetWithIncludes(m => m.CategoryId == categoryId, "Category", "Cinema");
    }

    public ICollection<Movie> GetMoviesByCinema(int cinemaId)
    {
        return GetWithIncludes(m => m.CinemaId == cinemaId, "Category", "Cinema");
    }

    public ICollection<Movie> GetMoviesByFilter(int categoryId, int cinemaId, MovieStatus availabilityId)
    {
        return dbSet
            .Include(e => e.Cinema).Include(m => m.Category)
            .Where(c => c.Category.Id == categoryId && c.Cinema.Id == cinemaId && c.MovieStatus == availabilityId)
            .ToList();
    }


    public ICollection<Movie> SearchMovies(string name)
    {
        return dbSet
            .Include(m => m.Category)
            .Include(m => m.Cinema)
            .Where(m => m.Name.Contains(name))
            .ToList();
    }

    public Movie GetMovieDetails(int id)
    {   
            return dbSet
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .Include(m=>m.ActorMovies)
                 .ThenInclude(e=>e.Actor)
                .FirstOrDefault(m => m.Id == id);   
    }

    public ICollection<ActorMovie> GetActorsByMovie(int movieId)
    {
        return context.ActorMovies
            .Where(am => am.MovieId == movieId)
            .Include(am => am.Actor)
            .ToList();
    }


}
