using HSEBank.scr.Export;

namespace HSEBank.scr.Domain.Entities
{
    public class Category : IVisitable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }

        public void Accept(IExportVisitor v) => v.Visit(this);
    }
}