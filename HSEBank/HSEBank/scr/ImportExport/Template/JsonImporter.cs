using System.Text;
using System.Text.Json;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;

namespace HSEBank.scr.ImportExport.Template
{
    public sealed class JsonImporter : ImportTemplate
    {
        public JsonImporter(IBankAccountRepository a, IOperationRepository o, ICategoryRepository c)
            : base(a, o, c) { }

        protected override (string accounts, string operations, string categories) Read(string path)
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            return (text, text, text); // читаем один файл, парсинг раскладывает
        }

        protected override (IEnumerable<BankAccount> accounts, IEnumerable<Operation> operations, IEnumerable<Category> categories)
            Parse((string accounts, string operations, string categories) text)
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var root = JsonSerializer.Deserialize<JsonDataRoot>(text.accounts, opts) ?? new JsonDataRoot();
            return (
                accounts:  root.Accounts ?? new(),
                operations:root.Operations ?? new(),
                categories:root.Categories ?? new()
            );
        }
    }
}