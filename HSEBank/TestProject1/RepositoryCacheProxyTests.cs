using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.Cashing;

namespace TestProject1
{
    public class RepositoryCacheProxyTests
    {
        private class CountingRepo<T> : IRepository<T> where T : class
        {
            public readonly List<T> Storage = new();
            public int GetAllCalls { get; private set; }
            public int GetByIdCalls { get; private set; }

            public void Add(T entity) => Storage.Add(entity);
            public void Remove(T entity) => Storage.Remove(entity);
            public IEnumerable<T>? GetAll()
            {
                GetAllCalls++;
                return Storage;
            }
            public T GetById(Guid id)
            {
                GetByIdCalls++;
                foreach (var item in Storage)
                {
                    var prop = typeof(T).GetProperty("Id");
                    if (prop != null && prop.PropertyType == typeof(Guid))
                    {
                        var v = (Guid)prop.GetValue(item)!;
                        if (v == id) return item;
                    }
                }
                throw new InvalidOperationException();
            }
        }

        [Fact]
        public void GetAll_CachedAfterFirstCall()
        {
            var inner = new CountingRepo<BankAccount>();
            inner.Add(new BankAccount { Id = Guid.NewGuid(), Name = "A", Balance = 1 });

            var proxy = new RepositoryCacheProxy<BankAccount>(inner);

            var first = proxy.GetAll();
            var second = proxy.GetAll();

            Assert.Equal(1, inner.GetAllCalls);
        }

        [Fact]
        public void Add_InvalidatesCache()
        {
            var inner = new CountingRepo<BankAccount>();
            var proxy = new RepositoryCacheProxy<BankAccount>(inner);

            proxy.Add(new BankAccount { Id = Guid.NewGuid(), Name = "A", Balance = 1 });
            proxy.GetAll();
            proxy.Add(new BankAccount { Id = Guid.NewGuid(), Name = "B", Balance = 2 });
            proxy.GetAll();

            Assert.Equal(2, inner.GetAllCalls);
        }

        [Fact]
        public void GetById_UsesPerItemCache()
        {
            var id = Guid.NewGuid();
            var inner = new CountingRepo<BankAccount>();
            inner.Add(new BankAccount { Id = id, Name = "A", Balance = 1 });
            var proxy = new RepositoryCacheProxy<BankAccount>(inner);

            var a1 = proxy.GetById(id);
            var a2 = proxy.GetById(id);

            Assert.Same(a1, a2);
            Assert.Equal(1, inner.GetByIdCalls);
        }
    }
}