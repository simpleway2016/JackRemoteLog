using Jack.RemoteLog.WebApi.Dtos;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public class LuceneContentWriter : ILogContentWriter
    {
        Analyzer _analyzer;
        IndexWriter _iw ;
        DateTime _lastCommitTime = DateTime.Now;
        ILogger<LuceneContentWriter> _logger;
        bool _disposed;
        long? _deleteEndTime;
        public LuceneContentWriter(string folderPath)
        {
            _logger = Global.ServiceProvider.GetService<ILogger<LuceneContentWriter>>();
            DirectoryInfo INDEX_DIR = new DirectoryInfo(folderPath);
            _analyzer = new PanGuAnalyzer();

            var options = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, null);
            options.OpenMode = OpenMode.CREATE_OR_APPEND;

            _iw = new IndexWriter(FSDirectory.Open(INDEX_DIR), options);
            new Thread(commitThread).Start();
        }

        void commitThread()
        {
            while (!_disposed)
            {
                Thread.Sleep(2000);
                try
                {
                    if (_deleteEndTime != null)
                    {
                        var timequery = NumericRangeQuery.NewInt64Range("Timestamp", null, _deleteEndTime, false, false);
                        _iw.DeleteDocuments(timequery);
                        _deleteEndTime = null;

                        _iw.Commit();
                        _iw.Flush(true, true);
                        continue;
                    }
                    _iw.Commit();
                    _iw.Flush(true, false);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _iw.Dispose();
                _analyzer.Dispose();
            }
        }

        public void Write(WriteLogModel writeLogModel)
        {
            if (_disposed)
                return;

            //存储文档添加索引
            Document doc = new Document();

            doc.Add(new TextField("Content", writeLogModel.Content, Field.Store.YES));

            doc.AddInt32Field("SourceContextId", writeLogModel.SourceContextId , Field.Store.YES);
            doc.AddInt32Field("Level", (int)writeLogModel.Level, Field.Store.YES);
            doc.AddInt64Field("Timestamp", writeLogModel.Timestamp, Field.Store.YES);

            //将解析完成的内容存储
            _iw.AddDocument(doc , _analyzer);
        }

        public void DeleteLogs(long endTIme)
        {
            _deleteEndTime = endTIme;
        }
    }
}
