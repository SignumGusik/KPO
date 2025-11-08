using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Services;

namespace HSEBank.scr.Facades
{
    public class CategoryFacade : ICategoryFacade
    {
        private readonly ICategoryService _cats;

        public CategoryFacade(ICategoryService cats)
        {
            _cats = cats;
        }

        public Category Create(string name, string type) => _cats.CreateCategory(name, type);
        public void Update(Guid id, string name, string type) => _cats.UpdateCategory(id, name, type);
        public void Delete(Guid id) => _cats.DeleteCategory(id);
        public IEnumerable<Category>? GetAll() => _cats.GetAllCategories();
        public Category Get(Guid id) => _cats.GetById(id);
    }
}