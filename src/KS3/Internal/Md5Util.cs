using System.Security.Cryptography;
using System.Text;

namespace KS3.Internal
{
    public class Md5Util
    {
        public static byte[] Md5Digest(string message)
        {
            byte[] result = Encoding.UTF8.GetBytes(message);
            MD5 md5 = MD5.Create();
            byte[] output = md5.ComputeHash(result);
            return output;
        }
    }
}
