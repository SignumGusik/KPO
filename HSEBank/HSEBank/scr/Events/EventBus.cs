namespace HSEBank.scr.Events;

/// хранит подписчиков по типу события и
/// синхронно вызывает их при публикации.
public class EventBus
{
    private readonly Dictionary<Type, List<object>> _handlers = new();

    // Подписка обработчика на события типа TEvent
    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<object>();
        _handlers[eventType].Add(handler);
    }

    /// публикация события
    /// если подписчиков нет - выходит
    public void Publish<TEvent>(TEvent ev) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType)) return;
        foreach (var handler in _handlers[eventType].OfType<IEventHandler<TEvent>>())
            handler.Handle(ev);
    }
}