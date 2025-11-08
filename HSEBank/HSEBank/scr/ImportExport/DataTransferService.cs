using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Export;
using HSEBank.scr.ImportExport.Template;
using HSEBank.scr.Ports;

namespace HSEBank.scr.ImportExport;

// Фасад для операций импорта/экспорта данных.
public class DataTransferService
{
    private readonly IBankAccountRepository _accountRepo;
    private readonly IOperationRepository _operationRepo;
    private readonly ICategoryRepository _categoryRepo;

    public DataTransferService(
        IBankAccountRepository accountRepo,
        IOperationRepository operationRepo,
        ICategoryRepository categoryRepo)
    {
        _accountRepo = accountRepo;
        _operationRepo = operationRepo;
        _categoryRepo = categoryRepo;
    }

    // Импорт всех данных из заданного формата и пути
    public void ImportAll(string format, string path)
    {
        format = format.ToLowerInvariant();

        ImportTemplate importer = format switch
        {
            "csv"  => new CsvImporter(_accountRepo, _operationRepo, _categoryRepo),
            "json" => new JsonImporter(_accountRepo, _operationRepo, _categoryRepo),
            "yaml" => new YamlImporter(_accountRepo, _operationRepo, _categoryRepo),
            _ => throw new ArgumentException("Неизвестный формат. Поддерживаются CSV, JSON, YAML.")
        };

        var res = importer.Run(path);
        Console.WriteLine($"Импортировано: {res.Accounts} счетов, {res.Operations} операций, {res.Categories} категорий");
    }

    // Экспорт всех данных в выбранный формат
    public void ExportAll(string format, string path)
    {
        format = format.ToLowerInvariant();

        IExportVisitor visitor = format switch
        {
            "csv"  => new CsvExportVisitor(),
            "json" => new JsonExportVisitor(),
            "yaml" => new YamlExportVisitor(),
            _ => throw new ArgumentException("Неизвестный формат. Поддерживаются CSV, JSON, YAML.")
        };

        foreach (var a in _accountRepo.GetAll() ?? Enumerable.Empty<BankAccount>()) visitor.Visit(a);
        foreach (var o in _operationRepo.GetAll() ?? Enumerable.Empty<Operation>())  visitor.Visit(o);
        foreach (var c in _categoryRepo.GetAll() ?? Enumerable.Empty<Category>())    visitor.Visit(c);

        visitor.Save(path);
    }
}