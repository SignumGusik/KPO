using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Infrastructure.Serializers;

namespace HSEBank.scr.Export
{
    public class CsvExportVisitor : IExportVisitor
    {
        private readonly List<BankAccount> _acc = new();
        private readonly List<Operation> _ops = new();
        private readonly List<Category> _cats = new();

        public void Visit(BankAccount account) => _acc.Add(account);
        public void Visit(Operation operation) => _ops.Add(operation);
        public void Visit(Category category) => _cats.Add(category);

        public void Save(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
            var csv = new CsvSerializer();

            File.WriteAllText(Path.Combine(folderPath, "accounts.csv"),   csv.Serialize(_acc));
            File.WriteAllText(Path.Combine(folderPath, "operations.csv"), csv.Serialize(_ops));
            File.WriteAllText(Path.Combine(folderPath, "categories.csv"), csv.Serialize(_cats));

            Console.WriteLine($"Экспортировано 3 CSV в папку {folderPath}");
        }
    }
}