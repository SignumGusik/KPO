using HSEBank.scr.Application.Commands;

namespace TestProject1
{
    public class CommandTests
    {
        private class TestLogger : ILogger
        {
            public int LogCount { get; private set; }
            public void Log(string message) => LogCount++;
        }

        private class FlagCommand : ICommand
        {
            public bool Executed { get; private set; }
            public bool Undone { get; private set; }
            public string Name => "Flag";

            public void Execute() => Executed = true;
            public void Undo() => Undone = true;
        }

        [Fact]
        public void ActionCommand_ExecuteAndUndo()
        {
            bool done = false;
            bool undone = false;

            var cmd = new ActionCommand(
                "Test",
                () => done = true,
                () => undone = true);

            cmd.Execute();
            cmd.Undo();

            Assert.True(done);
            Assert.True(undone);
        }

        [Fact]
        public void LoggedCommandDecorator_LogsAndDelegates()
        {
            var logger = new TestLogger();
            var inner = new FlagCommand();
            var logged = new LoggedCommandDecorator(inner, logger);

            logged.Execute();
            logged.Undo();

            Assert.True(inner.Executed);
            Assert.True(inner.Undone);
            Assert.True(logger.LogCount >= 2);
        }

        [Fact]
        public void TimeCommandDecorator_Delegates()
        {
            var inner = new FlagCommand();
            var timed = new TimeCommandDecorator(inner);

            timed.Execute();
            timed.Undo();

            Assert.True(inner.Executed);
            Assert.True(inner.Undone);
        }

        [Fact]
        public void CommandManager_WrapsAndStoresHistory()
        {
            var logger = new TestLogger();
            var manager = new CommandManager(logger);
            var inner = new FlagCommand();

            manager.ExecuteCommand(inner);
            manager.UndoLastCommand();

            Assert.True(inner.Executed);
            Assert.True(inner.Undone);
        }
    }
}