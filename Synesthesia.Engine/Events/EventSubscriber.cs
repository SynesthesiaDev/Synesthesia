namespace Synesthesia.Engine.Events;

public record EventSubscriber<T>(Action<T> Action) : IEventSubscriber;

public interface IEventSubscriber;
