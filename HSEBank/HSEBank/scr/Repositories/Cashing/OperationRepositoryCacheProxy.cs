using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Repositories.Cashing 
{


    public class OperationRepositoryCacheProxy : IOperationRepository
    {
        private readonly RepositoryCacheProxy<Operation> _inner;

        public OperationRepositoryCacheProxy(IOperationRepository real) =>
            _inner = new RepositoryCacheProxy<Operation>(real);

        public void Add(Operation e) => _inner.Add(e);
        public void Remove(Operation e) => _inner.Remove(e);
        public IEnumerable<Operation> GetAll() => _inner.GetAll() ?? Enumerable.Empty<Operation>();
        public Operation GetById(Guid id) => _inner.GetById(id);
    }
}