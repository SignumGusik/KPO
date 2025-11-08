namespace HSEBank.scr.Export
{
    public interface IVisitable
    {
        void Accept(IExportVisitor visitor);
    }
}