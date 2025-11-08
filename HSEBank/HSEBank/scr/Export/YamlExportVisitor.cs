using System.Text;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Infrastructure.Serializers;

namespace HSEBank.scr.Export
{
    public class YamlExportVisitor : IExportVisitor
    {
        private readonly List<BankAccount> _acc = new();
        private readonly List<Operation> _ops = new();
        private readonly List<Category> _cats = new();

        public void Visit(BankAccount account) => _acc.Add(account);
        public void Visit(Operation operation) => _ops.Add(operation);
        public void Visit(Category category) => _cats.Add(category);

        public void Save(string filePath)
        {
            var yaml = new YamlSerializer();
            var sb = new StringBuilder();
            sb.AppendLine(yaml.Serialize(_acc));
            sb.AppendLine(yaml.Serialize(_ops));
            sb.AppendLine(yaml.Serialize(_cats));
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            Console.WriteLine($"YAML экспорт выполнен → {filePath}");
        }
    }
}