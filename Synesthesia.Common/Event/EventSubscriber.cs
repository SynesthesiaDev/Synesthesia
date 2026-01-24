namespace Common.Event;

public record EventSubscriber<T>(Action<T> Action);
