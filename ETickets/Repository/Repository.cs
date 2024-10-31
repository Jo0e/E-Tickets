using ETickets.Data;
using ETickets.Models;
using ETickets.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ETickets.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext context;
        protected readonly DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        public ICollection<T> GetAll(params string[] includes)
        {
            IQueryable<T> values = dbSet;
            foreach (var item in includes)
            {
                values = values.Include(item);
            }
            return values.ToList();
        }
        public T? GetById(int id)
        {
            return dbSet.Find(id);
        }
        public void Add(T item)
        {
            dbSet.Add(item);
            context.SaveChanges();
        }
        public void Update(T item)
        {
            dbSet.Update(item);
            context.SaveChanges();
        }
        public void Delete(T item)
        {
            dbSet.Remove(item);
            context.SaveChanges();
        }
        public void Commit()
        {
            context.SaveChanges();
        }

        public IQueryable<T> ThenInclude<TProperty, TThenProperty>( Expression<Func<T, TProperty>> includeExpression,
            Expression<Func<TProperty, TThenProperty>> thenIncludeExpression)
        {
            return dbSet.Include(includeExpression).ThenInclude(thenIncludeExpression);
        }


        public ICollection<T> GetWithIncludes(Expression<Func<T, bool>> filter, params string[] includes)
        {
            IQueryable<T> query = dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.Where(filter).ToList();
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
