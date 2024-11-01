namespace ElderGloom.Network.Utils;

using System.Security.Cryptography;

public static class CryptoHelper
{
    // Standard sizes for AES-256
    private const int KeySize = 256;
    private const int BlockSize = 128;
    private const int IvSize = 16; // 128 bit

    /// <summary>
    /// Generates a random key for AES-256
    /// </summary>
    public static byte[] GenerateKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.GenerateKey();
        return aes.Key;
    }

    /// <summary>
    /// Generates a random IV
    /// </summary>
    public static byte[] GenerateIV()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        return aes.IV;
    }

    /// <summary>
    /// Encrypts a buffer using AES-256
    /// </summary>
    /// <param name="data">Buffer to encrypt</param>
    /// <param name="key">AES-256 key (32 bytes)</param>
    /// <param name="iv">IV (16 bytes)</param>
    /// <returns>Encrypted buffer with prepended IV</returns>
    public static byte[] Encrypt(byte[]? data, byte[] key, byte[]? iv = null)
    {
        if (data == null || data.Length == 0)
        {
            return Array.Empty<byte>();
        }

        if (key is not { Length: KeySize / 8 })
        {
            throw new ArgumentException($"Key must be {KeySize / 8} bytes", nameof(key));
        }

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // If no IV is provided, generate a new one
        if (iv == null)
        {
            aes.GenerateIV();
            iv = aes.IV;
        }
        else
        {
            if (iv.Length != IvSize)
            {
                throw new ArgumentException($"IV must be {IvSize} bytes", nameof(iv));
            }

            aes.IV = iv;
        }

        using var ms = new MemoryStream();
        // Prepend the IV to the encrypted buffer
        ms.Write(iv, 0, iv.Length);

        using (var encryptor = aes.CreateEncryptor())
        {
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
            }
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Decrypts a buffer using AES-256
    /// </summary>
    /// <param name="encryptedData">Encrypted buffer with prepended IV</param>
    /// <param name="key">AES-256 key (32 bytes)</param>
    /// <returns>Decrypted buffer</returns>
    public static byte[] Decrypt(byte[]? encryptedData, byte[] key)
    {
        if (encryptedData == null || encryptedData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        if (key == null || key.Length != KeySize / 8)
        {
            throw new ArgumentException($"Key must be {KeySize / 8} bytes", nameof(key));
        }

        if (encryptedData.Length < IvSize)
        {
            throw new ArgumentException("Encrypted data too short", nameof(encryptedData));
        }

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extract the IV from the encrypted buffer
        byte[] iv = new byte[IvSize];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var ms = new MemoryStream();
        using (var decryptor = aes.CreateDecryptor())
        {
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                cs.Write(encryptedData, IvSize, encryptedData.Length - IvSize);
                cs.FlushFinalBlock();
            }
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Utility method to generate a key from a password
    /// </summary>
    public static byte[] GenerateKeyFromPassword(string password, byte[] salt = null)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be empty", nameof(password));
        }

        // If no salt is provided, generate a new one
        salt ??= GenerateRandomSalt();

        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            100000, // Number of iterations
            HashAlgorithmName.SHA256
        );

        return deriveBytes.GetBytes(KeySize / 8);
    }

    /// <summary>
    /// Generates a random salt for key derivation
    /// </summary>
    public static byte[] GenerateRandomSalt(int size = 32)
    {
        var salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    /// <summary>
    /// Async versions of the main methods
    /// </summary>
    public static async Task<byte[]> EncryptAsync(
        byte[] data, byte[] key, byte[] iv = null,
        CancellationToken cancellationToken = default
    )
    {
        if (data == null || data.Length == 0)
            return Array.Empty<byte>();

        if (key == null || key.Length != KeySize / 8)
            throw new ArgumentException($"Key must be {KeySize / 8} bytes", nameof(key));

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        if (iv == null)
        {
            aes.GenerateIV();
            iv = aes.IV;
        }
        else
        {
            if (iv.Length != IvSize)
                throw new ArgumentException($"IV must be {IvSize} bytes", nameof(iv));
            aes.IV = iv;
        }

        using var ms = new MemoryStream();
        await ms.WriteAsync(iv, 0, iv.Length, cancellationToken);

        using (var encryptor = aes.CreateEncryptor())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(data, 0, data.Length, cancellationToken);
                await cs.FlushFinalBlockAsync(cancellationToken);
            }

        return ms.ToArray();
    }

    public static async Task<byte[]> DecryptAsync(
        byte[] encryptedData, byte[] key,
        CancellationToken cancellationToken = default
    )
    {
        // Implementation similar to sync version but with async operations
        if (encryptedData == null || encryptedData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        if (key == null || key.Length != KeySize / 8)
        {
            throw new ArgumentException($"Key must be {KeySize / 8} bytes", nameof(key));
        }

        if (encryptedData.Length < IvSize)
        {
            throw new ArgumentException("Encrypted data too short", nameof(encryptedData));
        }

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        byte[] iv = new byte[IvSize];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var ms = new MemoryStream();
        using (var decryptor = aes.CreateDecryptor())
        {
            await using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(encryptedData, IvSize, encryptedData.Length - IvSize, cancellationToken);
                await cs.FlushFinalBlockAsync(cancellationToken);
            }
        }

        return ms.ToArray();
    }
}
