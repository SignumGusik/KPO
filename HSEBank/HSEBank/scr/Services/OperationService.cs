using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Services;

public class OperationService : IOperationService
{
    private IOperationRepository _operationRepo;

    private readonly EventBus _eventBus;
    public OperationService(IOperationRepository operationRepo, EventBus eventBus)
    {
        _operationRepo = operationRepo;
        _eventBus = eventBus;
    }

    public Operation CreateOperation(string type, double amount, DateTime date, Guid categoryId, Guid accountId, string? description = "")
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Тип операции не может быть пустым.");
        if (type != "Income" && type != "Expense")
            throw new ArgumentException("Тип операции должен быть Income или Expense.");
        if (amount <= 0)
            throw new ArgumentException("Сумма операции должна быть > 0.");

        var operation = new Operation
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = type,
            Amount = amount,
            Date = date,
            CategoryId = categoryId,
            Description = description ?? ""
        };

        _operationRepo.Add(operation);

        // публикуем событие
        _eventBus.Publish(new OperationCreatedEvent(operation));
        return operation;
    }


    public void DeleteOperation(Operation operation)
    {
        _operationRepo.Remove(operation);
    }

    public IEnumerable<Operation>? GetAllOperations()
    {
        return _operationRepo.GetAll();
    }
    public void UpdateOperation(Guid id, string type, double amount, DateTime date, Guid categoryId, string? description = "")
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Тип операции не может быть пустым.");
        if (type != "Income" && type != "Expense")
            throw new ArgumentException("Тип операции должен быть Income или Expense.");
        if (amount <= 0)
            throw new ArgumentException("Сумма операции должна быть > 0.");

        var op = _operationRepo.GetById(id);
        op.Type = type;
        op.Amount = amount;
        op.Date = date;
        op.CategoryId = categoryId;
        op.Description = description ?? "";
    }
    public Operation GetById(Guid id) => _operationRepo.GetById(id);
}