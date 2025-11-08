using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Facades
{
    public interface ICategoryFacade
    {
        Category Create(string name, string type);
        void Update(Guid id, string name, string type);
        void Delete(Guid id);
        IEnumerable<Category>? GetAll();
        Category Get(Guid id);
    }
}