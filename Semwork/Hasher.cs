using System.Security.Cryptography;
using System.Text;

namespace Semwork;

public class Hasher
{
    public string GetPasswordHash(string password)
    {
        const string salt = "super_salt_by_evaklq_solumfy";
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        var bytes = SHA256.HashData(passwordBytes);
        return GetStringFromBytes(bytes);
    }

    private static string GetStringFromBytes(IEnumerable<byte> bytes)
    {
        return Encoding.UTF8.GetString(bytes.ToArray());
        return bytes.Aggregate(string.Empty, (current, b) => current + b.ToString());
    }
}
