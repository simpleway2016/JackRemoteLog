namespace Jack.RemoteLog.WebApi.Dtos
{
    public class SearchRequestBody
    {
        public string AppContext { get; set; }
        public string[] Sources { get; set; }
        public LogLevel[] Levels { get; set; }
        public long Start { get; set; }
        public long? End { get; set; }
        public string[] KeyWords { get; set; }
        public string[] TraceIds { get; set; }
        public string[] TraceNames { get; set; }
    }
}
