using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Services;

namespace HSEBank.scr.Facades
{
    public class OperationFacade : IOperationFacade
    {
        private readonly IOperationService _ops;
        private readonly IAccountService _accs;

        public OperationFacade(IOperationService ops, IAccountService accs)
        {
            _ops = ops;
            _accs = accs;
        }

        public Operation Create(string type, double amount, DateTime date, Guid categoryId, Guid accountId, string? description = "")
        {
            var op = _ops.CreateOperation(type, amount, date, categoryId, accountId, description);
            _accs.RecalculateBalance(accountId);
            return op;
        }

        public void Update(Guid id, string type, double amount, DateTime date, Guid categoryId, string? description = "")
        {
            var current = _ops.GetById(id); // быстрее и надёжнее
            _ops.UpdateOperation(id, type, amount, date, categoryId, description);
            _accs.RecalculateBalance(current.AccountId);
        }

        public void Delete(Operation op)
        {
            _ops.DeleteOperation(op);
            _accs.RecalculateBalance(op.AccountId);
        }

        public IEnumerable<Operation>? GetAll() => _ops.GetAllOperations();
    }
}