using System.Security.Cryptography;
using System.Text;

namespace ForexTradingWorkspace.Services.Security;

public sealed class DpapiEncryptionService : IEncryptionService
{
    public string Protect(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public string Unprotect(string cipherText)
    {
        var bytes = Convert.FromBase64String(cipherText);
        var plainBytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
