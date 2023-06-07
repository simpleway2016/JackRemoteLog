namespace Jack.RemoteLog.WebApi.Dtos
{
    public class LogItem
    {
        public string SourceContext { get; set; }
        public long Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Content { get; set; }
        public int? TotalHits { get; set; }
        public string TraceId { get; set; }
    }
}
