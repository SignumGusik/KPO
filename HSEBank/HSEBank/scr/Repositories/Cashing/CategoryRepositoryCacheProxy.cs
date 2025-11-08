using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Repositories.Cashing
{
    public class CategoryRepositoryCacheProxy : ICategoryRepository
    {
        private readonly RepositoryCacheProxy<Category> _inner;
        public CategoryRepositoryCacheProxy(ICategoryRepository real) => _inner = new RepositoryCacheProxy<Category>(real);
        public void Add(Category e) => _inner.Add(e);
        public void Remove(Category e) => _inner.Remove(e);
        public IEnumerable<Category> GetAll() => _inner.GetAll() ?? Enumerable.Empty<Category>();
        public Category GetById(Guid id) => _inner.GetById(id);
    }
}