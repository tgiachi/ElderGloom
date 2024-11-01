using System.Buffers;
using System.IO.Compression;
using System.Text;
using ElderGloom.Network.Types;

namespace ElderGloom.Network.Utils;

public static class CompressionHelper
{
    private const int DefaultBufferSize = 64 * 1024; // 64KB buffer


    public static byte[] CompressString(
        string text,
        CompressionType type = CompressionType.GZip,
        CompressionLevel level = CompressionLevel.Optimal
    )
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var raw = Encoding.UTF8.GetBytes(text);

        using var memory = new MemoryStream();
        using var compression = CreateCompressionStream(memory, type, level);

        compression.Write(raw, 0, raw.Length);
        compression.Flush();

        return memory.ToArray();
    }

    public static async Task<byte[]> CompressStringAsync(
        string text,
        CompressionType type = CompressionType.GZip,
        CompressionLevel level = CompressionLevel.Optimal,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var raw = Encoding.UTF8.GetBytes(text);

        using var memory = new MemoryStream();
        await using var compression = CreateCompressionStream(memory, type, level);

        await compression.WriteAsync(raw, cancellationToken);
        await compression.FlushAsync(cancellationToken);

        return memory.ToArray();
    }

    public static string DecompressString(
        byte[]? compressed,
        CompressionType type = CompressionType.GZip
    )
    {
        if (compressed == null || compressed.Length == 0)
        {
            return string.Empty;
        }

        using var memory = new MemoryStream(compressed);
        using var decompression = CreateDecompressionStream(memory, type);
        using var resultStream = new MemoryStream();

        var buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        try
        {
            int read;
            while ((read = decompression.Read(buffer, 0, buffer.Length)) > 0)
            {
                resultStream.Write(buffer, 0, read);
            }

            return Encoding.UTF8.GetString(resultStream.ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static async Task<string> DecompressStringAsync(
        byte[]? compressed,
        CompressionType type = CompressionType.GZip,
        CancellationToken cancellationToken = default
    )
    {
        if (compressed == null || compressed.Length == 0)
        {
            return string.Empty;
        }

        using var memory = new MemoryStream(compressed);
        await using var decompression = CreateDecompressionStream(memory, type);
        using var resultStream = new MemoryStream();

        var buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        try
        {
            int read;
            while ((read = await decompression.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await resultStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }

            return Encoding.UTF8.GetString(resultStream.ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static byte[] Compress(
        byte[] data,
        CompressionType type = CompressionType.GZip,
        CompressionLevel level = CompressionLevel.Optimal
    )
    {
        if (data == null || data.Length == 0)
            return Array.Empty<byte>();

        using var memory = new MemoryStream();
        using var compression = CreateCompressionStream(memory, type, level);

        compression.Write(data, 0, data.Length);
        compression.Flush();

        return memory.ToArray();
    }

    public static async Task<byte[]> CompressAsync(
        byte[]? data,
        CompressionType type = CompressionType.GZip,
        CompressionLevel level = CompressionLevel.Optimal,
        CancellationToken cancellationToken = default
    )
    {
        if (data == null || data.Length == 0)
        {
            return [];
        }

        using var memory = new MemoryStream();
        using var compression = CreateCompressionStream(memory, type, level);

        await compression.WriteAsync(data, cancellationToken);
        await compression.FlushAsync(cancellationToken);

        return memory.ToArray();
    }

    public static byte[] Decompress(
        byte[]? compressed,
        CompressionType type = CompressionType.GZip
    )
    {
        if (compressed == null || compressed.Length == 0)
        {
            return [];
        }

        using var memory = new MemoryStream(compressed);
        using var decompression = CreateDecompressionStream(memory, type);
        using var resultStream = new MemoryStream();

        var buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        try
        {
            int read;
            while ((read = decompression.Read(buffer, 0, buffer.Length)) > 0)
            {
                resultStream.Write(buffer, 0, read);
            }

            return resultStream.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static async Task<byte[]> DecompressAsync(
        byte[]? compressed,
        CompressionType type = CompressionType.GZip,
        CancellationToken cancellationToken = default
    )
    {
        if (compressed == null || compressed.Length == 0)
        {
            return Array.Empty<byte>();
        }

        using var memory = new MemoryStream(compressed);
        await using var decompression = CreateDecompressionStream(memory, type);
        using var resultStream = new MemoryStream();

        var buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        try
        {
            int read;
            while ((read = await decompression.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await resultStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }

            return resultStream.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static long EstimateCompressedSize(string text)
    {
        var originalSize = Encoding.UTF8.GetByteCount(text);
        return (long)(originalSize * 0.6);
    }

    private static Stream CreateCompressionStream(
        Stream outputStream,
        CompressionType type,
        CompressionLevel level
    )
    {
        return type switch
        {
            CompressionType.GZip    => new GZipStream(outputStream, level, true),
            CompressionType.Deflate => new DeflateStream(outputStream, level, true),
            CompressionType.Brotli  => new BrotliStream(outputStream, level, true),
            _                       => throw new ArgumentException("Unsupported compression type", nameof(type))
        };
    }

    private static Stream CreateDecompressionStream(
        Stream inputStream,
        CompressionType type
    )
    {
        return type switch
        {
            CompressionType.GZip    => new GZipStream(inputStream, CompressionMode.Decompress, true),
            CompressionType.Deflate => new DeflateStream(inputStream, CompressionMode.Decompress, true),
            CompressionType.Brotli  => new BrotliStream(inputStream, CompressionMode.Decompress, true),
            _                       => throw new ArgumentException("Unsupported compression type", nameof(type))
        };
    }
}
