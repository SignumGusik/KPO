using HSEBank.scr.Export;

namespace HSEBank.scr.Domain.Entities
{
    public class BankAccount : IVisitable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public double Balance { get; set; }
        
        public void Accept(IExportVisitor v) => v.Visit(this);
    }
}