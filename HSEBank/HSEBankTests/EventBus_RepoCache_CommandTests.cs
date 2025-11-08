using System;
using System.Linq;
using FluentAssertions;
using HSEBank.scr.Application.Commands;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.Cashing;
using HSEBank.Tests.Common;
using Xunit;

namespace HSEBank.Tests.Infrastructure;

public class EventBusTests
{
    [Fact]
    public void Publish_calls_all_subscribers_once()
    {
        var bus = new EventBus();
        var h1 = new SpyHandler<AccountDeletedEvent>();
        var h2 = new SpyHandler<AccountDeletedEvent>();
        bus.Subscribe(h1);
        bus.Subscribe(h2);

        var ev = new AccountDeletedEvent(new BankAccount{ Id=Guid.NewGuid(), Name="A"});
        bus.Publish(ev);

        h1.Calls.Should().Be(1);
        h2.Calls.Should().Be(1);
        h1.Received[0].Account.Name.Should().Be("A");
    }
}

public class RepositoryCacheProxyTests
{
    private static BankAccount Make(Guid id, string name) => new() { Id=id, Name=name, Balance=0 };

    [Fact]
    public void GetAll_is_cached_until_invalidation()
    {
        var real = new TestRepo();
        var proxy = new BankAccountRepositoryCacheProxy(real);

        real.Add(Make(Guid.NewGuid(), "a"));
        proxy.GetAll()!.Count().Should().Be(1);
        proxy.GetAll()!.Count().Should().Be(1);

        real.GetAllCalls.Should().Be(1, "второй вызов должен быть из кэша");

        // invalidate by Add via proxy
        proxy.Add(Make(Guid.NewGuid(), "b"));
        proxy.GetAll()!.Count().Should().Be(2);
        real.GetAllCalls.Should().Be(2);
    }

    [Fact]
    public void GetById_caches_items()
    {
        var id = Guid.NewGuid();
        var real = new TestRepo();
        real.Add(Make(id, "a"));
        var proxy = new BankAccountRepositoryCacheProxy(real);

        proxy.GetById(id).Name.Should().Be("a");
        proxy.GetById(id).Name.Should().Be("a");

        real.GetByIdCalls.Should().Be(1);
    }

    private sealed class TestRepo : IRepository<BankAccount>
    {
        public int GetAllCalls { get; private set; }
        public int GetByIdCalls { get; private set; }
        private readonly System.Collections.Generic.List<BankAccount> _list = new();

        public void Add(BankAccount e) => _list.Add(e);
        public void Remove(BankAccount e) => _list.Remove(e);
        public System.Collections.Generic.IEnumerable<BankAccount>? GetAll() { GetAllCalls++; return _list; }
        public BankAccount GetById(Guid id)
        {
            GetByIdCalls++;
            return _list.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException();
        }
    }
}

public class CommandManagerTests
{
    private sealed class FlagCommand : ICommand
    {
        public string Name => "Flag";
        public bool Executed, Undone;
        public void Execute() => Executed = true;
        public void Undo() => Undone = true;
    }

    [Fact]
    public void ExecuteCommand_wraps_with_logged_and_time_and_pushes_to_history()
    {
        var logger = new TestLogger();
        var cm = new CommandManager(logger);

        var cmd = new FlagCommand();
        cm.ExecuteCommand(cmd);

        cmd.Executed.Should().BeTrue();
        logger.Lines.Count(l => l.Contains("Выполнение команды")).Should().Be(1);
        logger.Lines.Count(l => l.Contains("завершена")).Should().Be(1);
    }

    [Fact]
    public void UndoLastCommand_invokes_inner_undo_and_pops()
    {
        var cm = new CommandManager(new TestLogger());
        var cmd = new FlagCommand();
        cm.ExecuteCommand(cmd);
        cm.UndoLastCommand();
        cmd.Undone.Should().BeTrue();
    }
}