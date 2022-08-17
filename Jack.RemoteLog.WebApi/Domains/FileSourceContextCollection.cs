using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace Jack.RemoteLog.WebApi.Domains
{
    public class FileSourceContextCollection : ISourceContextCollection,IDisposable
    {
        int _currentMaxId;
        ConcurrentDictionary<string,int> _dict = new ConcurrentDictionary<string,int>();
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
                            var index = line.IndexOf(":");
                            var id = int.Parse(line.Substring(0, index));
                            var content = line.Substring(index + 1);
                            _dict.TryAdd(content,id);
                            if (id > _currentMaxId)
                                id = _currentMaxId;
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
            if (_dict.TryAdd(sourceContext, 0))
            {
                var newid = Interlocked.Increment(ref _currentMaxId);
                _dict[sourceContext] = newid;

                _stream.WriteLine($"{newid}:{sourceContext}");
                _stream.Flush();
            }
        }

        public int GetId(string sourceContext)
        {
            if (sourceContext == null)
                return 0;
            if (_dict.TryGetValue(sourceContext, out int id))
                return id;
            return 0;
        }
        public string GetSourceContext(int id)
        {
            return _dict.Where(m => m.Value == id).Select(m => m.Key).FirstOrDefault();
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
