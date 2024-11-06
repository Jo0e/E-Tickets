using E_Ticket.Data;
using E_Ticket.Models;
using E_Ticket.Repository.IRepository;

namespace E_Ticket.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
