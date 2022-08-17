using Jack.RemoteLog.WebApi.Domains;
using Jack.RemoteLog.WebApi.Dtos;
using System.Runtime.InteropServices;
using System.Text;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public class FileLogContentReader : ILogContentReader
    {
        string _folderPath;
        public FileLogContentReader(string folerpath)
        {
            _folderPath = folerpath;
        }

        public void Dispose()
        {
        }

        public unsafe LogItem[] Read(ISourceContextCollection sourceContextes, string sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord)
        {
            var intLevel = (short)level.GetValueOrDefault();
            if (sourceContext != null && sourceContext.Contains("\r"))
                sourceContext = sourceContext.Replace("\r", "");

            var starthour = startTimeStamp - startTimeStamp % 3600000L;

            if (endTimeStamp == null)
                endTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var endhour = endTimeStamp - endTimeStamp % 3600000L;

            FileStream reader = null;
            FileStream indexReader = null;
            List<LogItem> list = new List<LogItem>();
            var findSourceId = sourceContextes.GetId(sourceContext);
            byte[] indexData = new byte[sizeof(IndexModel)];
            byte[] header = new byte[3];
            //如果日志timestamp大于lastTimestamp，则不再读取
            long lastTimestamp = long.MaxValue;
            try
            {
                for (long i = starthour; i <= endhour && list.Count < Global.PageSize; i += 3600000L)
                {
                    var filepath = $"{_folderPath}/{i}.txt";
                    var indexFilepath = $"{_folderPath}/{i}.index.txt";
                    if (File.Exists(filepath) == false || File.Exists(indexFilepath) == false)
                        continue;

                    reader = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    indexReader = new FileStream(indexFilepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                  
                    fixed (byte* indexDataPtr = indexData)
                    {
                        var ptr = new IntPtr(indexDataPtr);
                        while (true)
                        {
                            if (indexReader.Read(indexData, 0, indexData.Length) == 0)
                                break;

                            IndexModel indexItem = Marshal.PtrToStructure<IndexModel>(ptr);
                            if (indexItem.Time > lastTimestamp)
                                break;

                            if(indexItem.Time >= startTimeStamp && indexItem.Time <= endTimeStamp)
                            {
                                if(findSourceId != 0)
                                {
                                    //需要匹对sourceContext
                                    if (findSourceId != indexItem.SourceContextId)
                                    {
                                        continue;
                                    }
                                }

                                if(level != null)
                                {
                                    if(intLevel != indexItem.Level)
                                    {
                                        continue;
                                    }
                                }

                                reader.Position = indexItem.Position;
                                reader.Read(header);
                                if (Encoding.ASCII.GetString(header) != "LOG")
                                    throw new IOException("文件已损坏");

                                byte[] data = new byte[indexItem.Length - 3];
                                reader.Read(data);
                                var content = Encoding.UTF8.GetString(data);

                                if(keyWord != null)
                                {
                                    if (!content.Contains(keyWord))
                                        continue;
                                }

                                list.Add(new LogItem { 
                                    Content = content,
                                    SourceContext = sourceContextes.GetSourceContext(indexItem.SourceContextId),
                                    Level = (LogLevel)indexItem.Level,
                                    Timestamp = indexItem.Time
                                });
                                if(list.Count >= Global.PageSize)
                                {
                                    lastTimestamp = indexItem.Time;
                                }
                            }

                            if(indexItem.Time > endTimeStamp)
                            {
                                return list.ToArray();
                            }
                        }
                    }

                    reader.Dispose();
                    indexReader.Dispose();

                    reader = null;
                    indexReader = null;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                reader?.Dispose();
                indexReader?.Dispose();
            }
            return list.ToArray();
        }
    }
}
