using Microsoft.Data.Sqlite;
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
        SqliteConnection _sqlCon;
        public FileSourceContextCollection(string folderPath)
        {
            _filepath = $"{folderPath}/sourcecontext.db";
            _sqlCon = new SqliteConnection($"data source='{_filepath}'");
            

     
            if (File.Exists(_filepath) == false)
            {
                _sqlCon.Open();
                init();
            }
            else
            {
                _sqlCon.Open();

                using (SqliteCommand cmd = new SqliteCommand(null,_sqlCon))
                {
                    cmd.CommandText = "select id,name from sourcecontext";
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _dict[reader["name"].ToString()] = Convert.ToInt32( reader["id"]);
                            }
                        }
                    }
                    catch
                    {
                        init();
                    }
                }
            }
        }

        void init()
        {
            try
            {
                SqliteCommand cmd = new SqliteCommand();
                cmd.CommandText = @"
create table sourcecontext (
    [id]            integer PRIMARY KEY autoincrement,
    [name]         varchar (255)
)
";
                cmd.Connection = _sqlCon;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch 
            {

            }
        }



        public void Add(string sourceContext)
        {
            if (_dict.ContainsKey(sourceContext) == false)
            {
                using (SqliteCommand cmd = new SqliteCommand())
                {
                    var sqlparam = new SqliteParameter();
                    sqlparam.Value = sourceContext;
                    sqlparam.ParameterName = "@p";
                    cmd.Parameters.Add(sqlparam);

                    cmd.Connection = _sqlCon;
                    cmd.CommandText = "insert into sourcecontext (name) values (@p)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "select last_insert_rowid()";
                    _dict[sourceContext] = Convert.ToInt32( cmd.ExecuteScalar());
                }
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
            _sqlCon?.Dispose();
            _sqlCon = null;
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
