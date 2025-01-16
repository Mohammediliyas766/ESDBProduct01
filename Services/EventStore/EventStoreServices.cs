using EventStore.Client;
using System.Text.Json;
using System.Text;

public class EventStoreService : IEventStoreService
{
    private readonly EventStoreClient _eventStoreClient;

    public EventStoreService(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task AppendEventAsync<T>(string streamName, T eventData)
    {
        var eventJson = JsonSerializer.Serialize(eventData);
        var eventDataObject = new EventData(
            Uuid.NewUuid(),
            eventData.GetType().Name,
            Encoding.UTF8.GetBytes(eventJson)
        );
        await _eventStoreClient.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            new[] { eventDataObject }
        );
    }

    public async Task<IEnumerable<T>> ReadEventsAsync<T>(string streamName)
    {
        var result = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start
        );
        var events = new List<T>();
        await foreach (var @event in result)
        {
            var eventData = Encoding.UTF8.GetString(@event.Event.Data.ToArray());
            var deserializedEvent = JsonSerializer.Deserialize<T>(eventData);
            if (deserializedEvent != null)
                events.Add(deserializedEvent);
        }
        return events;
    }

    public async Task<IEnumerable<object>> GetAllEventsAsync(string streamName)
    {
        var result = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start
        );

        var events = new List<object>();
        await foreach (var @event in result)
        {
            var eventData = Encoding.UTF8.GetString(@event.Event.Data.ToArray());
            var eventObject = new
            {
                EventType = @event.Event.EventType,
                EventId = @event.Event.EventId.ToString(),
                Created = @event.Event.Created,
                Data = JsonSerializer.Deserialize<object>(eventData),
                Metadata = @event.Event.Metadata.Length > 0
                    ? JsonSerializer.Deserialize<object>(Encoding.UTF8.GetString(@event.Event.Metadata.ToArray()))
                    : null
            };
            events.Add(eventObject);
        }
        return events;
    }

    public async Task<IEnumerable<object>> ReadGroupedEventsAsync()
    {
        var result = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            "grouped-product-events",
            StreamPosition.Start
        );

        var groups = new List<object>();
        await foreach (var @event in result)
        {
            var eventData = Encoding.UTF8.GetString(@event.Event.Data.ToArray());
            var groupEvent = JsonSerializer.Deserialize<object>(eventData);
            if (groupEvent != null)
            {
                groups.Add(groupEvent);
            }
        }
        return groups;
    }
}