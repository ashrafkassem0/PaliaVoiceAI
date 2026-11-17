using System.Text.RegularExpressions;

namespace PaliaVoiceAI.Core;

public sealed class SpeechCache
{
    private static readonly Regex KeyPattern = new("^[0-9a-f]{64}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly string _directory;

    public SpeechCache(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Cache directory cannot be empty.", nameof(directory));
        _directory = directory;
    }

    public string DirectoryPath => _directory;

    public bool TryRead(string key, out byte[] audio)
    {
        audio = Array.Empty<byte>();
        if (!TryPath(key, out var path) || !File.Exists(path))
            return false;

        try
        {
            var bytes = File.ReadAllBytes(path);
            if (bytes.Length == 0)
                return false;
            audio = bytes;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool TryStore(string key, byte[] audio)
    {
        if (audio is null || audio.Length == 0 || !KeyPattern.IsMatch(key))
            return false;

        try
        {
            Directory.CreateDirectory(_directory);
            var destination = Path.Combine(_directory, key + ".wav");
            var temporary = destination + ".tmp";
            File.WriteAllBytes(temporary, audio);
            File.Move(temporary, destination, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public int DeleteOlderThan(DateTimeOffset cutoff)
    {
        if (!Directory.Exists(_directory))
            return 0;

        var removed = 0;
        foreach (var path in Directory.EnumerateFiles(_directory))
        {
            var name = Path.GetFileName(path);
            if (!name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) &&
                !name.EndsWith(".wav.tmp", StringComparison.OrdinalIgnoreCase))
                continue;
            DateTimeOffset written;
            try
            {
                written = File.GetLastWriteTimeUtc(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            if (written >= cutoff)
                continue;
            if (TryDelete(path))
                removed++;
        }

        return removed;
    }

    public int DeleteAll()
    {
        if (!Directory.Exists(_directory))
            return 0;

        var removed = 0;
        foreach (var path in Directory.EnumerateFiles(_directory))
        {
            var name = Path.GetFileName(path);
            if (!name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) &&
                !name.EndsWith(".wav.tmp", StringComparison.OrdinalIgnoreCase))
                continue;
            if (TryDelete(path))
                removed++;
        }

        return removed;
    }

    private bool TryPath(string key, out string path)
    {
        path = string.Empty;
        if (!KeyPattern.IsMatch(key))
            return false;
        path = Path.Combine(_directory, key + ".wav");
        return true;
    }

    private static bool TryDelete(string path)
    {
        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}

public static class WavDuration
{
    public static TimeSpan Read(ReadOnlySpan<byte> wav)
    {
        if (wav.Length < 12 || wav[0] != (byte)'R' || wav[1] != (byte)'I' || wav[2] != (byte)'F' || wav[3] != (byte)'F')
            return TimeSpan.Zero;
        if (wav[8] != (byte)'W' || wav[9] != (byte)'A' || wav[10] != (byte)'V' || wav[11] != (byte)'E')
            return TimeSpan.Zero;

        var sampleRate = 0;
        var channels = 0;
        var bits = 0;
        var dataLength = -1;
        var offset = 12;
        while (offset + 8 <= wav.Length)
        {
            var size = wav[offset + 4] | (wav[offset + 5] << 8) | (wav[offset + 6] << 16) | (wav[offset + 7] << 24);
            if (size < 0)
                return TimeSpan.Zero;
            var start = offset + 8;
            if (IsChunk(wav, offset, "fmt ") && size >= 16 && start + 16 <= wav.Length)
            {
                channels = wav[start + 2] | (wav[start + 3] << 8);
                sampleRate = wav[start + 4] | (wav[start + 5] << 8) | (wav[start + 6] << 16) | (wav[start + 7] << 24);
                bits = wav[start + 14] | (wav[start + 15] << 8);
            }
            else if (IsChunk(wav, offset, "data"))
            {
                dataLength = size;
                break;
            }

            var step = 8 + size + (size & 1);
            if (step <= 0 || offset + step < offset)
                return TimeSpan.Zero;
            offset += step;
        }

        if (sampleRate <= 0 || channels <= 0 || bits < 8 || dataLength < 0)
            return TimeSpan.Zero;
        var bytesPerSecond = sampleRate * (long)channels * (bits / 8);
        if (bytesPerSecond <= 0)
            return TimeSpan.Zero;
        return TimeSpan.FromSeconds(dataLength / (double)bytesPerSecond);
    }

    private static bool IsChunk(ReadOnlySpan<byte> wav, int offset, string id)
    {
        for (var i = 0; i < id.Length; i++)
        {
            if (wav[offset + i] != (byte)id[i])
                return false;
        }

        return true;
    }
}
