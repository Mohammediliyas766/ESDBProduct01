namespace Project1.Events
{
    public class ProductEvent
    {
        public record ProductCreated(Guid Id, string Name, decimal Price, string Description);
        public record ProductUpdated(Guid Id, string Name, decimal Price, string Description);
        public record ProductDeleted(Guid Id);

    }
}
