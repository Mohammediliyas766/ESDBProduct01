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
            throw new NotImplementedException();
        }

        internal IAsyncEnumerable<object> ReadStreamAsync(object forwards, string streamName, int start)
        {
            throw new NotImplementedException();
        }
    }
}