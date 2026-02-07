namespace Common.Event;

public record EventSubscriber<T>(Action<T> Action) : IEventSubscriber;

public interface IEventSubscriber;
