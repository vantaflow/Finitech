using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Finitech.BuildingBlocks.Domain.Security;
using Konscious.Security.Cryptography;

namespace Finitech.BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Production-ready Argon2id password hasher
/// Implements OWASP recommendations for password hashing
/// </summary>
public class Argon2PasswordHasher : IPasswordHasher
{
    // Argon2id parameters following OWASP recommendations
    // m=65536 (64MB), t=3, p=4
    private const int MemoryCost = 65536;  // 64 MB
    private const int TimeCost = 3;         // 3 iterations
    private const int Parallelism = 4;      // 4 parallel threads
    private const int HashLength = 32;      // 256 bits
    private const int SaltLength = 16;      // 128 bits

    // Version identifier for future parameter changes
    private const int CurrentVersion = 1;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate cryptographically secure random salt
        var salt = new byte[SaltLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash using Argon2id
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize = MemoryCost,
            Iterations = TimeCost
        };

        var hash = argon2.GetBytes(HashLength);

        // Format: $argon2id$v=version$m=mem,t=time,p=para$salt$hash
        var saltB64 = Convert.ToBase64String(salt).Replace('+', '-').Replace('/', '_');
        var hashB64 = Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_');

        return $"$argon2id$v={CurrentVersion}$m={MemoryCost},t={TimeCost},p={Parallelism}${saltB64}${hashB64}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            // Parse the hash
            var parsed = ParseHash(hash);
            if (parsed == null)
                return false;

            // Recompute hash with stored salt and parameters
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = parsed.Salt,
                DegreeOfParallelism = parsed.Parallelism,
                MemorySize = parsed.MemoryCost,
                Iterations = parsed.TimeCost
            };

            var computedHash = argon2.GetBytes(HashLength);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(computedHash, parsed.Hash);
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehash(string hash)
    {
        if (string.IsNullOrEmpty(hash))
            return true;

        try
        {
            var parsed = ParseHash(hash);
            if (parsed == null)
                return true;

            // Check if parameters match current settings
            return parsed.MemoryCost != MemoryCost
                || parsed.TimeCost != TimeCost
                || parsed.Parallelism != Parallelism
                || parsed.Version != CurrentVersion;
        }
        catch
        {
            return true;
        }
    }

    private static ParsedHash? ParseHash(string hash)
    {
        // Format: $argon2id$v=version$m=mem,t=time,p=para$salt$hash
        var parts = hash.Split('$');
        if (parts.Length != 5)
            return null;

        if (parts[1] != "argon2id")
            return null;

        // Parse version
        if (!parts[2].StartsWith("v=") || !int.TryParse(parts[2][2..], out var version))
            return null;

        // Parse parameters (m=65536,t=3,p=4)
        var paramParts = parts[3].Split(',');
        if (paramParts.Length != 3)
            return null;

        if (!paramParts[0].StartsWith("m=") || !int.TryParse(paramParts[0][2..], out var memCost))
            return null;

        if (!paramParts[1].StartsWith("t=") || !int.TryParse(paramParts[1][2..], out var timeCost))
            return null;

        if (!paramParts[2].StartsWith("p=") || !int.TryParse(paramParts[2][2..], out var parallelism))
            return null;

        // Decode salt and hash
        var saltB64 = parts[4].Split('$')[0].Replace('-', '+').Replace('_', '/');
        var hashB64 = parts[4].Split('$')[0].Replace('-', '+').Replace('_', '/');

        // The hash is after the last $ in parts[4], but we need to split differently
        var saltAndHash = parts[4].Split('$');
        if (saltAndHash.Length < 2)
            return null;

        saltB64 = saltAndHash[0].Replace('-', '+').Replace('_', '/');
        hashB64 = saltAndHash[1].Replace('-', '+').Replace('_', '/');

        // Add padding if necessary
        while (saltB64.Length % 4 != 0) saltB64 += '=';
        while (hashB64.Length % 4 != 0) hashB64 += '=';

        try
        {
            var salt = Convert.FromBase64String(saltB64);
            var hashBytes = Convert.FromBase64String(hashB64);

            return new ParsedHash
            {
                Version = version,
                MemoryCost = memCost,
                TimeCost = timeCost,
                Parallelism = parallelism,
                Salt = salt,
                Hash = hashBytes
            };
        }
        catch
        {
            return null;
        }
    }

    private class ParsedHash
    {
        public int Version { get; set; }
        public int MemoryCost { get; set; }
        public int TimeCost { get; set; }
        public int Parallelism { get; set; }
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        public byte[] Hash { get; set; } = Array.Empty<byte>();
    }
}

