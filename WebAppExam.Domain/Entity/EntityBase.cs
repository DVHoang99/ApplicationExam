namespace WebAppExam.Domain
{
    public interface IAggregateRoot { }
    public class EntityBase
    {
        public Ulid Id { get; set; } = Ulid.NewUlid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
