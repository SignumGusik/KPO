using HSEBank.scr.Domain.Entities;
using HSEBank.scr.Events;
using HSEBank.scr.Events.Handlers;
using HSEBank.scr.Facades;
using HSEBank.scr.Infrastructure;
using HSEBank.scr.Ports;
using HSEBank.scr.Repositories.Cashing;
using HSEBank.scr.Repositories.InMemoryRepositories;
using HSEBank.scr.Services;
using IServiceProvider = HSEBank.scr.Infrastructure.IServiceProvider;


namespace HSEBank;

// регистрирует репозитории, сервисы, фасады, EventBus и обработчики событий
public class Bootstrapper
{
    public static IServiceProvider Configure()
{
    var container = new ServiceContainer();

    var eventBus = new EventBus();
    container.RegisterSingleton(eventBus);
    
    var realOps = new OperationRepositoryInMemory(new List<Operation>());
    var realAcc = new BankAccountRepositoryInMemory(new List<BankAccount>());
    var realCat = new CategoryRepositoryInMemory(new List<Category>());
    
    container.RegisterSingleton<IOperationRepository>(new OperationRepositoryCacheProxy(realOps));
    container.RegisterSingleton<IBankAccountRepository>(new BankAccountRepositoryCacheProxy(realAcc));
    container.RegisterSingleton<ICategoryRepository>(new CategoryRepositoryCacheProxy(realCat));
    
    container.RegisterFactory<IAccountService>(sp => 
        new AccountService(sp.GetService<IBankAccountRepository>(), sp.GetService<IOperationRepository>(), eventBus)
    );
    container.RegisterFactory<IOperationService>(sp => 
        new OperationService(sp.GetService<IOperationRepository>(), eventBus)
    );
    container.RegisterFactory<ICategoryService>(sp => 
        new CategoryService(sp.GetService<ICategoryRepository>())
    );
    
    container.RegisterFactory<IAccountFacade>(sp =>
        new AccountFacade(sp.GetService<IAccountService>(), sp.GetService<IOperationService>())
    );
    container.RegisterFactory<IOperationFacade>(sp =>
        new OperationFacade(sp.GetService<IOperationService>(), sp.GetService<IAccountService>())
    );
    container.RegisterFactory<ICategoryFacade>(sp =>
        new CategoryFacade(sp.GetService<ICategoryService>())
    );
    
    eventBus.Subscribe<OperationCreatedEvent>(new AuditLogHandler());
    eventBus.Subscribe<AccountDeletedEvent>(new AuditLogHandler());
    eventBus.Subscribe(new BalanceUpdateHandler(container.GetService<IAccountService>()));

    return container;
}
    
}