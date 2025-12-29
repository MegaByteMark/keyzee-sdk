using KeyZee.Application.Common.Encryption;
using KeyZee.Infrastructure.Options;

namespace KeyZee.Infrastructure.Encryption;

/// <summary>
/// AES Encryption Service Implementation
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly KeyZeeOptions _options;

    public AesEncryptionService(KeyZeeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Encrypts the given plain text using AES encryption. The encryption key and secret from options are used.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted text as a base64 string.</returns>
    public string Encrypt(string plainText)
    {
        return Encrypt(plainText, _options.EncryptionKey, _options.EncryptionSecret);
    }

    /// <summary>
    /// Encrypts the given plain text using AES encryption with the specified key and secret.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="key">The encryption key.</param>
    /// <param name="secret">The encryption secret (IV).</param>
    /// <returns>The encrypted text as a base64 string.</returns>
    public string Encrypt(string plainText, string key, string secret)
    {
        using var aes = System.Security.Cryptography.Aes.Create();

        aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
        aes.IV = System.Text.Encoding.UTF8.GetBytes(secret);

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();

        using (var csEncrypt = new System.Security.Cryptography.CryptoStream(msEncrypt, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    /// <summary>
    /// Decrypts the given cipher text using AES decryption. The encryption key and secret from options are used.
    /// </summary>
    /// <param name="cipherText">The cipher text to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    public string Decrypt(string cipherText)
    {
        return Decrypt(cipherText, _options.EncryptionKey,  _options.EncryptionSecret);
    }

    /// <summary>
    /// Decrypts the given cipher text using AES decryption with the specified key and secret.
    /// </summary>
    /// <param name="cipherText">The cipher text to decrypt.</param>
    /// <param name="key">The encryption key.</param>
    /// <param name="secret">The encryption secret (IV).</param>
    /// <returns>The decrypted plain text.</returns>
    public string Decrypt(string cipherText, string key, string secret)
    {
        using var aes = System.Security.Cryptography.Aes.Create();

        aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
        aes.IV = System.Text.Encoding.UTF8.GetBytes(secret);

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));

        using (var csDecrypt = new System.Security.Cryptography.CryptoStream(msDecrypt, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
        using (var srDecrypt = new StreamReader(csDecrypt))
        {
            return srDecrypt.ReadToEnd();
        }
    }
}