public interface IEventStoreService
{
    Task AppendEventAsync<T>(string streamName, T eventData);
    Task<IEnumerable<T>> ReadEventsAsync<T>(string streamName);
    Task<IEnumerable<object>> GetAllEventsAsync(string streamName);  // New method for reading all events
    Task<IEnumerable<object>> ReadGroupedEventsAsync();
}