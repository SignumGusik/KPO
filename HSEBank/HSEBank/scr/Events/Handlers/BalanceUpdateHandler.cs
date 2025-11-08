using HSEBank.scr.Services;

namespace HSEBank.scr.Events.Handlers;

/// Обработчик, который после создания операции
/// инициирует пересчёт баланса по счёту
public class BalanceUpdateHandler : IEventHandler<OperationCreatedEvent>
{
    private readonly IAccountService _accountService;

    public BalanceUpdateHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public void Handle(OperationCreatedEvent ev)
    {
        _accountService.RecalculateBalance(ev.Operation.AccountId);
        Console.WriteLine($"Баланс счёта {ev.Operation.AccountId} пересчитан после новой операции.");
    }
}