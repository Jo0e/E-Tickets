using System.Linq.Expressions;

namespace E_Ticket.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        void AddRange(ICollection<T> entity);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Commit();

        IEnumerable<T> Get(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true);
        public T? GetOne(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true);




        void UpdateImage(T entity, IFormFile imageFile, string currentImagePath, string imageFolder, string imageUrlProperty);
        void CreateWithImage(T entity, IFormFile imageFile, string imageFolder, string imageUrlProperty);

        IQueryable<T> ThenInclude<TProperty, TThenProperty>(Expression<Func<T, TProperty>> includeExpression,
                Expression<Func<TProperty, TThenProperty>> thenIncludeExpression);
    }
}
