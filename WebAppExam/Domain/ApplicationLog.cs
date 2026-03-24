namespace WebAppExam.Domain
{
    public class ApplicationLog
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Level { get; init; } = "Info";
        public string Message { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
