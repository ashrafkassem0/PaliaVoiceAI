using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PaliaVoiceAI.Core;

public sealed record SpeechIdentity(
    string Text,
    string VoiceId,
    float Pace,
    string ModelHash,
    string EngineVersion,
    string ContentType)
{
    public const string WavContentType = "audio/wav";

    public static SpeechIdentity Create(string text, string voiceId, float pace, string modelHash, string engineVersion)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));
        if (string.IsNullOrWhiteSpace(voiceId))
            throw new ArgumentException("Voice cannot be empty.", nameof(voiceId));
        if (string.IsNullOrWhiteSpace(modelHash))
            throw new ArgumentException("Model hash cannot be empty.", nameof(modelHash));
        if (string.IsNullOrWhiteSpace(engineVersion))
            throw new ArgumentException("Engine version cannot be empty.", nameof(engineVersion));

        var normalizedPace = (float)Math.Round(Math.Clamp(pace, 0.7, 1.4), 2, MidpointRounding.AwayFromZero);
        return new SpeechIdentity(
            text.Trim(),
            voiceId.Trim(),
            normalizedPace,
            modelHash.Trim().ToLowerInvariant(),
            engineVersion.Trim(),
            WavContentType);
    }
}

public static class SpeechCacheKey
{
    public static string Create(SpeechIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        var pace = identity.Pace.ToString("0.00", CultureInfo.InvariantCulture);
        var payload = string.Join('\n', identity.Text, identity.VoiceId, pace, identity.ModelHash, identity.EngineVersion, identity.ContentType);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
}

public static class ModelFileHash
{
    private static readonly Dictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static string Sha256(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
            throw new FileNotFoundException("Model file was not found.", path);

        var stamp = string.Create(CultureInfo.InvariantCulture, $"{info.FullName}|{info.Length}|{info.LastWriteTimeUtc.Ticks}");
        lock (Cache)
        {
            if (Cache.TryGetValue(stamp, out var cached))
                return cached;
        }

        using var stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        lock (Cache)
            Cache[stamp] = hash;
        return hash;
    }
}
