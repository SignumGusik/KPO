using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Services;

public interface IOperationService
{
    Operation CreateOperation(string type, double amount, DateTime date, Guid categoryId, Guid accountId, string? description = "");
    void DeleteOperation(Operation operation);
    IEnumerable<Operation>? GetAllOperations();
    Operation GetById(Guid id);
    
    void UpdateOperation(Guid id, string type, double amount, DateTime date, Guid categoryId, string? description = "");
}