namespace HSEBank.scr.Ports;

public interface IRepository<T>  where T : class
{
    void Add(T entity);
    void Remove(T entity);
    IEnumerable<T>? GetAll();
    T GetById(Guid id);
}