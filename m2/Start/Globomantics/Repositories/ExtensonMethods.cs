using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Globomantics.Repositories
{
    public static class ExtensonMethods
    {
        public static string sh256(this string password)
        {
            using(var sh = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sh.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
