namespace HSEBank.scr.Application.Commands
{
    /// Менеджер команд:
    /// оборачивает команды в декораторы логирования и измерения времени;
    /// хранит стек истории для Undo.
    public class CommandManager
    {
        private readonly Stack<ICommand> _history = new();
        private readonly ILogger _logger;

        public CommandManager(ILogger? logger = null)
        {
            _logger = logger ?? new ConsoleLogger();
        }
        
        /// Выполнить команду:
        /// обернуть её в TimeCommandDecorator и LoggedCommandDecorator,
        /// выполнить,
        /// положить в историю для последующего Undo
        public void ExecuteCommand(ICommand command)
        {
            ICommand decorated = new LoggedCommandDecorator(
                new TimeCommandDecorator(command),
                _logger);

            decorated.Execute();
            _history.Push(decorated);
        }

        // Отменить последнюю команду в истории
        public void UndoLastCommand()
        {
            if (_history.Count == 0)
            {
                Console.WriteLine("История пуста. Отменять нечего.");
                return;
            }
            _history.Pop().Undo();
        }

        // Вывести список выполненных команд
        public void ShowHistory()
        {
            Console.WriteLine("\nИстория команд:");
            foreach (var cmd in _history)
                Console.WriteLine($" - {cmd.Name}");
        }
    }
}