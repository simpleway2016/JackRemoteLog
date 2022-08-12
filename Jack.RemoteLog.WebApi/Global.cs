using System.Security.Cryptography;
using System.Text;

namespace Jack.RemoteLog.WebApi
{
    public class Global
    {
        public static IConfiguration Configuration { get; set; }
        public static IServiceProvider ServiceProvider { get; set; }

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
