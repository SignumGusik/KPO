using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Export
{
    public interface IExportVisitor
    {
        void Visit(BankAccount account);
        void Visit(Operation operation);
        void Visit(Category category);
        
        void Save(string pathOrFolder);
    }
}