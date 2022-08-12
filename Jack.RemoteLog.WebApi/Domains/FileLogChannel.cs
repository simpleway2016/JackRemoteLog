using Jack.RemoteLog.WebApi.Dtos;
using Jack.RemoteLog.WebApi.Infrastructures;

namespace Jack.RemoteLog.WebApi.Domains
{
    public class FileLogChannel : ILogChannel
    {
    
        public string ApplicationContext { get; }
        ISourceContextCollection _sourceContexts;
        ILogContentWriter _contentWriter;
        ILogContentReader _contentReader;
        public FileLogChannel(string applicationContext)
        {
            this.ApplicationContext = applicationContext;
            var folderPath = Global.Configuration["DataPath"] + "/" + applicationContext;
            var logFolderPath = $"{folderPath}/logs";

            if(Directory.Exists(logFolderPath) == false)
            {
                Directory.CreateDirectory(logFolderPath);
            }
            _sourceContexts = new FileSourceContextCollection(folderPath);
            _contentWriter = new FileLogContentWriter(logFolderPath);
            _contentReader = new FileLogContentReader(logFolderPath);
        }

        public void WriteLog(WriteLogModel request)
        {
            _contentWriter.Write(request);
            _sourceContexts.Add(request.SourceContext);
        }

        public LogItem[] Read(string sourceContext, long startTimeStamp, long? endTimeStamp, string keyWord)
        {
            return _contentReader.Read(sourceContext, startTimeStamp, endTimeStamp, keyWord);
        }

        public string[] GetAllSourceContext()
        {
            return _sourceContexts.ToArray();
        }
    }
}
