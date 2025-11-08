using HSEBank.scr.Domain.Entities;
using HSEBank.scr.ImportExport;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.InMemoryRepositories;

namespace TestProject1
{
    public class DataTransferServiceTests
    {
        [Fact]
        public void ExportAll_Csv_CreatesFiles()
        {
            var accounts = new List<BankAccount>
            {
                new BankAccount { Id = Guid.NewGuid(), Name = "A", Balance = 100 }
            };
            var operations = new List<Operation>();
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Начальный баланс", Type = "Income" }
            };

            IBankAccountRepository accRepo = new BankAccountRepositoryInMemory(accounts);
            IOperationRepository opRepo = new OperationRepositoryInMemory(operations);
            ICategoryRepository catRepo = new CategoryRepositoryInMemory(categories);

            var service = new DataTransferService(accRepo, opRepo, catRepo);

            var folder = Path.Combine(Path.GetTempPath(), "hsebank-tests-" + Guid.NewGuid());
            Directory.CreateDirectory(folder);

            service.ExportAll("csv", folder);

            Assert.True(File.Exists(Path.Combine(folder, "accounts.csv")));
            Assert.True(File.Exists(Path.Combine(folder, "operations.csv")));
            Assert.True(File.Exists(Path.Combine(folder, "categories.csv")));
        }
    }
}