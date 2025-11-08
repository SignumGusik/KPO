using System.Text;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;


namespace HSEBank.scr.ImportExport.Template
{
    // результат импорта сколько сущностей каждого типа было загружено
    public record ImportResult(int Accounts, int Operations, int Categories);

    /// Абстрактный класс-шаблон для всех импортёров
    /// Общий алгоритм:
    /// 1) Read - читает текст из файла
    /// 2) Parse - превращаем текст в объекты
    /// 3) Validate - валидация
    /// 4) Upsert - вставляем/обновляем в репозиториях
    public abstract class ImportTemplate
    {
        protected readonly IBankAccountRepository Accounts;
        protected readonly IOperationRepository Operations;
        protected readonly ICategoryRepository Categories;

        protected ImportTemplate(
            IBankAccountRepository acc,
            IOperationRepository ops,
            ICategoryRepository cats)
        {
            Accounts = acc;
            Operations = ops;
            Categories = cats;
        }
        
        // импорт.
        public ImportResult Run(string path)
        {
            var textByType = Read(path);             
            var parsed = Parse(textByType);            
            Validate();                        
            var res = Upsert(parsed);                    
            return res;
        }


        /// Чтение текста для аккаунтов, операций и категорий
        /// сам решает, из одного файла читать или из нескольких
        protected abstract (string accounts, string operations, string categories) Read(string path);
        
        // Разбор текста в коллекции доменных сущностей.
        protected abstract (IEnumerable<BankAccount> accounts, IEnumerable<Operation> operations, IEnumerable<Category> categories)
            Parse((string accounts, string operations, string categories) text);

        private void Validate() { }

        // Вставка/обновление сущностей в репозиториях.
        private ImportResult Upsert((IEnumerable<BankAccount> acc, IEnumerable<Operation> ops, IEnumerable<Category> cats) data)
        {
            int a = 0, o = 0, c = 0;
            UpsertMany(Accounts, data.acc, ref a);
            UpsertMany(Operations, data.ops, ref o);
            UpsertMany(Categories, data.cats, ref c);
            return new ImportResult(a, o, c);
        }
        
        private static void UpsertMany<T>(IRepository<T> repo, IEnumerable<T> items, ref int cnt) where T : class
        {
            var idProp = typeof(T).GetProperty("Id");
            foreach (var x in items)
            {
                var idObj = idProp?.GetValue(x);
                var id = idObj is Guid g ? g : Guid.Empty;
                if (id == Guid.Empty && idProp != null) { id = Guid.NewGuid(); idProp.SetValue(x, id); }

                try
                {
                    var existed = repo.GetById(id);
                    repo.Remove(existed);
                }
                catch
                {
                    // если не нашли добавляем
                }

                repo.Add(x);
                cnt++;
            }
        }
        
        // метод чтения файла целиком в строку.
        protected static string ReadFile(string p) => File.ReadAllText(p, Encoding.UTF8);
    }
}