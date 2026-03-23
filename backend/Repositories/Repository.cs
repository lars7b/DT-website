using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext appDbContext)
    {
        _context = appDbContext;
        _dbSet = _context.Set<T>();
    }

    public void Add(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
        ;
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public T? FindById(object Id)
    {
        return _dbSet.Find(Id);
    }

    public IEnumerable<T> GetBy(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public IEnumerable<T> ReadAll()
    {
        return _dbSet.ToList();
    }
}