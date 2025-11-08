using System;
using FluentAssertions;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;
using Xunit;

namespace HSEBank.Tests.Services;

public class OperationServiceTests
{
    [Fact]
    public void CreateOperation_validates_and_publishes_event()
    {
        var opRepo = new OperationRepositoryInMemory(new());
        var bus    = new EventBus();
        var svc    = new OperationService(opRepo, bus);

        // spy
        var spy = new HSEBank.Tests.Common.SpyHandler<OperationCreatedEvent>();
        bus.Subscribe(spy);

        var op = svc.CreateOperation("Income", 123, new DateTime(2025,1,1), Guid.NewGuid(), Guid.NewGuid(), "x");

        op.Id.Should().NotBe(Guid.Empty);
        spy.Calls.Should().Be(1);
        spy.Received[0].Operation.Id.Should().Be(op.Id);
    }

    [Fact]
    public void CreateUpdate_validate_type_and_amount()
    {
        var svc = new OperationService(new OperationRepositoryInMemory(new()), new EventBus());
        var acc = Guid.NewGuid();
        var cat = Guid.NewGuid();

        svc.Invoking(s => s.CreateOperation("", 1, DateTime.UtcNow, cat, acc))
           .Should().Throw<ArgumentException>();
        svc.Invoking(s => s.CreateOperation("Unknown", 1, DateTime.UtcNow, cat, acc))
           .Should().Throw<ArgumentException>();
        svc.Invoking(s => s.CreateOperation("Income", 0, DateTime.UtcNow, cat, acc))
           .Should().Throw<ArgumentException>();

        var op = svc.CreateOperation("Expense", 10, new DateTime(2025,1,1), cat, acc);

        svc.Invoking(s => s.UpdateOperation(op.Id, "?", 10, op.Date, cat))
           .Should().Throw<ArgumentException>();
        svc.Invoking(s => s.UpdateOperation(op.Id, "Income", 0, op.Date, cat))
           .Should().Throw<ArgumentException>();

        svc.UpdateOperation(op.Id, "Income", 20, op.Date, cat, "ok");
    }
}

public class CategoryServiceTests
{
    [Fact]
    public void CreateUpdateDelete_with_validation_and_no_duplicates()
    {
        var repo = new CategoryRepositoryInMemory(new());
        var svc  = new CategoryService(repo);

        svc.Invoking(s=> s.CreateCategory("", "Income")).Should().Throw<ArgumentException>();
        svc.Invoking(s=> s.CreateCategory("x", "???")).Should().Throw<ArgumentException>();

        var c1 = svc.CreateCategory("A", "Income");
        var c2 = svc.CreateCategory("A", "Income"); // дубликат — вернёт существующую
        c2.Id.Should().Be(c1.Id);

        svc.UpdateCategory(c1.Id, "A2", "Expense");
        repo.GetById(c1.Id).Type.Should().Be("Expense");

        svc.DeleteCategory(c1.Id);
        repo.Invoking(r => r.GetById(c1.Id)).Should().Throw<InvalidOperationException>();
    }
}