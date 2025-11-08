namespace HSEBank.scr.Events.Handlers;

/// Обработчик для логирования событий:
/// создание операции,
/// удаление счёта
public class AuditLogHandler :
    IEventHandler<OperationCreatedEvent>,
    IEventHandler<AccountDeletedEvent>
{
    public void Handle(OperationCreatedEvent ev)
    {
        Console.WriteLine($"Лог: добавлена операция {ev.Operation.Type} {ev.Operation.Amount}");
    }

    public void Handle(AccountDeletedEvent ev)
    {
        Console.WriteLine($"Лог: удалён счёт {ev.Account.Name}");
    }
}