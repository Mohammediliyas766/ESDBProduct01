using EventStore.Client;

namespace Project1.Controllers
{
    internal class EventStoreClient
    {
        private object settings;

        public EventStoreClient(object settings)
        {
            this.settings = settings;
        }

        internal async Task AppendToStreamAsync(string v, object any, EventData[] eventDatas)
        {
            // Assuming _client is an instance of EventStoreClient from EventStore.Client package
            var client = new EventStoreClient(settings);
            await client.AppendToStreamAsync(v, any, eventDatas);
        }

        internal IAsyncEnumerable<object> ReadStreamAsync(object forwards, string streamName, int start)
        {
            throw new NotImplementedException();
        }
    }
}