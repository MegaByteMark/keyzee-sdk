using Microsoft.Extensions.Primitives;

namespace KeyZee.Application.Common.Encryption;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Encrypt(string plainText, string key, string secret);
    string Decrypt(string cipherText);
    string Decrypt(string cipherText, string key, string secret);
}