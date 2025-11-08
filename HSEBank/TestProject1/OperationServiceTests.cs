using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;

namespace TestProject1
{
    public class OperationServiceTests
    {
        private readonly List<Operation> _operations = new();
        private readonly EventBus _bus;
        private readonly OperationService _service;

        private class TestOpHandler : IEventHandler<OperationCreatedEvent>
        {
            public OperationCreatedEvent? Received { get; private set; }
            public void Handle(OperationCreatedEvent ev) => Received = ev;
        }

        public OperationServiceTests()
        {
            IOperationRepository repo = new OperationRepositoryInMemory(_operations);
            _bus = new EventBus();
            _service = new OperationService(repo, _bus);
        }

        [Fact]
        public void CreateOperation_Valid_AddsAndPublishesEvent()
        {
            var handler = new TestOpHandler();
            _bus.Subscribe(handler);

            var op = _service.CreateOperation(
                "Income",
                100,
                DateTime.Today,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "test");

            Assert.NotEqual(Guid.Empty, op.Id);
            Assert.Single(_service.GetAllOperations() ?? Array.Empty<Operation>());
            Assert.NotNull(handler.Received);
            Assert.Equal(op.Id, handler.Received!.Operation.Id);
        }

        [Theory]
        [InlineData(null, 10)]
        [InlineData(" ", 10)]
        [InlineData("WrongType", 10)]
        [InlineData("Income", 0)]
        [InlineData("Expense", -1)]
        public void CreateOperation_Invalid_Throws(string type, double amount)
        {
            Assert.Throws<ArgumentException>(() =>
                _service.CreateOperation(type, amount, DateTime.Now, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public void UpdateOperation_ChangesFields()
        {
            var op = _service.CreateOperation("Income", 100, DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), "old");

            _service.UpdateOperation(op.Id, "Expense", 50, op.Date.AddDays(1), Guid.NewGuid(), "new");

            var updated = _service.GetById(op.Id);
            Assert.Equal("Expense", updated.Type);
            Assert.Equal(50, updated.Amount);
            Assert.Equal("new", updated.Description);
        }

        [Fact]
        public void DeleteOperation_Removes()
        {
            var op = _service.CreateOperation("Income", 100, DateTime.Today, Guid.NewGuid(), Guid.NewGuid());

            _service.DeleteOperation(op);

            Assert.Empty(_service.GetAllOperations() ?? Array.Empty<Operation>());
        }
    }
}