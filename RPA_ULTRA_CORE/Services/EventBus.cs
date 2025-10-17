using RPA_ULTRA_CORE.Models.Geometry;

namespace RPA_ULTRA_CORE.Services
{
    /// <summary>
    /// EventBus Singleton para comunicação entre componentes
    /// </summary>
    public sealed class EventBus
    {
        private static readonly Lazy<EventBus> _instance = new(() => new EventBus());
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public static EventBus Instance => _instance.Value;

        private EventBus() { }

        /// <summary>
        /// Inscreve para receber eventos de um tipo
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (!_subscribers.ContainsKey(eventType))
                _subscribers[eventType] = new List<Delegate>();

            _subscribers[eventType].Add(handler);
        }

        /// <summary>
        /// Remove inscrição
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (_subscribers.ContainsKey(eventType))
                _subscribers[eventType].Remove(handler);
        }

        /// <summary>
        /// Publica evento para todos os inscritos
        /// </summary>
        public void Publish<TEvent>(TEvent eventData) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (!_subscribers.ContainsKey(eventType))
                return;

            foreach (var subscriber in _subscribers[eventType].ToList())
            {
                (subscriber as Action<TEvent>)?.Invoke(eventData);
            }
        }
    }

    // Eventos do canvas
    public class CanvasInvalidatedEvent { }
    public class ShapeAddedEvent { public BaseShape? Shape { get; set; } }
    public class ShapeRemovedEvent { public BaseShape? Shape { get; set; } }
    public class SelectionChangedEvent { public BaseShape? Shape { get; set; } }
}