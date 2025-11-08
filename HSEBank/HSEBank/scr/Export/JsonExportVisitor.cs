using System.Text;
using System.Text.Json;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.ImportExport;

namespace HSEBank.scr.Export
{
    public class JsonExportVisitor : IExportVisitor
    {
        private readonly List<BankAccount> _acc = new();
        private readonly List<Operation> _ops = new();
        private readonly List<Category> _cats = new();

        public void Visit(BankAccount account) => _acc.Add(account);
        public void Visit(Operation operation) => _ops.Add(operation);
        public void Visit(Category category) => _cats.Add(category);

        public void Save(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath)) Directory.CreateDirectory(filePath);
            var root = new JsonDataRoot
            {
                Accounts = _acc,
                Operations = _ops,
                Categories = _cats
            };
            var text = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, text, Encoding.UTF8);
            Console.WriteLine($"JSON экспорт выполнен → {filePath}");
        }
    }
}