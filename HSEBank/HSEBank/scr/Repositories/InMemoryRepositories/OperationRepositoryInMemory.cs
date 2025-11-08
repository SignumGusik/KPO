using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Repositories.InMemoryRepositories;

public class OperationRepositoryInMemory : IOperationRepository
{
    private readonly List<Operation> _operations;

    public OperationRepositoryInMemory(List<Operation> operations)
    {
        _operations = operations;
    }

    public void Add(Operation entity)
    {
        _operations.Add(entity);
    }

    public void Remove(Operation entity)
    {
        _operations.Remove(entity);
    }

    public IEnumerable<Operation> GetAll()
    {
        return _operations;
    }

    public Operation GetById(Guid id)
    {
        return _operations.SingleOrDefault(operation => operation.Id == id) ?? throw new InvalidOperationException();
    }
}