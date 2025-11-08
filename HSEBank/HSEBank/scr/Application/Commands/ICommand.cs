namespace HSEBank.scr.Application.Commands;

// Базовый интерфейс команды
public interface ICommand
{
    void Execute();
    void Undo();
    string Name { get; }
}