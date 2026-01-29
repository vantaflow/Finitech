using System.Security.Cryptography;

namespace Finitech.BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Service for PCI-compliant card tokenization
/// Replaces PAN with non-sensitive tokens
/// </summary>
public interface ICardTokenizationService
{
    /// <summary>
    /// Tokenizes a PAN (Primary Account Number)
    /// </summary>
    /// <param name="pan">Card number (14-19 digits)</param>
    /// <returns>Token and masked PAN</returns>
    (string Token, string MaskedPan) Tokenize(string pan);

    /// <summary>
    /// Detokenizes to retrieve original PAN (restricted access)
    /// </summary>
    string? Detokenize(string token);

    /// <summary>
    /// Validates card number using Luhn algorithm
    /// </summary>
    bool ValidatePan(string pan);

    /// <summary>
    /// Masks PAN for display
    /// </summary>
    string MaskPan(string pan);
}

public class CardTokenizationService : ICardTokenizationService
{
    // In production: use HSM (Hardware Security Module) or cloud KMS
    private readonly byte[] _encryptionKey;
    private readonly Dictionary<string, string> _tokenStore; // In production: use secure vault

    public CardTokenizationService()
    {
        _encryptionKey = GenerateKey();
        _tokenStore = new Dictionary<string, string>();
    }

    public (string Token, string MaskedPan) Tokenize(string pan)
    {
        if (string.IsNullOrEmpty(pan) || pan.Length < 14 || pan.Length > 19)
            throw new ArgumentException("Invalid PAN length");

        if (!ValidatePan(pan))
            throw new ArgumentException("Invalid PAN (Luhn check failed)");

        // Generate deterministic token based on PAN
        var token = GenerateToken(pan);
        var maskedPan = MaskPan(pan);

        // Store encrypted PAN (in production: HSM or secure vault)
        var encryptedPan = EncryptPan(pan);
        _tokenStore[token] = encryptedPan;

        return (token, maskedPan);
    }

    public string? Detokenize(string token)
    {
        if (_tokenStore.TryGetValue(token, out var encryptedPan))
        {
            return DecryptPan(encryptedPan);
        }
        return null;
    }

    public bool ValidatePan(string pan)
    {
        if (string.IsNullOrEmpty(pan) || !pan.All(char.IsDigit))
            return false;

        // Luhn algorithm
        int sum = 0;
        bool alternate = false;
        for (int i = pan.Length - 1; i >= 0; i--)
        {
            int digit = pan[i] - '0';
            if (alternate)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }

    public string MaskPan(string pan)
    {
        if (pan.Length < 4) return pan;
        return $"**** **** **** {pan[^4..]}";
    }

    private string GenerateToken(string pan)
    {
        // Generate format-preserving token (FPAN)
        // First 6 digits (BIN) + last 4 digits preserved, middle randomized
        var bin = pan[..6];
        var last4 = pan[^4..];
        var middleLength = pan.Length - 10;

        // Hash-based middle section
        using var hmac = new HMACSHA256(_encryptionKey);
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pan));
        var middle = Convert.ToHexString(hash)[..middleLength];

        // Convert to digits
        var tokenMiddle = new string(middle.Select(c => char.IsDigit(c) ? c : (char)('0' + (c % 10))).ToArray());

        return $"{bin}{tokenMiddle}{last4}";
    }

    private string EncryptPan(string pan)
    {
        // Simplified encryption - in production use AES-256-GCM with HSM
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor();
        var panBytes = System.Text.Encoding.UTF8.GetBytes(pan);
        var encrypted = encryptor.TransformFinalBlock(panBytes, 0, panBytes.Length);

        return Convert.ToBase64String(aes.IV.Concat(encrypted).ToArray());
    }

    private string DecryptPan(string encrypted)
    {
        var data = Convert.FromBase64String(encrypted);
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = data[..16];

        var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(data, 16, data.Length - 16);

        return System.Text.Encoding.UTF8.GetString(decrypted);
    }

    private static byte[] GenerateKey()
    {
        // In production: load from secure key management
        return SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("ProductionKeyFromVault"));
    }
}
