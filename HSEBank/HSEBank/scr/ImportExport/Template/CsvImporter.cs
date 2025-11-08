using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Infrastructure.Serializers;
using HSEBank.scr.Ports;

namespace HSEBank.scr.ImportExport.Template
{
    public class CsvImporter : ImportTemplate
    {
        public CsvImporter(IBankAccountRepository a, IOperationRepository o, ICategoryRepository c)
            : base(a, o, c) { }

        protected override (string accounts, string operations, string categories) Read(string path)
        {
            var folder = Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory();
            return (
                accounts: File.Exists(Path.Combine(folder, "accounts.csv"))   ? ReadFile(Path.Combine(folder, "accounts.csv"))   : "",
                operations: File.Exists(Path.Combine(folder, "operations.csv"))? ReadFile(Path.Combine(folder, "operations.csv")): "",
                categories: File.Exists(Path.Combine(folder, "categories.csv"))? ReadFile(Path.Combine(folder, "categories.csv")): ""
            );
        }

        protected override (IEnumerable<BankAccount> accounts, IEnumerable<Operation> operations, IEnumerable<Category> categories)
            Parse((string accounts, string operations, string categories) text)
        {
            var csv = new CsvSerializer();
            return (
                accounts:    string.IsNullOrWhiteSpace(text.accounts)    ? Enumerable.Empty<BankAccount>() : csv.Deserialize<BankAccount>(text.accounts),
                operations:  string.IsNullOrWhiteSpace(text.operations)  ? Enumerable.Empty<Operation>()   : csv.Deserialize<Operation>(text.operations),
                categories:  string.IsNullOrWhiteSpace(text.categories)  ? Enumerable.Empty<Category>()     : csv.Deserialize<Category>(text.categories)
            );
        }
    }
}