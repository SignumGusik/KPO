using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Services;

public interface IAccountService
{
    BankAccount CreateAccount(string name, double initialBalance);
    void DeleteAccount(BankAccount account);
    BankAccount GetAccount(Guid id);
    IEnumerable<BankAccount>? GetAllAccounts();
    void RecalculateBalance(Guid accountId);
    
    void RenameAccount(Guid id, string newName);
    void RestoreAccount(BankAccount account);
}