using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.ImportExport;

public class JsonDataRoot
{
    public List<BankAccount>? Accounts { get; init; }
    public List<Operation>? Operations { get; init; }
    public List<Category>? Categories { get; init; }
}