using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Services;


namespace HSEBank.scr.Facades;

public class AccountFacade : IAccountFacade
{
    private readonly IAccountService _accountService;
    private readonly IOperationService _operationService;

    public AccountFacade(IAccountService accountService, IOperationService operationService)
    {
        _accountService = accountService;
        _operationService = operationService;
    }

    public BankAccount CreateAccountWithInitialOperation(string name, double initialBalance, Guid categoryId)
    {
        var account = _accountService.CreateAccount(name, 0);
        
        if (initialBalance > 0)
        {
            _operationService.CreateOperation(
                "Income",
                initialBalance,
                DateTime.Now,
                categoryId,
                account.Id
            );

            _accountService.RecalculateBalance(account.Id);
        }

        return account;
    }
    

    public void DeleteAccount(BankAccount account)
    {
        _accountService.DeleteAccount(account);
    }

    public IEnumerable<BankAccount>? GetAllAccounts()
    {
        return _accountService.GetAllAccounts();
    }

    public BankAccount GetAccount(Guid id)
    {
        return _accountService.GetAccount(id);
    }
}