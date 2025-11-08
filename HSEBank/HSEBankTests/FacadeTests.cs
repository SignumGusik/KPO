using System;
using System.Linq;
using FluentAssertions;
using HSEBank.scr.Facades;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;
using Xunit;

namespace HSEBank.Tests.Facades;

public class AccountFacadeTests
{
    [Fact]
    public void CreateAccountWithInitialOperation_creates_account_and_initial_op_and_recalculates()
    {
        var accRepo = new BankAccountRepositoryInMemory(new());
        var opRepo  = new OperationRepositoryInMemory(new());
        var bus     = new EventBus();

        var accountService  = new AccountService(accRepo, opRepo, bus);
        var operationService= new OperationService(opRepo, bus);
        var facade = new AccountFacade(accountService, operationService);

        var initCat = Guid.NewGuid();
        var acc = facade.CreateAccountWithInitialOperation("A", 100, initCat);

        ((IRepository<BankAccount>)accRepo).GetAll()!.Count().Should().Be(1);
        accRepo.GetById(acc.Id).Balance.Should().Be(100);
    }
}

public class OperationFacadeTests
{
    [Fact]
    public void Update_and_Delete_always_recalculate_account_balance()
    {
        var accRepo = new BankAccountRepositoryInMemory(new());
        var opRepo  = new OperationRepositoryInMemory(new());
        var bus     = new EventBus();

        var accSvc = new AccountService(accRepo, opRepo, bus);
        var opSvc  = new OperationService(opRepo, bus);
        var facade = new OperationFacade(opSvc, accSvc);

        var acc = accSvc.CreateAccount("A", 0);
        var cat = Guid.NewGuid();

        var op = facade.Create("Income", 100, new DateTime(2025,1,1), cat, acc.Id);
        accRepo.GetById(acc.Id).Balance.Should().Be(100);

        facade.Update(op.Id, "Expense", 60, op.Date, cat);
        accRepo.GetById(acc.Id).Balance.Should().Be(40);

        facade.Delete(op);
        accRepo.GetById(acc.Id).Balance.Should().Be(0);
    }
}

public class CategoryFacadeTests
{
    [Fact]
    public void Crud_passes_through_to_service()
    {
        var repo = new HSEBank.scr.Repositories.InMemoryRepositories.CategoryRepositoryInMemory(new());
        var svc  = new CategoryService(repo);
        var f    = new CategoryFacade(svc);

        var c = f.Create("A", "Income");
        f.Update(c.Id, "B", "Expense");
        f.Get(c.Id).Name.Should().Be("B");
        f.Delete(c.Id);
        f.GetAll().Should().BeEmpty();
    }
}