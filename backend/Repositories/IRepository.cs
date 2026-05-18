using System.Linq.Expressions;

public interface IRepository<T> where T : class
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);

    int SaveChanges();

    IEnumerable<T> ReadAll();
    T? FindById(object Id);
    IEnumerable<T> GetBy(Expression<Func<T, bool>> predicate);

    IQueryable<T> Query();
}