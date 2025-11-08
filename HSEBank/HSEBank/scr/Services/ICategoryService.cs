using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Services;

public interface ICategoryService
{
    IEnumerable<Category>? GetAllCategories();
    Category GetById(Guid id);
    Category CreateCategory(string name, string type);
    
    void UpdateCategory(Guid id, string name, string type);
    void DeleteCategory(Guid id);
}