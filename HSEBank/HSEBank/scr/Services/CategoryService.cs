using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }
    public Category GetById(Guid id) => _categoryRepo.GetById(id);
    public void UpdateCategory(Guid id, string name, string type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название категории не может быть пустым.");
        if (type != "Income" && type != "Expense")
            throw new ArgumentException("Тип категории должен быть Income или Expense.");

        var cat = _categoryRepo.GetById(id);
        cat.Name = name;
        cat.Type = type;
    }

    public void DeleteCategory(Guid id)
    {
        var cat = _categoryRepo.GetById(id);
        _categoryRepo.Remove(cat);
    }

    public Category CreateCategory(string name, string type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название категории не может быть пустым.");
        if (type != "Income" && type != "Expense")
            throw new ArgumentException("Тип категории должен быть Income или Expense.");

        var existing = (_categoryRepo.GetAll() ?? Array.Empty<Category>())
            .FirstOrDefault(c => c.Name == name && c.Type == type);
        if (existing != null)
            return existing;

        var category = new Category { Id = Guid.NewGuid(), Name = name, Type = type };
        _categoryRepo.Add(category);
        return category;
    }

    public IEnumerable<Category>? GetAllCategories()
    {
        return _categoryRepo.GetAll();
    }
}