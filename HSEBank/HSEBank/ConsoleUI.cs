using HSEBank.scr.Application.Analytics;
using HSEBank.scr.Application.Commands;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Facades;
using HSEBank.scr.ImportExport;
using HSEBank.scr.Reports;
using HSEBank.scr.Ports;
using HSEBank.scr.Services;
using IServiceProvider = HSEBank.scr.Infrastructure.IServiceProvider;

namespace HSEBank;

// Главный консольный интерфейс приложения
public class ConsoleUi
{
    private readonly IAccountFacade _accountFacade;
    private readonly IOperationFacade _operationFacade;
    private readonly ICategoryFacade _categoryFacade;
    private readonly IAccountService _accountService;
    private readonly DataTransferService _dataTransferService;
    private readonly CommandManager _cmd;

    public ConsoleUi(IServiceProvider container)
    {
        _accountService = container.GetService<IAccountService>();
        _accountFacade = container.GetService<IAccountFacade>();
        _operationFacade = container.GetService<IOperationFacade>();
        _categoryFacade = container.GetService<ICategoryFacade>();
        _cmd = new CommandManager(new ConsoleLogger());

        var analyticsContext = new AnalyticsContext(new IncomeVsExpenseStrategy());
        if (analyticsContext == null) throw new ArgumentNullException(nameof(analyticsContext));
        _dataTransferService = new DataTransferService(
            container.GetService<IBankAccountRepository>(),
            container.GetService<IOperationRepository>(),
            container.GetService<ICategoryRepository>());
    }

    /// показывает главное меню и по выбору пользователя
    /// вызывает соответствующие подменю/операции
    public void Start()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== HSE Finance ===");
            Console.ResetColor();
            Console.WriteLine("1. Счета");
            Console.WriteLine("2. Операции");
            Console.WriteLine("3. Категории");
            Console.WriteLine("4. Аналитика");
            Console.WriteLine("5. Отчёт");
            Console.WriteLine("6. Импорт / Экспорт данных");
            Console.WriteLine("7. Пересчёт баланса");
            Console.WriteLine("8. Показать историю команд");
            Console.WriteLine("9. Undo");
            Console.WriteLine("0. Выход");

