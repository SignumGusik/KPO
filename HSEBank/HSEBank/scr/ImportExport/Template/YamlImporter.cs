using System.Text.RegularExpressions;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Infrastructure.Serializers;
using HSEBank.scr.Ports;

namespace HSEBank.scr.ImportExport.Template
{
    public class YamlImporter : ImportTemplate
    {
        public YamlImporter(IBankAccountRepository a, IOperationRepository o, ICategoryRepository c)
            : base(a, o, c) { }

        protected override (string accounts, string operations, string categories) Read(string path)
        {
            var text = ReadFile(path);
           
            string Extract(string key)
            {
                var rx = new Regex($@"(?ms)^{key}:\s*(?:-.*?)(?=^\w+:|$)");
                var m = rx.Match(text);
                return m.Success ? m.Value : $"{key}:\n";
            }
            return (Extract("bankaccounts"), Extract("operations"), Extract("categories"));
        }

        protected override (IEnumerable<BankAccount> accounts, IEnumerable<Operation> operations, IEnumerable<Category> categories)
            Parse((string accounts, string operations, string categories) text)
        {
            var yaml = new YamlSerializer();
            return (
                yaml.Deserialize<BankAccount>(text.accounts),
                yaml.Deserialize<Operation>(text.operations),
                yaml.Deserialize<Category>(text.categories)
            );
        }
    }
}