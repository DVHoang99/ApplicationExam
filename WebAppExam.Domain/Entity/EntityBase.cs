using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain
{
    public interface IAggregateRoot { }
    public class EntityBase : AggregateRoot
    {
        public Ulid Id { get; set; } = Ulid.NewUlid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
