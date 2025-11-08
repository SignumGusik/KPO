using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Events;

/// Событие "создана операция"
/// Используется для аудита и пересчёта баланса
public class OperationCreatedEvent : IEvent
{
    public Operation Operation { get; }

    public OperationCreatedEvent(Operation operation)
    {
        Operation = operation;
    }
}

// Событие "удалён счёт".
public class AccountDeletedEvent : IEvent
{
    public BankAccount Account { get; }

    public AccountDeletedEvent(BankAccount account)
    {
        Account = account;
    }
}