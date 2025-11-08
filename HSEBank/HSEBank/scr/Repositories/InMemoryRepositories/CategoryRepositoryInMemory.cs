using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Repositories.InMemoryRepositories;

public class CategoryRepositoryInMemory : ICategoryRepository
{
    private readonly List<Category> _categories;

    public CategoryRepositoryInMemory(List<Category> categories)
    {
        _categories = categories;
    }

    public void Add(Category entity)
    {
        _categories.Add(entity);
    }
    

    public void Remove(Category entity)
    {
        _categories.Remove(entity);
    }

    public IEnumerable<Category> GetAll()
    {
        return _categories;
    }

    public Category GetById(Guid id)
    {
        return _categories.FirstOrDefault(category => category.Id == id) ?? throw new InvalidOperationException();
    }
}