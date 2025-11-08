using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Repositories.InMemoryRepositories;

public class BankAccountRepositoryInMemory : IBankAccountRepository
{
    private readonly List<BankAccount> _accounts;

    public BankAccountRepositoryInMemory(List<BankAccount> accounts)
    {
        _accounts = accounts;
    }

    public void Add(BankAccount account)
    {
        _accounts.Add(account);
    }

    public void Remove(BankAccount account)
    {
        _accounts.Remove(account);
    }

    IEnumerable<BankAccount> IRepository<BankAccount>.GetAll()
    {
        return _accounts;
    }


    public BankAccount GetById(Guid id)
    {
        return _accounts.FirstOrDefault(account => account.Id == id) ?? throw new InvalidOperationException();
    }
    
}