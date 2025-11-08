using HSEBank.scr.Ports;

namespace HSEBank.scr.Repositories.Cashing;

public class RepositoryCacheProxy<T> : IRepository<T> where T : class
{
    private readonly IRepository<T> _realRepository;
    private readonly Dictionary<Guid, T> _cacheById;
    private IEnumerable<T>? _cachedAllItems;
    private bool _isAllCached;

    public RepositoryCacheProxy(IRepository<T> realRepository)
    {
        _realRepository = realRepository;
        _cacheById = new Dictionary<Guid, T>();
        _cachedAllItems = null;
        _isAllCached = false;
        
    }
    public void Add(T entity)
    {
        _realRepository.Add(entity);
        _cacheById.Clear();
        _isAllCached = false;
    }

    public void Remove(T entity)
    {
        _realRepository.Remove(entity);
        _cacheById.Clear();
        _isAllCached = false;
    }

    public IEnumerable<T>? GetAll()
    {
        if (_isAllCached)
        {
            return _cachedAllItems;
        }
        _cachedAllItems = (_realRepository.GetAll() ?? throw new InvalidOperationException()).ToList();
        _isAllCached = true;
        return _cachedAllItems;
    }

    public T GetById(Guid id)
    {
        if (_cacheById.ContainsKey(id))
        {
            return _cacheById[id];
        }
        T entity = _realRepository.GetById(id);
        _cacheById[id] = entity;
        return entity;
    }
}