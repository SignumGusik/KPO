namespace HSEBank.scr.Infrastructure;

public class ServiceContainer : IServiceProvider
{
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, Func<IServiceProvider, object>> _factories = new();

    public T GetService<T>()
    {
        var type = typeof(T);

        if (_singletons.ContainsKey(type))
            return (T)_singletons[type];

        if (_factories.ContainsKey(type))
            return (T)_factories[type](this);

        throw new Exception($"Сервис {type.Name} не зарегистрирован.");
    }

    public void RegisterSingleton<TService>(TService instance)
    {
        if (instance != null) _singletons[typeof(TService)] = instance;
    }

    public void RegisterFactory<TService>(Func<IServiceProvider, TService> factory)
    {
        _factories[typeof(TService)] = sp => factory(sp)!;
    }
}