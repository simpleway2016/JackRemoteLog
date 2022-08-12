namespace Jack.RemoteLog.WebApi.Dtos
{
    public class WriteLogModel
    {
        public string ApplicationContext { get; set; }
        public string SourceContext { get; set; }
        public long Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Content { get; set; }
    }
}
