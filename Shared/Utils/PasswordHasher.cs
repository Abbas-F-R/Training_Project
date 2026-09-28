namespace OC_System_Training.Shared.Utils;

/// <summary>
/// Cryptographic utility for BCrypt password hashing and verification with automated salting.
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using the BCrypt algorithm with a salt work factor of 11.
    /// </summary>
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    /// <summary>
    /// Verifies that a plain-text password matches a stored BCrypt password hash.
    /// </summary>
    public static bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
