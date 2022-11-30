using System.Security.Cryptography;
using System.Text;

namespace TabFusionRMS.WebCS
{
    public class TabQUIKEncrypt
    {
        private static byte[] _key = new[] { (byte)123, (byte)217, (byte)19, (byte)11, (byte)24, (byte)26, (byte)85, (byte)45, (byte)114, (byte)184, (byte)27, (byte)162, (byte)37, (byte)112, (byte)222, (byte)211, (byte)241, (byte)24, (byte)175, (byte)144, (byte)173, (byte)53, (byte)196, (byte)29, (byte)24, (byte)26, (byte)17, (byte)218, (byte)131, (byte)236, (byte)53, (byte)209 };
        private static byte[] _vector = new[] { (byte)146, (byte)64, (byte)191, (byte)111, (byte)23, (byte)3, (byte)113, (byte)119, (byte)231, (byte)101, (byte)221, (byte)112, (byte)79, (byte)32, (byte)114, (byte)156 };
        private ICryptoTransform _encryptor;
        private ICryptoTransform _decryptor;
        private UTF8Encoding _encoder;

        public TabQUIKEncrypt()
        {
            using (var rm = new AesCryptoServiceProvider())
            {
                _encryptor = rm.CreateEncryptor(_key, _vector);
                _decryptor = rm.CreateDecryptor(_key, _vector);
            }

            _encoder = new UTF8Encoding();
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(_encoder.GetBytes(unencrypted)));
        }

        public string Decrypt(string encrypted)
        {
            return _encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, _encryptor);
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, _decryptor);
        }

        protected byte[] Transform(byte[] buffer, ICryptoTransform transform__1)
        {
            using (var stream = new MemoryStream())
            {
                using (var cs = new CryptoStream(stream, transform__1, CryptoStreamMode.Write))
                {
                    cs.Write(buffer, 0, buffer.Length);
                }

                return stream.ToArray();
            }
        }
    }
}
