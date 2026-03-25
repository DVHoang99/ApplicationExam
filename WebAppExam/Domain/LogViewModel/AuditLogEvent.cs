namespace WebAppExam.Domain.LogViewModel
{
    public class AuditLogEvent
    {
        public string TraceId { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public int StatusCode { get; set; }
        public long Duration { get; set; }
        public string? RequestBody { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