/// <summary>
/// Fallback PBKDF2 hasher for environments where Argon2 is not available
/// Also handles verification of existing PBKDF2 hashes during migration
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 600000; // OWASP 2023 recommendation
    private const int HashLength = 32;
    private const int SaltLength = 16;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        var salt = new byte[SaltLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashLength);

        var saltB64 = Convert.ToBase64String(salt).Replace('+', '-').Replace('/', '_');
        var hashB64 = Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_');

        return $"$pbkdf2-sha256$v=1$i={Iterations}${saltB64}${hashB64}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var parsed = ParseHash(hash);
            if (parsed == null)
                return false;

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                parsed.Salt,
                parsed.Iterations,
                HashAlgorithmName.SHA256,
                parsed.Hash.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, parsed.Hash);
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehash(string hash)
    {
        if (string.IsNullOrEmpty(hash))
            return true;

        // If it's PBKDF2, always suggest rehash to Argon2
        if (hash.StartsWith("$pbkdf2"))
            return true;

        return false;
    }

    private static ParsedHash? ParseHash(string hash)
    {
        // Format: $pbkdf2-sha256$v=1$i=600000$salt$hash
        var parts = hash.Split('$');
        if (parts.Length != 5)
            return null;

        if (parts[1] != "pbkdf2-sha256")
            return null;

        // Parse version
        var versionPart = parts[2].Split('=');
        if (versionPart.Length != 2 || !int.TryParse(versionPart[1], out _))
            return null;

        // Parse iterations
        var iterPart = parts[3].Split('=');
        if (iterPart.Length != 2 || !int.TryParse(iterPart[1], out var iterations))
            return null;

        // Parse salt and hash
        var saltHashParts = parts[4].Split('$');
        if (saltHashParts.Length < 2)
            return null;

        var saltB64 = saltHashParts[0].Replace('-', '+').Replace('_', '/');
        var hashB64 = saltHashParts[1].Replace('-', '+').Replace('_', '/');

        while (saltB64.Length % 4 != 0) saltB64 += '=';
        while (hashB64.Length % 4 != 0) hashB64 += '=';

        try
        {
            return new ParsedHash
            {
                Iterations = iterations,
                Salt = Convert.FromBase64String(saltB64),
                Hash = Convert.FromBase64String(hashB64)
            };
        }
        catch
        {
            return null;
        }
    }

    private class ParsedHash
    {
        public int Iterations { get; set; }
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        public byte[] Hash { get; set; } = Array.Empty<byte>();
    }
}

/// <summary>
/// Composite hasher that prefers Argon2id but can verify PBKDF2 for migration
/// </summary>
public class CompositePasswordHasher : IPasswordHasher
{
    private readonly Argon2PasswordHasher _argon2Hasher = new();
    private readonly Pbkdf2PasswordHasher _pbkdf2Hasher = new();

    public string HashPassword(string password)
    {
        // Always use Argon2id for new hashes
        return _argon2Hasher.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash))
            return false;

        // Route to appropriate hasher based on hash prefix
        if (hash.StartsWith("$argon2id"))
            return _argon2Hasher.VerifyPassword(password, hash);

        if (hash.StartsWith("$pbkdf2"))
            return _pbkdf2Hasher.VerifyPassword(password, hash);

        // Unknown format
        return false;
    }

    public bool NeedsRehash(string hash)
    {
        // If it's not Argon2id, it needs rehash
        return !hash.StartsWith("$argon2id") || _argon2Hasher.NeedsRehash(hash);
    }
}
