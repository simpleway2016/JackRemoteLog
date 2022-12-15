using Jack.RemoteLog.WebApi.Dtos;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Collections.Concurrent;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public class LuceneContentWriter : ILogContentWriter
    {
        Analyzer _analyzer;
        IndexWriter _iw ;
        DateTime _lastCommitTime = DateTime.Now;
        ILogger<LuceneContentWriter> _logger;
        ConcurrentQueue<WriteLogModel> _writingQueue;
        bool _disposed;
        long? _deleteEndTime;
        bool _isDirty;
        string _folderPath;
        public LuceneContentWriter(string folderPath)
        {
            _folderPath = folderPath;
            _logger = Global.ServiceProvider.GetService<ILogger<LuceneContentWriter>>();
            _writingQueue = new ConcurrentQueue<WriteLogModel>();
            initIndexWriter();
            
            new Thread(commitThread).Start();
        }

        void initIndexWriter()
        {
            if(_iw != null)
            {
                _iw.Dispose();
                _iw = null;

                _analyzer?.Dispose();
                _analyzer = null;
            }

            DirectoryInfo INDEX_DIR = new DirectoryInfo(_folderPath);
            _analyzer = new PanGuAnalyzer();

            var options = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, null);
            options.OpenMode = OpenMode.CREATE_OR_APPEND;

            _iw = new IndexWriter(FSDirectory.Open(INDEX_DIR), options);
        }

        void commitThread()
        {
            while (!_disposed)
            {
                Thread.Sleep(2000);
                if (_isDirty)
                {
                    _isDirty = false;
                    try
                    {
                        bool applyAllDelete = false;

                        if (_deleteEndTime != null)
                        {
                            var timequery = NumericRangeQuery.NewInt64Range("Timestamp", null, _deleteEndTime, false, false);
                            _iw.DeleteDocuments(timequery);
                            _deleteEndTime = null;

                            applyAllDelete = true;
                        }

                        int count = 0;
                        while(_writingQueue.TryDequeue(out WriteLogModel writeLogModel))
                        {
                            //存储文档添加索引
                            Document doc = new Document();

                            doc.Add(new TextField("Content", writeLogModel.Content, Field.Store.YES));

                            doc.AddInt32Field("SourceContextId", writeLogModel.SourceContextId, Field.Store.YES);
                            doc.AddInt32Field("Level", (int)writeLogModel.Level, Field.Store.YES);
                            doc.AddInt64Field("Timestamp", writeLogModel.Timestamp, Field.Store.YES);

                            //将解析完成的内容存储
                            try
                            {
                                _iw.AddDocument(doc, _analyzer);
                                count++;
                                if(count > 1000)
                                {
                                    count = 0;
                                    _iw.Commit();
                                    _iw.Flush(true, applyAllDelete);
                                    applyAllDelete = false;
                                }
                            }
                            catch (OutOfMemoryException)
                            {
                                initIndexWriter();
                                //重新放回队列
                                _writingQueue.Enqueue(writeLogModel);
                            }
                        }

                        _iw.Commit();
                        _iw.Flush(true, applyAllDelete);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "");
                    }
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

            _writingQueue.Enqueue(writeLogModel);
           
            _isDirty = true;
        }

        public void DeleteLogs(long endTIme)
        {
            _deleteEndTime = endTIme;
        }
    }
}
