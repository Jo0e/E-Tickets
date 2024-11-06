using E_Ticket.Data;
using E_Ticket.Models;
using E_Ticket.Repository.IRepository;

namespace E_Ticket.Repository
{
    public class CinemaRepository : Repository<Cinema>, ICinemaRepository
    {
        private readonly ApplicationDbContext context;

        public CinemaRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
