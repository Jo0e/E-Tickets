using System.Linq.Expressions;

namespace ETickets.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        ICollection<T> GetAll(params string[] includes);
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Commit();
        void UpdateImage(T entity, IFormFile imageFile, string currentImagePath, string imageFolder, string imageUrlProperty);
        void CreateWithImage(T entity, IFormFile imageFile, string imageFolder, string imageUrlProperty);
        ICollection<T> GetWithIncludes(Expression<Func<T, bool>> filter, params string[] includes);


        IQueryable<T> ThenInclude<TProperty, TThenProperty>(Expression<Func<T, TProperty>> includeExpression,
           Expression<Func<TProperty, TThenProperty>> thenIncludeExpression);
    }
}
