using System.Security.Cryptography;

namespace MyApp.XProtocol;

public static class RijndaelHandler
{
    public static byte[] Encrypt(byte[] data, string passPhrase)
    {
        byte[] saltStringBytes = Generate256BitsOfRandomEntropy();
        byte[] ivStringBytes = Generate256BitsOfRandomEntropy();
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, 1000))
        {
            var keyBytes = password.GetBytes(32);
            using (var symmetricKey = Aes.Create())
            {
                symmetricKey.BlockSize = 128;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(data, 0, data.Length);
                            cryptoStream.FlushFinalBlock();
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return cipherTextBytes;
                        }
                    }
                }
            }
        }
    }

    public static byte[] Decrypt(byte[] data, string passPhrase)
    {
        var saltStringBytes = data.Take(32).ToArray();
        var ivStringBytes = data.Skip(32).Take(32).ToArray();
        var cipherTextBytes = data.Skip(64).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, 1000))
        {
            var keyBytes = password.GetBytes(32);
            using (var symmetricKey = Aes.Create())
            {
                symmetricKey.BlockSize = 128;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return plainTextBytes.Take(decryptedByteCount).ToArray();
                        }
                    }
                }
            }
        }
    }

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        var randomBytes = new byte[32];
        using (var rngCsp = RandomNumberGenerator.Create())
        {
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }
}
