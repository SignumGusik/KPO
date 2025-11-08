using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Repositories.Cashing
{
    public class BankAccountRepositoryCacheProxy : IBankAccountRepository
    {
        private readonly RepositoryCacheProxy<BankAccount> _inner;

        public BankAccountRepositoryCacheProxy(IRepository<BankAccount> real) =>
            _inner = new RepositoryCacheProxy<BankAccount>(real);

        public void Add(BankAccount e) => _inner.Add(e);
        public void Remove(BankAccount e) => _inner.Remove(e);
        public IEnumerable<BankAccount> GetAll() => _inner.GetAll() ?? Enumerable.Empty<BankAccount>();
        public BankAccount GetById(Guid id) => _inner.GetById(id);
    }
}