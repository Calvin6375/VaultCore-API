namespace VaultCore.Application.Common;

/// <summary>
/// Password hashing (BCrypt) abstraction.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
