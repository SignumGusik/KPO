using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Services;

/// создание/удаление, получение, пересчёт баланса,
/// переименование и восстановление
public class AccountService : IAccountService
{
    private readonly IBankAccountRepository _accountRepo;
    private readonly IOperationRepository _operationRepo;
    private readonly EventBus _eventBus;
    public AccountService(IBankAccountRepository accountRepo, IOperationRepository operationRepo, EventBus eventBus)
    {
        _accountRepo = accountRepo;
        _operationRepo = operationRepo;
        _eventBus = eventBus;
    }
    
    // Создаёт новый счёт с указанным начальным балансом
    public BankAccount CreateAccount(string name, double initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название счёта не может быть пустым.");

        if (initialBalance < 0)
            throw new ArgumentException("Начальный баланс не может быть отрицательным.");

        BankAccount account = new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = name,
            Balance = initialBalance
        };

        _accountRepo.Add(account);
        return account;
    }

    // Удаляет счёт и публикует событие AccountDeletedEvent
    public void DeleteAccount(BankAccount account)
    {
        _accountRepo.Remove(account);
        _eventBus.Publish(new AccountDeletedEvent(account));
    }

    public BankAccount GetAccount(Guid id)
    {
        return _accountRepo.GetById(id);
    }

    public IEnumerable<BankAccount>? GetAllAccounts()
    {
        return _accountRepo.GetAll();
    }

    // Пересчитывает баланс счёта на основе всех операций
    public void RecalculateBalance(Guid accountId)
    {
        var account = _accountRepo.GetById(accountId);
        var operations = (_operationRepo.GetAll() ?? Array.Empty<Operation>()).Where(op => op.AccountId == accountId);

        account.Balance = operations.Sum(op => op.Type == "Income" ? op.Amount : -op.Amount);
    }
    
    // Переименовать счёт
    public void RenameAccount(Guid id, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Название счёта не может быть пустым.");

        var acc = _accountRepo.GetById(id);
        acc.Name = newName;
    }
    
    // Восстановление удалённого счёта
    public void RestoreAccount(BankAccount account)
    {
        if (account == null) throw new ArgumentNullException(nameof(account));
        
        var exists = true;
        try
        {
            _ = _accountRepo.GetById(account.Id);
        }
        catch
        {
            exists = false;
        }

        if (!exists)
            _accountRepo.Add(account);
        
        RecalculateBalance(account.Id);
    }
}