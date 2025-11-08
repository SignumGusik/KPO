using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Facades
{
    public interface IOperationFacade
    {
        Operation Create(string type, double amount, DateTime date, Guid categoryId, Guid accountId, string? description = "");
        void Update(Guid id, string type, double amount, DateTime date, Guid categoryId, string? description = "");
        void Delete(Operation op);
        IEnumerable<Operation>? GetAll();
    }
}