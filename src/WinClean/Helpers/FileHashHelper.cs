using System.Security.Cryptography;

namespace WinClean.Helpers;

public static class FileHashHelper
{
    private const int PartialHashSize = 4096;

    public static async Task<string> ComputePartialHashAsync(string filePath, CancellationToken ct = default)
    {
        var buffer = new byte[PartialHashSize];
        int bytesRead;

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, PartialHashSize, useAsync: true);
        bytesRead = await stream.ReadAsync(buffer.AsMemory(0, PartialHashSize), ct);

        return Convert.ToHexString(SHA256.HashData(buffer.AsSpan(0, bytesRead)));
    }

    public static async Task<string> ComputeFullHashAsync(string filePath, CancellationToken ct = default)
    {
        const int bufferSize = 81920;
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexString(hash);
    }
}
