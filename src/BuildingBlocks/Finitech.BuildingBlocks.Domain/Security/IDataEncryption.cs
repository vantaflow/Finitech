namespace Finitech.BuildingBlocks.Domain.Security;

/// <summary>
/// Service for AES-256 encryption of sensitive data (PII)
/// </summary>
public interface IDataEncryption
{
    /// <summary>
    /// Encrypts sensitive data using AES-256-GCM
    /// </summary>
    /// <param name="plaintext">Data to encrypt</param>
    /// <returns>Base64 encoded ciphertext with nonce and tag</returns>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts data encrypted with Encrypt
    /// </summary>
    /// <param name="ciphertext">Base64 encoded ciphertext</param>
    /// <returns>Decrypted plaintext</returns>
    string Decrypt(string ciphertext);

    /// <summary>
    /// Encrypts data with a specific key (for key rotation)
    /// </summary>
    /// <param name="plaintext">Data to encrypt</param>
    /// <param name="keyId">Key identifier</param>
    /// <returns>Encrypted data with key ID</returns>
    string EncryptWithKey(string plaintext, string keyId);

    /// <summary>
    /// Rotates encryption to a new key
    /// </summary>
    /// <param name="ciphertext">Existing encrypted data</param>
    /// <returns>Re-encrypted data with current key</returns>
    string RotateKey(string ciphertext);

    /// <summary>
    /// Generates a deterministic hash for searching encrypted data
    /// Uses HMAC-SHA256 with a blind index key
    /// </summary>
    /// <param name="value">Value to hash</param>
    /// <returns>Deterministic hash for indexing</returns>
    string GenerateBlindIndex(string value);

    /// <summary>
    /// Masks sensitive data for logging/display
    /// </summary>
    /// <param name="value">Value to mask</param>
    /// <param name="mask">Mask character</param>
    /// <returns>Masked value</returns>
    string Mask(string value, char mask = '*');
}

/// <summary>
/// Encrypted value with metadata
/// </summary>
public record EncryptedValue
{
    public required string Ciphertext { get; init; }
    public required string KeyId { get; init; }
    public required string Algorithm { get; init; } = "AES-256-GCM";
    public DateTime EncryptedAt { get; init; } = DateTime.UtcNow;
}
