using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Facades;

public interface IAccountFacade
{
    BankAccount CreateAccountWithInitialOperation(string name, double initialBalance, Guid categoryId);
    void DeleteAccount(BankAccount account);
    IEnumerable<BankAccount>? GetAllAccounts();
    BankAccount GetAccount(Guid id);
}