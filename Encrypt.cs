 
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Exceptions = Smead.RecordsManagement.Imaging.Permissions.ExceptionString;
 
internal class APIParameters
{
    internal string Ticket = string.Empty;
    internal string DatabaseName = string.Empty;
    internal int UserId = 0;
    internal string TableName = string.Empty;
    internal string TableId = string.Empty;
    internal int AttachmentNumber = 0;
    internal string UserName = string.Empty;
    internal string Password = string.Empty;
}

internal class Encrypt
{
    private
    const string ENCRYPTION_KEY = "Ldi3dShj";
    private static readonly byte[] SALT = new[] { (byte)76, (byte)19, (byte)126, (byte)206, (byte)240, (byte)229, (byte)89, (byte)9 };

    internal static string AesDecrypt(string inputText)
    {
        byte[] encryptedData = System.Convert.FromBase64String(inputText);

        using (AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider())
        {
            using (Rfc2898DeriveBytes secretKey = new Rfc2898DeriveBytes(ENCRYPTION_KEY, SALT))
            {
                using (ICryptoTransform decryptor = aesCrypto.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                {
                    using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            byte[] plainText = new byte[encryptedData.Length - 1 + 1];
                            int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);
                            return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
                        }
                    }
                }
            }
        }
    }

    internal static string IPDecrypt(string inputText, string IP)
    {
        try
        {
            byte[] encryptedData = System.Convert.FromBase64String(inputText);
            byte[] IPSalt = new byte[8];
            string plainText = "";

            using (AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider())
            {
                try
                {
                    IPSalt = (IP + "." + IP).Split(".".ToCharArray()).Select(Byte.Parse).ToArray();
                }
                catch (Exception)
                {
                    Int32 i = 0;
                    string[] IPArray = IP.Split(":".ToCharArray());

                    foreach (string IPElement in IPArray)
                    {
                        if (IPElement.Length > 2)
                            IPSalt[i] = System.Convert.ToByte(IPElement.Substring(0, 2), 16);
                        else if (IPElement.Length == 0)
                            IPSalt[i] = SALT[i];
                        else
                            IPSalt[i] = System.Convert.ToByte(IPElement, 16);
                        i += 1;
                    }
                }

                using (Rfc2898DeriveBytes secretKey = new Rfc2898DeriveBytes(ENCRYPTION_KEY, IPSalt))
                {
                    using (ICryptoTransform decryptor = aesCrypto.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                using (var streamReader = new StreamReader(cryptoStream, Encoding.Unicode))
                                {
                                    plainText = streamReader.ReadToEnd();

                                }
                                //var plainText = new byte[encryptedData.Length + 1];
                                //int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length - 1);
                                //return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
                            }
                        }
                    }
                }

                return plainText;
            }
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    internal static APIParameters DecryptDecodeToken(string token, string documentId, ref string DBUserName, ref string DBPassword)
    {
        string decryptedToken;

        try
        {
            decryptedToken = AesDecrypt(token);
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException(string.Format("{0} - {1}", Exceptions.NotAuthorized, ex.Message));
        }

        APIParameters @params = ParseDecryptedToken(decryptedToken, documentId);
        string[] dbCredentials = decryptedToken.Split('|');

        if (dbCredentials.Length > 5)
            DBUserName = dbCredentials[5];
        if (dbCredentials.Length > 6)
            DBPassword = dbCredentials[6];
        return @params;
    }

    internal static APIParameters DecryptDecodeToken(string token, string documentId)
    {
        string decryptedToken;

        try
        {
            decryptedToken = AesDecrypt(token);
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException(string.Format("{0} - {1}", Exceptions.NotAuthorized, ex.Message));
        }

        return ParseDecryptedToken(decryptedToken, documentId);
    }

    private static APIParameters ParseDecryptedToken(string decryptedToken, string documentId)
    {
        if (!decryptedToken.Contains("|"))
            throw new FileNotFoundException(Exceptions.NotAuthorized);
        string[] decrypted = decryptedToken.Split('|');
        if (decrypted.Length < 5)
            throw new FileNotFoundException(Exceptions.NotAuthorized);
        string[] dateString = decrypted[0].Split(':');
        if (dateString.Length < 6)
            throw new FileNotFoundException(Exceptions.NotAuthorized);
        DateTime expiryDate = Convert.ToDateTime(ReverseExpiry(DateTime.Now.ToUniversalTime(), true));
        if (expiryDate > ReverseExpiry(dateString))
            throw new Exception(Exceptions.AuthenticationExpired);

        APIParameters @params = new APIParameters();
        string[] values = null;
        if (!string.IsNullOrEmpty(documentId))
            values = documentId.Split('-');

        {
            var withBlock = @params;
            withBlock.DatabaseName = decrypted[1];
            withBlock.UserId = System.Convert.ToInt32(decrypted[2]);
            withBlock.UserName = decrypted[3];
            withBlock.Password = decrypted[4];

            if (values != null)
            {
                withBlock.TableName = values[0];
                if (values.Length > 1)
                    withBlock.TableId = values[1];
                if (values.Length > 2)
                    withBlock.AttachmentNumber = System.Convert.ToInt32(values[2]);
                withBlock.Ticket = Smead.Security.Encrypt.HashTicket(withBlock.UserId, withBlock.DatabaseName, withBlock.TableName, withBlock.TableId);
            }
        }

        return @params;
    }

    internal static DateTime ReverseExpiry(string[] expiryDate)
    {
        try
        {
            return Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}:{4}:{5}", expiryDate[4], expiryDate[3], expiryDate[5], expiryDate[2], expiryDate[1], expiryDate[0]));
        }
        catch (Exception)
        {
            throw new FileNotFoundException(Exceptions.NotAuthorized);
        }
    }

    internal static string ReverseExpiry(DateTime expiryDate, bool forNow)
    {
        if (forNow)
            return string.Format("{0}/{1}/{2} {3}:{4}:{5}", expiryDate.Month.ToString("00"), expiryDate.Day.ToString("00"), expiryDate.Year.ToString("0000"), expiryDate.Hour.ToString("00"), expiryDate.Minute.ToString("00"), expiryDate.Second.ToString("00"));
        return string.Format("{0}:{1}:{2}:{3}:{4}:{5}", expiryDate.Second.ToString("00"), expiryDate.Minute.ToString("00"), expiryDate.Hour.ToString("00"), expiryDate.Day.ToString("00"), expiryDate.Month.ToString("00"), expiryDate.Year.ToString("0000"));
    }
}