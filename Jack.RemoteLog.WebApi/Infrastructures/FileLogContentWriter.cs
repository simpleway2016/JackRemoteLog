using Jack.RemoteLog.WebApi.Dtos;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public class FileLogContentWriter : ILogContentWriter,IDisposable
    {
        bool _disposed;
        long _curTimeStamp;
        string _folderPath;
        FileStream _Writer;
        FileStream _indexWriter;
        ConcurrentQueue<WriteLogModel> _queue = new ConcurrentQueue<WriteLogModel>();
        AutoResetEvent _event = new AutoResetEvent(false);
        static byte[] HEADER = Encoding.ASCII.GetBytes("LOG");
        public FileLogContentWriter(string folerpath)
        {
            _folderPath = folerpath;
            new Thread(runInThread).Start();
        }

        void runInThread()
        {
            while (!_disposed)
            {
                if (_queue.TryDequeue(out WriteLogModel model))
                {
                    writeToFile(model);
                    Thread.Sleep(1);
                }
                else
                {
                    _event.WaitOne();
                }
            }

            _Writer?.Dispose();
            _Writer = null;

            _indexWriter?.Dispose();
            _indexWriter = null;
        }

        public void Write(WriteLogModel writeLogModel)
        {
            _queue.Enqueue(writeLogModel);
            _event.Set();
        }

        unsafe void writeToFile(WriteLogModel writeLogModel)
        {
            if (writeLogModel.SourceContext.Contains("\r"))
                writeLogModel.SourceContext = writeLogModel.SourceContext.Replace("\r", "");

            var hour = writeLogModel.Timestamp - writeLogModel.Timestamp % 3600000L;
            if (_curTimeStamp != hour)
            {
                _curTimeStamp = hour;
                _Writer?.Dispose();
                _indexWriter?.Dispose();

                _Writer = new FileStream($"{_folderPath}/{hour}.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                _Writer.Position = _Writer.Length;

                _indexWriter = new FileStream($"{_folderPath}/{hour}.index.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                _indexWriter.Position = _indexWriter.Length;
            }

            var position = _Writer.Position;
            string writeContent = $"{writeLogModel.SourceContext}\r\n{writeLogModel.Content}";
            var bs = Encoding.UTF8.GetBytes(writeContent);
            _Writer.Write(HEADER);
            _Writer.Write(bs);
            _Writer.Flush();
            var len = bs.Length + HEADER.Length;

            bs = new byte[38];
           
            var model = new IndexModel();
            model.Position = position;
            model.Length = len;
            model.Time = writeLogModel.Timestamp;
            model.Level = (short)writeLogModel.Level;
            model.SourceContext = Global.GetHash(writeLogModel.SourceContext);

            fixed (byte* bsPtr = bs)
            {
                Marshal.StructureToPtr(model,new IntPtr(bsPtr),true);
            }

            _indexWriter.Write(bs);
            _indexWriter.Flush();
        }

        public void Dispose()
        {
            _disposed = true;

           
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct IndexModel
    {
        public long Position;
        public int Length;
        public long Time;
        public short Level;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] SourceContext;
    }
}
