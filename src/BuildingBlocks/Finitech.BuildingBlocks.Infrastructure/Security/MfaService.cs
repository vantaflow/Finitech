using System.Security.Cryptography;
using Finitech.BuildingBlocks.Domain.Security;

namespace Finitech.BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Service for MFA/2FA using TOTP (Time-based One-Time Password)
/// Compatible with Google Authenticator, Microsoft Authenticator, etc.
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new TOTP secret and QR code URI for setup
    /// </summary>
    (string Secret, string QrCodeUri) GenerateSecret(string userEmail, string issuer = "Finitech");

    /// <summary>
    /// Validates a TOTP code against a secret
    /// </summary>
    bool ValidateTotp(string secret, string code);

    /// <summary>
    /// Generates recovery codes for account recovery
    /// </summary>
    string[] GenerateRecoveryCodes(int count = 10);

    /// <summary>
    /// Validates a recovery code (one-time use)
    /// </summary>
    bool ValidateRecoveryCode(string code, string[] validCodes);
}

public class MfaService : IMfaService
{
    public (string Secret, string QrCodeUri) GenerateSecret(string userEmail, string issuer = "Finitech")
    {
        // Generate 20-byte secret (160 bits)
        var secretBytes = new byte[20];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(secretBytes);
        }

        // Encode as Base32 for compatibility with authenticator apps
        var secret = Base32Encode(secretBytes);

        // Generate otpauth URI for QR code
        var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userEmail)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

        return (secret, qrCodeUri);
    }

    public bool ValidateTotp(string secret, string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 6)
            return false;

        var expectedCode = GenerateTotp(secret);
        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(code),
            System.Text.Encoding.UTF8.GetBytes(expectedCode));
    }

    public string[] GenerateRecoveryCodes(int count = 10)
    {
        var codes = new string[count];
        for (int i = 0; i < count; i++)
        {
            var bytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            // Format: XXXX-XXXX-XXXX for readability
            codes[i] = $"{BitConverter.ToUInt32(bytes, 0):X4}-{BitConverter.ToUInt32(bytes, 4):X4}";
        }
        return codes;
    }

    public bool ValidateRecoveryCode(string code, string[] validCodes)
    {
        return validCodes.Contains(code, StringComparer.OrdinalIgnoreCase);
    }

    private static string GenerateTotp(string secret)
    {
        var secretBytes = Base32Decode(secret);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var timestampBytes = BitConverter.GetBytes(timestamp);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }

        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timestampBytes);

        // Dynamic truncation
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24) |
                     ((hash[offset + 1] & 0xFF) << 16) |
                     ((hash[offset + 2] & 0xFF) << 8) |
                     (hash[offset + 3] & 0xFF);

        var otp = binary % 1000000;
        return otp.ToString("D6");
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new char[(data.Length * 8 + 4) / 5];
        int i = 0;
        int buffer = data[0];
        int next = 1;
        int bitsLeft = 8;

        while (bitsLeft > 0 || next < data.Length)
        {
            if (bitsLeft < 5)
            {
                if (next < data.Length)
                {
                    buffer <<= 8;
                    buffer |= data[next++];
                    bitsLeft += 8;
                }
                else
                {
                    int pad = 5 - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }
            int index = (buffer >> (bitsLeft - 5)) & 31;
            bitsLeft -= 5;
            output[i++] = alphabet[index];
        }

        return new string(output, 0, i);
    }

    private static byte[] Base32Decode(string encoded)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>();
        int buffer = 0;
        int bitsLeft = 0;

        foreach (var c in encoded.ToUpper())
        {
            int value = alphabet.IndexOf(c);
            if (value < 0) continue;

            buffer <<= 5;
            buffer |= value;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                output.Add((byte)((buffer >> bitsLeft) & 255));
            }
        }

        return output.ToArray();
    }
}
