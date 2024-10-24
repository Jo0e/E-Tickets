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
        void UpdateImage(T entity, IFormFile imageFile, string currentImagePath, string imageFolder, string imageUrlProperty);
        void CreateWithImage(T entity, IFormFile imageFile, string imageFolder, string imageUrlProperty);


    }
}
