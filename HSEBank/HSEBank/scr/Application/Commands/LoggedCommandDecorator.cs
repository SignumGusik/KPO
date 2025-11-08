namespace HSEBank.scr.Application.Commands;
/// Интерфейс простого логгера, чтобы отвязать команды
/// от конкретной реализации (консоль, файл и т.д.)
public interface ILogger
{
    void Log(string message);
}

// пишет сообщения в консоль.
public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}

/// Декоратор команды, который добавляет логирование перед/после выполнения
/// и при отмене
public class LoggedCommandDecorator : ICommand
{
    private readonly ICommand _inner;
    private readonly ILogger _logger;

    public string Name => _inner.Name + " logged";
    public LoggedCommandDecorator(ICommand inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void Execute()
    {
        _logger.Log($"Выполнение команды: {_inner.Name} ({DateTime.Now})");
        _inner.Execute();
        _logger.Log($"Команда {_inner.Name} завершена.\n");
    }

    public void Undo()
    {
        _logger.Log($"Отмена команды: {_inner.Name} ({DateTime.Now})");
        _inner.Undo();
    }
    
}