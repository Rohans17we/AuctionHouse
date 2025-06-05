using System;
using BCrypt.Net;

namespace TheAuctionHouse.Common;

/// <summary>
/// Utility class for password hashing and verification using BCrypt
/// </summary>
public static class PasswordHelper
{
    private const int WORK_FACTOR = 12;    /// <summary>
    /// Hashes a password using BCrypt with a secure work factor
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The hashed password</returns>
    /// <exception cref="ArgumentException">Thrown when password is null or empty</exception>
    public static string HashPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("[PasswordHelper] Attempt to hash null/empty password");
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        try
        {
            // Use work factor of 12 for good security/performance balance
            var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: WORK_FACTOR);
            Console.WriteLine($"[PasswordHelper] Password hashed successfully, prefix: {hashedPassword.Substring(0, Math.Min(10, hashedPassword.Length))}...");
            return hashedPassword;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PasswordHelper] Error hashing password: {ex.Message}");
            Console.WriteLine($"[PasswordHelper] Stack trace: {ex.StackTrace}");
            throw;
        }
    }/// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="plainPassword">The plain text password to verify</param>
    /// <param name="hashedPassword">The hashed password to verify against</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    public static bool VerifyPassword(string? plainPassword, string? hashedPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
        {
            Console.WriteLine("[PasswordHelper] Plain password is null/empty");
            return false;
        }

        if (string.IsNullOrEmpty(hashedPassword))
        {
            Console.WriteLine("[PasswordHelper] Hashed password is null/empty");
            return false;
        }

        try
        {
            Console.WriteLine($"[PasswordHelper] Verifying password hash format: {hashedPassword.Substring(0, Math.Min(10, hashedPassword.Length))}...");
            // BCrypt.Net-Next can verify both $2a$ and $2b$ prefixes
            return BCrypt.Net.BCrypt.EnhancedVerify(plainPassword, hashedPassword);
        }
        catch (Exception ex)
        {
            // Log the specific error but don't expose it to the caller
            Console.WriteLine($"[PasswordHelper] Error verifying password: {ex.Message}");
            Console.WriteLine($"[PasswordHelper] Stack trace: {ex.StackTrace}");
            return false;
        }
    }    /// <summary>
    /// Validates password strength requirements
    /// </summary>
    /// <param name="password">The password to validate</param>
    /// <returns>True if password meets requirements, false otherwise</returns>
    public static bool IsValidPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("[PasswordHelper] Password is null or whitespace");
            return false;
        }

        // Basic password requirements: at least 6 characters
        if (password.Length < 6)
        {
            Console.WriteLine("[PasswordHelper] Password is too short");
            return false;
        }

        return true;
    }
}