            Console.Write("Выбор: ");
            switch (Console.ReadLine())
            {
                case "1": ManageAccounts(); break;
                case "2": ManageOperations(); break;
                case "3": ManageCategories(); break;
                case "4": ShowAnalytics(); break;
                case "5": BuildReport(); break;
                case "6": DataTransferMenu(); break;
                case "7": RecalcMenu(); break;
                case "8": ShowCmdHistory(); break;
                case "9": UndoCmd(); break;
                case "0": return;
                default: Console.WriteLine("Неверный ввод."); break;
            }
        }
    }

    // Выводит историю выполненных команд из CommandManager
    private void ShowCmdHistory()
    {
        Console.Clear();
        _cmd.ShowHistory();
        Console.WriteLine("\nНажмите Enter для возврата...");
        Console.ReadLine();
    }
    // Отменяет последнюю команду
    private void UndoCmd()
    {
        _cmd.UndoLastCommand();
        Console.WriteLine("\nНажмите Enter для возврата...");
        Console.ReadLine();
    }

    // Меню для операций импорта/экспорта
    private void DataTransferMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Экспорт данных");
            Console.WriteLine("2. Импорт данных");
            Console.WriteLine("0. Назад");
            Console.Write("Выбор: ");
            var choice = Console.ReadLine();
            if (choice == "0") return;

            Console.Write("Формат (CSV/JSON/YAML): ");
            var format = Console.ReadLine()?.ToLower();

            string path;
            if (choice == "1")
            {
                path = format == "csv"
                    ? ReadPath("Куда сохранить CSV-файлы (папка): ")
                    : ReadPath("Путь к экспортируемому файлу: ");
                _cmd.ExecuteCommand(new ActionCommand("Экспорт данных",
                    @do: () =>
                    {
                        if (format != null) _dataTransferService.ExportAll(format, path);
                    }));
            }
            else if (choice == "2")
            {
                path = format == "csv"
                    ? ReadPath("Папка, где лежат CSV-файлы: ")
                    : ReadPath("Путь к импортируемому файлу: ");
                _cmd.ExecuteCommand(new ActionCommand("Импорт данных",
                    @do: () =>
                    {
                        if (format != null) _dataTransferService.ImportAll(format, path);
                    }));
            }
            Console.WriteLine("Нажмите Enter для возврата...");
            Console.ReadLine();
        }
    }
    
    // Читает путь от пользователя
    private string ReadPath(string prompt)
    {
        Console.Write(prompt);
        var path = Console.ReadLine();
        return string.IsNullOrWhiteSpace(path) ? Directory.GetCurrentDirectory() : path;
    }

    // Меню пересчёта баланса
    private void RecalcMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Пересчёт баланса ===");
        Console.WriteLine("1. Автоматически по всем счетам");
        Console.WriteLine("2. Ручной по выбранному счёту");
        Console.WriteLine("0. Назад");
        Console.Write("Выбор: ");
        var answer = Console.ReadLine();
        switch (answer)
        {
            case "1":
                _cmd.ExecuteCommand(new ActionCommand(
                    "Пересчёт балансов (все)",
                    @do: () =>
                    {
                        foreach (var acc in _accountFacade.GetAllAccounts()!)
                            _accountService.RecalculateBalance(acc.Id);
                        Console.WriteLine("Балансы всех счетов пересчитаны");
                    }
                ));
                Console.ReadLine();
                break;
            case "2":
                var accs = (_accountFacade.GetAllAccounts() ?? Array.Empty<BankAccount>()).ToList();
                for (int i = 0; i < accs.Count; i++)
                    Console.WriteLine($"{i + 1}. {accs[i].Name}");
                Console.Write("Выберите счёт: ");
                if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= accs.Count)
                {
                    _cmd.ExecuteCommand(new ActionCommand(
                        $"Пересчёт баланса ({accs[idx - 1].Name})",
                        @do: () => _accountService.RecalculateBalance(accs[idx - 1].Id)
                    ));
                    Console.WriteLine($"Баланс счёта {accs[idx - 1].Name} пересчитан.");
                }
                else Console.WriteLine("Неверный выбор.");
                Console.ReadLine();
                break;
            case "0": return;
        }
    }

    /// Подменю управления счетами:
    /// создание,
    /// отображение,
    /// удаление,
    /// редактирование
    private void ManageAccounts()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--- Управление счетами ---");
            Console.ResetColor();
            Console.WriteLine("1. Создать счёт");
            Console.WriteLine("2. Показать все счета");
            Console.WriteLine("3. Удалить счёт");
            Console.WriteLine("4. Редактировать счёт");
            Console.WriteLine("0. Назад");

            Console.Write("Выбор: ");
            switch (Console.ReadLine())
            {
                case "1":
                    CreateAccount();
                    break;
                case "2":
                    ShowAccounts();
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    break;
                case "3":
                    DeleteAccount();
                    break;
                case "4":
                    EditAccount();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Неверный ввод.");
                    break;
            }
        }
    }
    //Создать счёт
    private void CreateAccount()
    {
        Console.Clear();
        Console.Write("Введите имя счёта: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Имя не может быть пустым."); return; }
        Console.Write("Введите стартовый баланс: ");
        if (!double.TryParse(Console.ReadLine(), out var balance) || balance < 0)
        {
            Console.WriteLine("Некорректная сумма.");
            return;
        }

        var category = _categoryFacade.Create("Начальный баланс", "Income");

        BankAccount? created = null;
        _cmd.ExecuteCommand(new ActionCommand(
            "Создание счёта",
            @do: () =>
            {
                created = _accountFacade.CreateAccountWithInitialOperation(name, balance, category.Id);
                Console.WriteLine($"Счёт '{created!.Name}' создан с балансом {created.Balance:F2}.");
            },
            undo: () =>
            {
                if (created != null) _accountFacade.DeleteAccount(created);
            }));

        Console.WriteLine("Нажмите Enter для продолжения...");
        Console.ReadLine();
    }
    // переименовать существующий счёт
    private void EditAccount()
    {
        Console.Clear();
        var accounts = (_accountFacade.GetAllAccounts() ?? Array.Empty<BankAccount>()).ToList();
        if (!accounts.Any())
        {
            Console.WriteLine("Нет счетов.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Выберите счёт для редактирования:");
        for (int i = 0; i < accounts.Count; i++)
            Console.WriteLine($"{i + 1}. {accounts[i].Name} ({accounts[i].Balance:F2})");

        if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > accounts.Count)
        {
            Console.WriteLine("Неверный выбор.");
            Console.ReadLine();
            return;
        }

        var acc = accounts[idx - 1];
        var oldName = acc.Name;

        Console.Write("Новое имя счёта: ");
        var newName = Console.ReadLine() ?? oldName;
        if (string.IsNullOrWhiteSpace(newName)) newName = oldName;

        _cmd.ExecuteCommand(new ActionCommand(
            "Редактирование счёта",
            @do: () => _accountService.RenameAccount(acc.Id, newName),
            undo: () => _accountService.RenameAccount(acc.Id, oldName)
        ));

        Console.WriteLine("Счёт обновлён.");
        Console.WriteLine("Нажмите Enter для продолжения...");
        Console.ReadLine();
    }
    
    // Выводит в консоль список всех счетов и их балансы
    private void ShowAccounts()
    {
        Console.Clear();
        var accounts = (_accountFacade.GetAllAccounts() ?? Array.Empty<BankAccount>()).ToList();
        if (!accounts.Any())
        {
            Console.WriteLine("Нет счетов.");
            return;
        }

        Console.WriteLine("\nВаши счета:");
        foreach (var a in accounts)
            Console.WriteLine($"- {a.Name}: {a.Balance:F2}");
    }
    // Удаляет выбранный счёт
    private void DeleteAccount()
    {
        Console.Clear();
        var accounts = (_accountFacade.GetAllAccounts() ?? Array.Empty<BankAccount>()).ToList();
        if (!accounts.Any())
        {
            Console.WriteLine("Нет счетов для удаления.");
            return;
        }

        Console.WriteLine("Выберите счёт для удаления:");
        for (int i = 0; i < accounts.Count; i++)
            Console.WriteLine($"{i + 1}. {accounts[i].Name}");

        if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= accounts.Count)
        {
            var acc = accounts[index - 1];
            _cmd.ExecuteCommand(new ActionCommand(
                "Удаление счёта",
                @do: () =>
                {
                    _accountFacade.DeleteAccount(acc);
                    Console.WriteLine($"Счёт '{acc.Name}' удалён.");
                },
                undo: () =>
                {
                    _accountService.RestoreAccount(acc);
                    Console.WriteLine($"Счёт '{acc.Name}' восстановлен.");
                }
            ));
            Console.WriteLine($"Счёт '{acc.Name}' удалён.");

            ShowAccounts();
        }
        else
        {
            Console.WriteLine("Неверный выбор.");
        }

        Console.WriteLine("Нажмите Enter для продолжения...");
        Console.ReadLine();
    }
    
    /// Подменю управления категориями
    /// создание, просмотр, редактирование, удаление.
    private void ManageCategories()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Добавить категорию");
            Console.WriteLine("2. Показать все категории");
            Console.WriteLine("3. Редактировать категорию");
            Console.WriteLine("4. Удалить категорию");
            Console.WriteLine("0. Назад");
            Console.Write("Выбор: ");
            switch (Console.ReadLine())
            {
                case "1": AddCategory(); break;
                case "2": ShowCategories(); break;
                case "3": EditCategory(); break;
                case "4": DeleteCategoryUi(); break;
                case "0": return;
            }
        }
    }
    // Добавляет новую категорию
    private void AddCategory()
    {
        Console.Clear();
        Console.Write("Введите имя категории: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Имя не может быть пустым."); return;}
        var type = ChooseCategoryType();
        _cmd.ExecuteCommand(new ActionCommand(
            "Создание категории",
            @do: () => _categoryFacade.Create(name, type),
            undo: () => {
                var created = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).FirstOrDefault(c => c.Name == name && c.Type == type);
                if (created != null) _categoryFacade.Delete(created.Id);
            }
        ));
        Console.WriteLine("Категория создана."); Console.ReadLine();
    }
    
    // Ввод типа категории
    private string ChooseCategoryType()
    {
        while (true)
        {
            Console.WriteLine("Тип категории: 1 — Доход  2 — Расход");
            var sel = Console.ReadLine();
            if (sel == "1") return "Income";
            if (sel == "2") return "Expense";
            Console.WriteLine("Введите 1 или 2.");
        }
    }
    // Редактирование категории: имя и тип
    private void EditCategory()
    {
        Console.Clear();
        var cats = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).ToList();
        if (!cats.Any()) { Console.WriteLine("Нет категорий."); Console.ReadLine(); return; }
        for (int i = 0; i < cats.Count; i++)
            Console.WriteLine($"{i + 1}. {cats[i].Name} ({cats[i].Type})");
        if (!int.TryParse(Console.ReadLine(), out var idx) || idx < 1 || idx > cats.Count)
        {
            Console.WriteLine("Неверный выбор."); Console.ReadLine(); return;
        }
        var cat = cats[idx - 1];
        var oldName = cat.Name;
        var oldType = cat.Type;
        Console.Write("Новое имя категории [Enter = без изменений]: ");
        var newName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(newName)) newName = oldName;
        Console.WriteLine("Выберите новый тип категории:");
        Console.WriteLine("1. Доход");
        Console.WriteLine("2. Расход");
        var typeSel = Console.ReadLine();
        string newType = oldType;
        if (typeSel == "1") newType = "Income";
        else if (typeSel == "2") newType = "Expense";
        _cmd.ExecuteCommand(new ActionCommand(
            "Редактирование категории",
            @do: () => _categoryFacade.Update(cat.Id, newName, newType),
            undo: () => _categoryFacade.Update(cat.Id, oldName, oldType)
        ));
        Console.WriteLine("Категория обновлена."); Console.ReadLine();
    }
    
    // Выводит список всех категорий
    private void ShowCategories()
    {
        Console.Clear();
        var cats = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).ToList();
        if (!cats.Any()) { Console.WriteLine("Нет категорий."); }
        else foreach (var c in cats)
            Console.WriteLine($"- {c.Name} ({c.Type})");
        Console.WriteLine("Нажмите Enter для возврата...");
        Console.ReadLine();
    }
    // Удаляет выбранную категорию
    private void DeleteCategoryUi()
    {
        Console.Clear();
        var cats = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).ToList();
        if (!cats.Any()) { Console.WriteLine("Нет категорий."); Console.ReadLine(); return; }
        for (int i = 0; i < cats.Count; i++)
            Console.WriteLine($"{i + 1}. {cats[i].Name} ({cats[i].Type})");
        if (!int.TryParse(Console.ReadLine(), out var idx) || idx < 1 || idx > cats.Count)
        {
            Console.WriteLine("Неверный выбор."); Console.ReadLine(); return;
        }
        var cat = cats[idx - 1];
        _cmd.ExecuteCommand(new ActionCommand(
            "Удаление категории",
            @do: () => _categoryFacade.Delete(cat.Id),
            undo: () => _categoryFacade.Create(cat.Name, cat.Type)
        ));
        Console.WriteLine("Категория удалена."); Console.ReadLine();
    }
    /// Подменю для работы с операциями:
    /// добавление, просмотр, редактирование, удаление.
    private void ManageOperations()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- Операции ---");
            Console.ResetColor();
            Console.WriteLine("1. Добавить операцию");
            Console.WriteLine("2. Показать все операции");
            Console.WriteLine("3. Редактировать операцию");
            Console.WriteLine("4. Удалить операцию");
            Console.WriteLine("0. Назад");

            Console.Write("Выбор: ");
            switch (Console.ReadLine())
            {
                case "1": AddOperation(); break;
                case "2": ShowOperations(); break;
                case "3": EditOperation(); break;
                case "4": DeleteOperationUi(); break;
                case "0": return;
            }
        }
    }
    // Добавление новой операции
    private void AddOperation()
    {
        Console.Clear();
        var accounts = (_accountFacade.GetAllAccounts() ?? Array.Empty<BankAccount>()).ToList();
        if (!accounts.Any()) { Console.WriteLine("Нет счетов."); Console.ReadLine(); return; }
        Console.WriteLine("Выберите счёт:");
        for (int i = 0; i < accounts.Count; i++)
            Console.WriteLine($"{i + 1}. {accounts[i].Name} ({accounts[i].Balance})");
        int accId;
        if (!int.TryParse(Console.ReadLine(), out accId) || accId < 1 || accId > accounts.Count)
        {
            Console.WriteLine("Ошибка."); Console.ReadLine(); return;
        }
        var acc = accounts[accId - 1];
        var type = ChooseCategoryType();
        var categories = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).Where(c => c.Type == type).ToList();
        Category cat;
        if (categories.Any())
        {
            Console.WriteLine("Выберите категорию:");
            for (int i = 0; i < categories.Count; i++) Console.WriteLine($"{i + 1}. {categories[i].Name}");
            Console.WriteLine("0. Создать новую категорию");
            int catIdx;
            if (int.TryParse(Console.ReadLine(), out catIdx) && catIdx > 0 && catIdx <= categories.Count)
                cat = categories[catIdx - 1];
            else if (catIdx == 0)
                cat = AddCategoryDirect(type);
            else { Console.WriteLine("Ошибка."); Console.ReadLine(); return; }
        }
        else
        {
            Console.WriteLine("Нет категорий данного типа. Создаем новую...");
            cat = AddCategoryDirect(type);
        }
        double amount;
        do
        {
            Console.Write("Сумма: ");
        }
        while (!double.TryParse(Console.ReadLine(), out amount) || amount <= 0);
        Console.Write("Описание (необязательно): ");
        var desc = Console.ReadLine();

        Operation? createdOp = null;
        _cmd.ExecuteCommand(new ActionCommand(
            "Создание операции",
            @do: () => {
                createdOp = _operationFacade.Create(type, amount, DateTime.Now, cat.Id, acc.Id, desc ?? "");
                Console.WriteLine($"Операция {type} ({cat.Name}) на {amount:F2} добавлена! Новый баланс: {_accountFacade.GetAccount(acc.Id).Balance:F2}");
            },
            undo: () => {
                if (createdOp != null) _operationFacade.Delete(createdOp);
            }
        ));
        Console.WriteLine("Нажмите Enter для возврата...");
        Console.ReadLine();
    }
    private Category AddCategoryDirect(string type)
    {
        while (true)
        {
            Console.Write("Название категории: ");
            var name = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(name))
                return _categoryFacade.Create(name, type);

            Console.WriteLine("Имя не может быть пустым. Попробуйте снова.");
        }
    }
    // Позволяет пользователю выбрать операцию из списка.
    private Operation? ChooseOperation()
    {
        var operations = (_operationFacade.GetAll() ?? Array.Empty<Operation>()).ToList();
        if (!operations.Any())
        {
            Console.WriteLine("Нет операций.");
            Console.ReadLine();
            return null;
        }

        var categories = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).ToDictionary(c => c.Id, c => c.Name);
        Console.WriteLine("Выберите операцию:");
        for (int i = 0; i < operations.Count; i++)
        {
            var o = operations[i];
            var catName = categories.TryGetValue(o.CategoryId, out var n) ? n : "Неизвестная категория";
            Console.WriteLine($"{i + 1}. {o.Date:dd.MM.yyyy} | {o.Type,-7} | {o.Amount,10:F2} | {catName} | {o.Description}");
        }
        if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > operations.Count)
        {
            Console.WriteLine("Неверный выбор.");
            Console.ReadLine();
            return null;
        }
        return operations[idx - 1];
    }
    
    // Редактирование операции
    private void EditOperation()
    {
        Console.Clear();
        var op = ChooseOperation();
        if (op == null) return;
        var oldType = op.Type;
        var oldAmount = op.Amount;
        var oldCatId = op.CategoryId;
        var oldDesc = op.Description;
        Console.WriteLine("Выберите новый тип:");
        Console.WriteLine("1. Доход");
        Console.WriteLine("2. Расход");
        Console.WriteLine("Enter — без изменений");
        var typeSel = Console.ReadLine();
        string newType = oldType;
        if (typeSel == "1") newType = "Income";
        else if (typeSel == "2") newType = "Expense";
        Console.Write("Новая сумма [Enter=без изменений]: ");
        var amountStr = Console.ReadLine();
        double amount = oldAmount;
        if (!string.IsNullOrWhiteSpace(amountStr)) double.TryParse(amountStr, out amount);
        Guid newCatId = oldCatId;
        Console.WriteLine("Оставить категорию? (Enter — да / 0 — выбрать новую)");
        var choose = Console.ReadLine();
        if (choose == "0")
        {
            var cats = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).Where(c => c.Type == newType).ToList();
            if (!cats.Any())
            {
                var newCat = AddCategoryDirect(newType);
                newCatId = newCat.Id;
            }
            else
            {
                Console.WriteLine("Выберите категорию:");
                for (int i = 0; i < cats.Count; i++) Console.WriteLine($"{i + 1}. {cats[i].Name}");
                if (int.TryParse(Console.ReadLine(), out int ci) && ci >= 1 && ci <= cats.Count)
                    newCatId = cats[ci - 1].Id;
            }
        }
        Console.Write("Новое описание [Enter=без изменений]: ");
        var desc = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(desc)) desc = oldDesc;
        _cmd.ExecuteCommand(new ActionCommand(
            "Редактирование операции",
            @do: () => _operationFacade.Update(op.Id, newType, amount, op.Date, newCatId, desc!),
            undo: () => _operationFacade.Update(op.Id, oldType, oldAmount, op.Date, oldCatId, oldDesc)
        ));
        Console.WriteLine("Операция обновлена.");
        Console.WriteLine("Нажмите Enter для возврата...");
        Console.ReadLine();
    }
    // Удаление операции
    private void DeleteOperationUi()
    {
        Console.Clear();
        var op = ChooseOperation();
        if (op == null) return;
        _cmd.ExecuteCommand(new ActionCommand(
            "Удаление операции",
            @do: () => _operationFacade.Delete(op),
            undo: () => _operationFacade.Create(op.Type, op.Amount, op.Date, op.CategoryId, op.AccountId, op.Description)
        ));
        Console.WriteLine("Операция удалена.");
        Console.WriteLine("Нажмите Enter для возврата...");
        Console.ReadLine();
    }
    // Вывод списка всех операций с датой, типом, суммой, категорией и описанием.
    private void ShowOperations()
    {
        Console.Clear();
        var operations = (_operationFacade.GetAll() ?? Array.Empty<Operation>()).ToList();
        var categories = (_categoryFacade.GetAll() ?? Array.Empty<Category>()).ToDictionary(c => c.Id, c => c.Name);
        if (!operations.Any())
        {
            Console.WriteLine("Нет операций.");
            Console.ReadLine(); return;
        }
        Console.WriteLine("\nИстория операций:");
        foreach (var o in operations)
        {
            var catName = categories.TryGetValue(o.CategoryId, out var name) ? name : "Неизвестная категория";
            Console.WriteLine(
                $"{o.Date:dd.MM.yyyy} | {o.Type,-7} | {o.Amount,10:F2} | категория: {catName} | {o.Description}");
        }
        Console.WriteLine("Нажмите Enter для возврата...");
        Console.ReadLine();
    }

    private (DateTime, DateTime) SelectPeriod()
    {
        Console.Write("С какого дня? (yyyy-MM-dd): ");
        var start = DateTime.TryParse(Console.ReadLine(), out var startD) ? startD : DateTime.MinValue;
        Console.Write("По какую дату? (yyyy-MM-dd): ");
        var end = DateTime.TryParse(Console.ReadLine(), out var endD) ? endD : DateTime.MaxValue;
        return (start, end);
    }

    /// Помогает выбрать период дат: "С какого дня" и "По какую дату"
    /// Если формат неверный, берутся MinValue/MaxValue
    private void ShowAnalytics()
    {
        Console.Clear();
        var categories = _categoryFacade.GetAll();
        var (dateFrom, dateTo) = SelectPeriod();
        var operations = (_operationFacade.GetAll() ?? Array.Empty<Operation>())
            .Where(o => o.Date >= dateFrom && o.Date <= dateTo)
            .ToList();

        Console.WriteLine("\n1. Доходы vs Расходы");
        Console.WriteLine("2. По категориям");
        Console.WriteLine("3. По месяцам");
        Console.Write("Выберите стратегию: ");
        var choice = Console.ReadLine();

        IAnalyticsStrategy strategy;
        switch (choice)
        {
            case "1": strategy = new IncomeVsExpenseStrategy(); break;
            case "2": strategy = new ByCategoryStrategy(categories); break;
            case "3": strategy = new MonthlyTrendStrategy(); break;
            default:
                Console.WriteLine("Неверная стратегия.");
                return;
        }

        var analyticsContext = new AnalyticsContext(strategy);
        var analyticsCommand = new AnalyticsCommand(analyticsContext, operations);
        var timedAnalytics = new TimeCommandDecorator(analyticsCommand);
        timedAnalytics.Execute();
        Console.WriteLine("Нажмите Enter для возврата...");
        Console.ReadLine();
    }

    // Формирует отчёт по текущим операциям
    private void BuildReport()
    {
        Console.Clear();
        var operations = _operationFacade.GetAll();
        var categories = _categoryFacade.GetAll();

        if (operations != null)
        {
            var enumerable = operations as Operation[] ?? operations.ToArray();
            if (!enumerable.Any())
            {
                Console.WriteLine("Нет операций для отчёта.");
                Console.WriteLine("Нажмите Enter для возврата...");
                Console.ReadLine();
                return;
            }

            var strategy = new ByCategoryStrategy(categories);
            var analytics = strategy.Analyze(enumerable);

            var builder = new ConsoleReportBuilder();
            var director = new ReportDirector(builder);

            var report = director.BuildAnalyticsReport(analytics);
            report.Render();
        }

        Console.WriteLine("Отчёт построен успешно.");
        Console.WriteLine("Нажмите Enter для возврата...");
        Console.ReadLine();
    }

    /// Команда-обёртка для запуска аналитики
    /// Реализует ICommand, чтобы можно было оборачивать её в декораторы.
    public class AnalyticsCommand : ICommand
    {
        private readonly AnalyticsContext _context;
        private readonly IEnumerable<Operation>? _ops;
        public string Name => "Аналитика";
        public AnalyticsCommand(AnalyticsContext context, IEnumerable<Operation>? ops)
        {
            _context = context;
            _ops = ops;
        }
        public void Execute()
        {
            var result = _context.Execute(_ops);
            result.Print();
        }
        public void Undo() { }
    }
}