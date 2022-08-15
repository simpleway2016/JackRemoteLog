using System.Security.Cryptography;
using System.Text;

namespace Jack.RemoteLog.WebApi
{
    public class Global
    {
        public static IConfiguration Configuration { get; set; }
        public static IServiceProvider ServiceProvider { get; set; }

        static int _pageSize;
        public static int PageSize
        {
            get
            {
                if(_pageSize == 0)
                {
                    if ( int.TryParse( Configuration["PageSize"],out _pageSize) )
                    {
                        return _pageSize;
                    }
                    else
                    {
                        _pageSize = 50;
                    }
                }
                return _pageSize;
            }
        }

        public static byte[] GetHash(string value)
        {
            if (value == null)
                return null;
            return new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        public static bool IsEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
    }
}
