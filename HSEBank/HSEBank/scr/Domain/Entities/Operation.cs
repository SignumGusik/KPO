using HSEBank.scr.Export;

namespace HSEBank.scr.Domain.Entities
{
    public class Operation : IVisitable
    {
        public Guid Id { get; set; }
        public required string Type { get; set; }
        public Guid AccountId { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }

        public bool Validate() => Amount > 0;

        public void Accept(IExportVisitor v) => v.Visit(this);
    }
}