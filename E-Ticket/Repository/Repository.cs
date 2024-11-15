using E_Ticket.Data;
using E_Ticket.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
namespace E_Ticket.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private ApplicationDbContext context;
        protected DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        // CRUD operations
        public IEnumerable<T> Get(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;

            if (where != null)
            {
                query = query.Where(where);
            }

            if (include != null)
            {
                foreach (var prop in include)
                {
                    query = query.Include(prop);
                }
            }

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            return query.ToList();
        }

        public T? GetOne(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true)
        {
            return Get(include, where, tracked).FirstOrDefault();
        }

        public IQueryable<T> ThenInclude<TProperty, TThenProperty>(Expression<Func<T, TProperty>> include,
            Expression<Func<TProperty, TThenProperty>> thenInclude)
        {
            return dbSet.Include(include).ThenInclude(thenInclude);
        }


        public void Create(T entity)
        {
            dbSet.Add(entity);
        }

        public void AddRange(ICollection<T> entity)
        {
            dbSet.AddRange(entity);
        }


        public void Update(T entity)
        {
            dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public void Commit()
        {
            context.SaveChanges();
        }








        public void CreateWithImage(T entity, IFormFile imageFile, string imageFolder, string imageUrlProperty)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{imageFolder}", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    imageFile.CopyTo(stream);
                }

                var property = typeof(T).GetProperty(imageUrlProperty);
                if (property != null)
                {
                    property.SetValue(entity, fileName);
                }
            }

            dbSet.Add(entity);
            context.SaveChanges();
        }

        public void UpdateImage(T entity, IFormFile imageFile, string currentImagePath, string imageFolder, string imageUrlProperty)
        {
            var entityId = (int)typeof(T).GetProperty("Id").GetValue(entity);
            var oldEntity = dbSet.AsNoTracking().AsEnumerable().FirstOrDefault(e => (int)typeof(T).GetProperty("Id").GetValue(e) == entityId);

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{imageFolder}", fileName);
                var oldFilePath = oldEntity != null ? Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{imageFolder}", (string)typeof(T).GetProperty(imageUrlProperty).GetValue(oldEntity)) : "";

                using (var stream = System.IO.File.Create(filePath))
                {
                    imageFile.CopyTo(stream);
                }

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                var property = typeof(T).GetProperty(imageUrlProperty);
                if (property != null)
                {
                    property.SetValue(entity, fileName);
                }
            }
            else
            {
                var property = typeof(T).GetProperty(imageUrlProperty);
                if (property != null)
                {
                    property.SetValue(entity, oldEntity != null ? (string)property.GetValue(oldEntity) : currentImagePath);
                }
            }

            var trackedEntity = dbSet.Local.FirstOrDefault(e => (int)typeof(T).GetProperty("Id").GetValue(e) == entityId);
            if (trackedEntity != null)
            {
                context.Entry(trackedEntity).State = EntityState.Detached;
            }

            dbSet.Update(entity);
            context.SaveChanges();

        }




    }


}

