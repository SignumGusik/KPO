using System;
using System.Linq;
using FluentAssertions;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;
using Xunit;

namespace HSEBank.Tests.Services;

public class AccountServiceTests
{
    [Fact]
    public void RecalculateBalance_sums_Income_minus_Expense()
    {
        var accRepo = new BankAccountRepositoryInMemory(new());
        var opRepo  = new OperationRepositoryInMemory(new());
        var bus     = new EventBus();
        var svc     = new AccountService(accRepo, opRepo, bus);

        var acc = svc.CreateAccount("A", 0);

        opRepo.Add(new Operation { Id=Guid.NewGuid(), AccountId=acc.Id, Type="Income",  Amount=100 });
        opRepo.Add(new Operation { Id=Guid.NewGuid(), AccountId=acc.Id, Type="Expense", Amount=30  });

        svc.RecalculateBalance(acc.Id);

        accRepo.GetById(acc.Id).Balance.Should().Be(70);
    }

    [Fact]
    public void RenameAccount_validates_and_changes_name()
    {
        var accRepo = new BankAccountRepositoryInMemory(new());
        var opRepo  = new OperationRepositoryInMemory(new());
        var svc = new AccountService(accRepo, opRepo, new EventBus());

        var acc = svc.CreateAccount("A", 0);
        svc.Invoking(s => s.RenameAccount(acc.Id, ""))
           .Should().Throw<ArgumentException>();

        svc.RenameAccount(acc.Id, "New");
        accRepo.GetById(acc.Id).Name.Should().Be("New");
    }

    [Fact]
    public void RestoreAccount_readds_if_absent_and_recalculates()
    {
        var accRepo = new BankAccountRepositoryInMemory(new());
        var opRepo  = new OperationRepositoryInMemory(new());
        var svc = new AccountService(accRepo, opRepo, new EventBus());

        var acc = svc.CreateAccount("A", 0);
        var id  = acc.Id;

        // операция существует независимо от удаления счёта
        opRepo.Add(new Operation { Id=Guid.NewGuid(), AccountId=id, Type="Income", Amount=50 });

        // удаляем сам счёт
        accRepo.Remove(acc);

        // восстановление
        svc.RestoreAccount(acc);

        // счет есть и баланс = 50
        accRepo.GetById(id).Balance.Should().Be(50);
    }

    [Fact]
    public void CreateAccount_validates_inputs()
    {
        var svc = new AccountService(
            new BankAccountRepositoryInMemory(new()),
            new OperationRepositoryInMemory(new()),
            new EventBus());

        svc.Invoking(s => s.CreateAccount("", 0)).Should().Throw<ArgumentException>();
        svc.Invoking(s => s.CreateAccount("A", -1)).Should().Throw<ArgumentException>();
    }
}