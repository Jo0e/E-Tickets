using E_Ticket.Data;
using E_Ticket.Models;
using E_Ticket.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace E_Ticket.Repository
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        private readonly ApplicationDbContext context;

        public MovieRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public Movie GetDetails(int id)
        {
            return dbSet
                .Include(e => e.Category)
                .Include(e => e.CinemaMovies)
                .ThenInclude(e=>e.Cinema)
                .Include(e => e.ActorMovies)
                .ThenInclude(e => e.Actor)
                .FirstOrDefault(e=>e.Id == id);

        }


        public ICollection<Movie> SearchMovies(string name)
        {
            return dbSet
                .Include(m => m.Category)
                .Where(m => m.Name.Contains(name))
                .ToList();
        }


        public ICollection<Movie> GetMoviesByFilter(int categoryId, int cinemaId, MovieStatus availabilityId)
        {
            return dbSet
                .Include(c => c.Category)
                .Include(m=>m.CinemaMovies)
                .ThenInclude(c=>c.Cinema)
                .Where(c => c.Category.Id == categoryId && c.CinemaMovies.Any(c=>c.CinemaId== cinemaId) && c.MovieStatus == availabilityId)
                .ToList();
        }

    }
}
