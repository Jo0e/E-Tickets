using E_Ticket.Data;
using E_Ticket.Models;
using E_Ticket.Repository.IRepository;

namespace E_Ticket.Repository
{
    public class ActorRepository : Repository<Actor>, IActorRepository
    {
        private readonly ApplicationDbContext context;

        public ActorRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
