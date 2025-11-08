namespace HSEBank.scr.Application.Commands;

/// Декоратор команды, который измеряет время выполнения
/// и выводит его в консоль
public class TimeCommandDecorator : ICommand
{
    private readonly ICommand _inner;
    public string Name => _inner.Name + " time";
    public TimeCommandDecorator(ICommand inner)
    {
        _inner = inner;
    }
    public void Execute()
    {
        var start = DateTime.Now;
        _inner.Execute();
        var end = DateTime.Now;
        var duration = end - start;
        Console.WriteLine($"Команда '{_inner.Name}' выполнена за {duration.TotalMilliseconds:F2} мс");
    }

    public void Undo()
    {
        _inner.Undo();
    }
    
}