using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;

namespace TestProject1
{
    public class CategoryServiceTests
    {
        private readonly List<Category> _categories = new();
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            ICategoryRepository repo = new CategoryRepositoryInMemory(_categories);
            _service = new CategoryService(repo);
        }

        [Fact]
        public void CreateCategory_Valid_Adds()
        {
            var cat = _service.CreateCategory("Еда", "Expense");

            Assert.NotEqual(Guid.Empty, cat.Id);
            Assert.Equal("Еда", cat.Name);
            Assert.Equal("Expense", cat.Type);
            Assert.Single(_service.GetAllCategories() ?? Array.Empty<Category>());
        }

        [Fact]
        public void CreateCategory_SameNameAndType_ReturnsExisting()
        {
            var c1 = _service.CreateCategory("Зарплата", "Income");
            var c2 = _service.CreateCategory("Зарплата", "Income");

            Assert.Equal(c1.Id, c2.Id);
            Assert.Single(_service.GetAllCategories() ?? Array.Empty<Category>());
        }

        [Theory]
        [InlineData(null, "Income")]
        [InlineData("", "Income")]
        [InlineData("   ", "Income")]
        [InlineData("Еда", "Wrong")]
        public void CreateCategory_Invalid_Throws(string name, string type)
        {
            Assert.Throws<ArgumentException>(() => _service.CreateCategory(name, type));
        }

        [Fact]
        public void UpdateCategory_ChangesNameAndType()
        {
            var cat = _service.CreateCategory("Old", "Income");

            _service.UpdateCategory(cat.Id, "New", "Expense");

            var updated = _service.GetById(cat.Id);
            Assert.Equal("New", updated.Name);
            Assert.Equal("Expense", updated.Type);
        }

        [Fact]
        public void DeleteCategory_Removes()
        {
            var cat = _service.CreateCategory("Удалить", "Expense");

            _service.DeleteCategory(cat.Id);

            Assert.Empty(_service.GetAllCategories() ?? Array.Empty<Category>());
        }
    }
}