namespace HSEBank.scr.Infrastructure;

public interface IServiceProvider
{
    T GetService<T>();
    void RegisterSingleton<TService>(TService instance);
    void RegisterFactory<TService>(Func<IServiceProvider, TService> factory);
}