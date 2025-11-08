using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Facades;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;

namespace TestProject1
{
    public class OperationFacadeTests
    {
        private readonly List<BankAccount> _accounts = new();
        private readonly List<Operation> _operations = new();

        private readonly AccountService _accService;
        private readonly OperationFacade _facade;

        public OperationFacadeTests()
        {
            IBankAccountRepository accRepo = new BankAccountRepositoryInMemory(_accounts);
            IOperationRepository opRepo = new OperationRepositoryInMemory(_operations);
            var bus = new EventBus();

            _accService = new AccountService(accRepo, opRepo, bus);
            var opService = new OperationService(opRepo, bus);
            _facade = new OperationFacade(opService, _accService);
        }

        [Fact]
        public void Create_RecalculatesBalance()
        {
            var acc = _accService.CreateAccount("Test", 0);
            var catId = Guid.NewGuid();

            _facade.Create("Income", 200, DateTime.Today, catId, acc.Id, "init");

            var updated = _accService.GetAccount(acc.Id);
            Assert.Equal(200, updated.Balance);
        }

        [Fact]
        public void Update_RecalculatesBalance()
        {
            var acc = _accService.CreateAccount("Test", 0);
            var cat = Guid.NewGuid();

            var op = _facade.Create("Income", 200, DateTime.Today, cat, acc.Id);

            _facade.Update(op.Id, "Expense", 50, op.Date, cat, "fix");

            var updated = _accService.GetAccount(acc.Id);
            Assert.Equal(-50, updated.Balance);
        }

        [Fact]
        public void Delete_RecalculatesBalance()
        {
            var acc = _accService.CreateAccount("Test", 0);
            var cat = Guid.NewGuid();

            var op = _facade.Create("Income", 200, DateTime.Today, cat, acc.Id);

            _facade.Delete(op);

            var updated = _accService.GetAccount(acc.Id);
            Assert.Equal(0, updated.Balance);
        }
    }
}