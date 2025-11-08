namespace HSEBank.scr.Application.Commands
{
    /// реализация команды
    /// оборачивает произвольный Action,
    /// чтобы его можно было использовать в CommandManager.
    public class ActionCommand : ICommand
    {
        private readonly Action _do;
        private readonly Action? _undo;
        public string Name { get; }

        public ActionCommand(string name, Action @do, Action? undo = null)
        {
            Name = name;
            _do = @do;
            _undo = undo;
        }

        public void Execute() => _do();

        public void Undo()
        {
            if (_undo != null) _undo();
            else Console.WriteLine("Отмена не поддерживается для этой команды.");
        }
    }
}