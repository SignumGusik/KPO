using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Facades;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;

namespace TestProject1
{
    public class AccountFacadeTests
    {
        private readonly List<BankAccount> _accounts = new();
        private readonly List<Operation> _operations = new();

        private readonly IBankAccountRepository _accRepo;
        private readonly IOperationRepository _opRepo;

        private readonly AccountFacade _facade;

        public AccountFacadeTests()
        {
            _accRepo = new BankAccountRepositoryInMemory(_accounts);
            _opRepo = new OperationRepositoryInMemory(_operations);
            var bus = new EventBus();

            var accService = new AccountService(_accRepo, _opRepo, bus);
            var opService = new OperationService(_opRepo, bus);

            _facade = new AccountFacade(accService, opService);
        }

        [Fact]
        public void CreateAccountWithInitialOperation_WithPositiveBalance_CreatesAccountAndOperation()
        {
            var categoryId = Guid.NewGuid();

            var acc = _facade.CreateAccountWithInitialOperation("Мой счёт", 300, categoryId);
            Assert.NotEqual(Guid.Empty, acc.Id);
            Assert.Equal("Мой счёт", acc.Name);
            var opsForAcc = (_opRepo.GetAll() ?? Array.Empty<Operation>())
                .Where(o => o.AccountId == acc.Id)
                .ToList();

            Assert.Single(opsForAcc);
            var op = opsForAcc.Single();
            Assert.Equal("Income", op.Type);
            Assert.Equal(300, op.Amount);
            Assert.Equal(categoryId, op.CategoryId);
            var stored = _accRepo.GetById(acc.Id);
            Assert.Equal(300, stored.Balance);
        }

        [Fact]
        public void CreateAccountWithInitialOperation_ZeroBalance_NoInitialOperation()
        {
            var categoryId = Guid.NewGuid();

            var acc = _facade.CreateAccountWithInitialOperation("Без начального", 0, categoryId);

            Assert.NotEqual(Guid.Empty, acc.Id);
            Assert.Equal("Без начального", acc.Name);

            var opsForAcc = (_opRepo.GetAll() ?? Array.Empty<Operation>())
                .Where(o => o.AccountId == acc.Id)
                .ToList();

            Assert.Empty(opsForAcc);

            var stored = _accRepo.GetById(acc.Id);
            Assert.Equal(0, stored.Balance);
        }

        [Fact]
        public void DeleteAccount_RemovesFromRepository()
        {
            var categoryId = Guid.NewGuid();
            var acc = _facade.CreateAccountWithInitialOperation("Удалить", 100, categoryId);

            Assert.Single(_accRepo.GetAll() ?? Array.Empty<BankAccount>());

            _facade.DeleteAccount(acc);

            Assert.Empty(_accRepo.GetAll() ?? Array.Empty<BankAccount>());
            Assert.NotEmpty(_opRepo.GetAll() ?? Array.Empty<Operation>());
        }

        [Fact]
        public void GetAllAccounts_ReturnsCreatedAccounts()
        {
            var cat = Guid.NewGuid();
            var a1 = _facade.CreateAccountWithInitialOperation("1", 10, cat);
            var a2 = _facade.CreateAccountWithInitialOperation("2", 20, cat);

            var all = (_facade.GetAllAccounts() ?? Array.Empty<BankAccount>()).ToList();

            Assert.Equal(2, all.Count);
            Assert.Contains(all, x => x.Id == a1.Id);
            Assert.Contains(all, x => x.Id == a2.Id);
        }

        [Fact]
        public void GetAccount_ReturnsById()
        {
            var cat = Guid.NewGuid();
            var acc = _facade.CreateAccountWithInitialOperation("Поиск", 50, cat);

            var loaded = _facade.GetAccount(acc.Id);

            Assert.Equal(acc.Id, loaded.Id);
            Assert.Equal("Поиск", loaded.Name);
        }
    }
}