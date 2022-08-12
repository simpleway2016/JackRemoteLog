using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace Jack.RemoteLog.WebApi.Domains
{
    public class FileSourceContextCollection : ISourceContextCollection,IDisposable
    {
        ConcurrentDictionary<string, bool> _dict = new ConcurrentDictionary<string, bool>();
        string _filepath;
        StreamWriter _stream;
        public FileSourceContextCollection(string folderPath)
        {
            _filepath = $"{folderPath}/sourcecontext.txt";
            if (File.Exists(_filepath))
            {
                using ( var sr = new StreamReader(new FileStream(_filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
                {
                    while (true)
                    {
                        var line = sr.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            _dict.TryAdd(line, true);
                        }
                        if (line == null)
                            break;
                    }
                }

                _stream = new StreamWriter(new FileStream(_filepath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
                _stream.BaseStream.Position = _stream.BaseStream.Length;
            }
            else
            {
                _stream = new StreamWriter(new FileStream(_filepath , FileMode.Create , FileAccess.Write , FileShare.ReadWrite),Encoding.UTF8);
            }
        }
        public void Add(string sourceContext)
        {
            if (_dict.TryAdd(sourceContext, true))
            {
                _stream.WriteLine(sourceContext);
                _stream.Flush();
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _dict.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.Keys.GetEnumerator();
        }
    }
}
