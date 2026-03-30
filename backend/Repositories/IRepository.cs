namespace Backend.Repositories;

public interface IRepository<T>
{
    Task<List<T>> GetAll();
    Task<T?> GetById(long id);
    Task<bool> Add(T entity);
    Task<bool> Update(T entity);
    Task<bool> Delete(T entity);
}