using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;

namespace TestProject1
{
    public class AccountServiceTests
    {
        private readonly List<BankAccount> _accounts = new();
        private readonly List<Operation> _operations = new();
        private readonly IBankAccountRepository _accRepo;
        private readonly AccountService _service;

        public AccountServiceTests()
        {
            _accRepo = new BankAccountRepositoryInMemory(_accounts);
            IOperationRepository opRepo = new OperationRepositoryInMemory(_operations);
            var bus = new EventBus();
            _service = new AccountService(_accRepo, opRepo, bus);
        }

        [Fact]
        public void CreateAccount_ValidData_CreatesAndStores()
        {
            var acc = _service.CreateAccount("Кошелёк", 100);

            Assert.NotEqual(Guid.Empty, acc.Id);
            Assert.Equal("Кошелёк", acc.Name);
            Assert.Equal(100, acc.Balance);
            Assert.Single(_accRepo.GetAll() ?? Array.Empty<BankAccount>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateAccount_InvalidName_Throws(string name)
        {
            var ex = Assert.Throws<ArgumentException>(() => _service.CreateAccount(name, 0));
            Assert.Contains("Название счёта", ex.Message);
        }

        [Fact]
        public void CreateAccount_NegativeBalance_Throws()
        {
            Assert.Throws<ArgumentException>(() => _service.CreateAccount("Счёт", -1));
        }

        [Fact]
        public void RecalculateBalance_UsesOperationsIncomeMinusExpense()
        {
            var acc = _service.CreateAccount("Test", 0);

            _operations.Add(new Operation
            {
                Id = Guid.NewGuid(),
                AccountId = acc.Id,
                Type = "Income",
                Amount = 200
            });
            _operations.Add(new Operation
            {
                Id = Guid.NewGuid(),
                AccountId = acc.Id,
                Type = "Expense",
                Amount = 50
            });

            _service.RecalculateBalance(acc.Id);

            var updated = _service.GetAccount(acc.Id);
            Assert.Equal(150, updated.Balance);
        }

        [Fact]
        public void RenameAccount_ChangesName()
        {
            var acc = _service.CreateAccount("Old", 0);

            _service.RenameAccount(acc.Id, "New");

            var updated = _service.GetAccount(acc.Id);
            Assert.Equal("New", updated.Name);
        }

        [Fact]
        public void RenameAccount_Empty_Throws()
        {
            var acc = _service.CreateAccount("Old", 0);
            Assert.Throws<ArgumentException>(() => _service.RenameAccount(acc.Id, ""));
        }

        [Fact]
        public void RestoreAccount_ReaddsAndRecalculates()
        {
            var acc = _service.CreateAccount("ForDelete", 0);
            var accId = acc.Id;

            _operations.Add(new Operation
            {
                Id = Guid.NewGuid(),
                AccountId = acc.Id,
                Type = "Income",
                Amount = 500
            });

            _service.DeleteAccount(acc);
            Assert.Empty(_accRepo.GetAll() ?? Array.Empty<BankAccount>());

            _service.RestoreAccount(acc);

            var restored = _service.GetAccount(accId);
            Assert.Equal(500, restored.Balance);
        }
    }
}