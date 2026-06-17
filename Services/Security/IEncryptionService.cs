namespace ForexTradingWorkspace.Services.Security;

public interface IEncryptionService
{
    string Protect(string plainText);
    string Unprotect(string cipherText);
}
