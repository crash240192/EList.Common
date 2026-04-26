using System.Security.Cryptography;
using System.Text;

namespace EList.Common.Encryption
{
    public static class EncryptionUtility
    {
        private static byte[] key = new byte[8] { 5, 3, 8, 2, 1, 3, 8, 1 };
        private static byte[] iv = new byte[8] { 5, 4, 8, 0, 6, 1, 5, 3 };

        public static string EncryptDESToBase64(this string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
            byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        public static string DecryptDESFromBase64(this string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
            byte[] inputbuffer = Convert.FromBase64String(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Encoding.Unicode.GetString(outputBuffer);
        }

        public static string EncryptMD5(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var bytes = Encoding.UTF8.GetBytes(str);

            if (bytes == null || bytes.Length == 0)
                return null;

            using (var md5 = MD5.Create())
            {
                return string.Join("", md5.ComputeHash(bytes).Select(x => x.ToString("x2")));
            }
        }
    }
}
