namespace HSEBank.scr.Events;

// интерфейс для всех доменных событий
public interface IEvent { }

// интерфейс обработчика события определённого типа
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    void Handle(TEvent ev);
}