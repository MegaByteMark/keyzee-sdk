namespace KeyZee.Application.Common.Encryption;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}