using System.Globalization;
using System.Text;

namespace PaliaVoiceAI.Core;

public readonly record struct SpeechTiming(
    string Backend,
    string VoiceId,
    int Threads,
    double GenerationMs,
    double AudioMs,
    double RealTimeFactor,
    bool FromCache,
    int CharacterCount);

/// <summary>
/// Errors and timings only. Dialogue text is not a field on this log.
/// </summary>
public sealed class SessionLog
{
    public const int DefaultMaxBytes = 512 * 1024;
    public const string Header = "# Errors and timings only. Dialogue text is not written here.";

    private readonly int _maxBytes;
    private readonly int _maxArchives;
    private readonly Func<DateTimeOffset> _clock;
    private readonly object _gate = new();
    private static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public SessionLog(string directory, int maxBytes = DefaultMaxBytes, int maxArchives = 2, Func<DateTimeOffset>? clock = null)
    {
        DirectoryPath = directory ?? throw new ArgumentNullException(nameof(directory));
        _maxBytes = Math.Max(maxBytes, 1);
        _maxArchives = Math.Max(maxArchives, 1);
        _clock = clock ?? (() => DateTimeOffset.Now);
    }

    public string DirectoryPath { get; }

    public string CurrentPath => Path.Combine(DirectoryPath, "session.log");

    public static void WriteError(string directory, string source, string exceptionType)
    {
        try
        {
            new SessionLog(directory).Error(source, exceptionType);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
        }
    }

    public void Error(string source, string exceptionType)
    {
        var line = string.Create(CultureInfo.InvariantCulture, $"{_clock():O} error {SourceToken(source)} {TypeToken(exceptionType)}");
        lock (_gate)
            Append(line);
    }

    public void Timing(SpeechTiming timing)
    {
        var threads = Math.Clamp(timing.Threads, 0, 64);
        var chars = Math.Clamp(timing.CharacterCount, 0, 100_000);
        var line = string.Create(CultureInfo.InvariantCulture,
            $"{_clock():O} timing backend={BackendToken(timing.Backend)} voice={VoiceToken(timing.VoiceId)} threads={threads} generationMs={Number(timing.GenerationMs, "F2")} audioMs={Number(timing.AudioMs, "F2")} rtf={Number(timing.RealTimeFactor, "F4")} cache={(timing.FromCache ? "true" : "false")} chars={chars}");
        lock (_gate)
            Append(line);
    }

    public static string SourceToken(string? value) => Token(value, "app");

    public static string TypeToken(string? value) => Token(value, "Exception");

    public static string VoiceToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 32)
            return "unknown";
        foreach (var ch in value)
        {
            if (!char.IsAsciiLetterOrDigit(ch) && ch != '_')
                return "unknown";
        }

        return value;
    }

    public static string BackendToken(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "mock" => nameof(TtsBackend.Mock),
        "cpu" => nameof(TtsBackend.Cpu),
        "cuda" => nameof(TtsBackend.Cuda),
        _ => "Unknown"
    };

    private static string Token(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 64)
            return fallback;
        foreach (var ch in value)
        {
            if (!char.IsAsciiLetterOrDigit(ch))
                return fallback;
        }

        return value;
    }

    private static string Number(double value, string format)
    {
        if (!double.IsFinite(value) || value < 0)
            value = 0;
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private void Append(string line)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            var payload = line + Environment.NewLine;
            if (File.Exists(CurrentPath) && new FileInfo(CurrentPath).Length + Utf8.GetByteCount(payload) > _maxBytes)
                Rotate();
            if (!File.Exists(CurrentPath))
                payload = Header + Environment.NewLine + payload;
            File.AppendAllText(CurrentPath, payload, Utf8);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    private void Rotate()
    {
        var oldest = ArchivePath(_maxArchives);
        if (File.Exists(oldest))
            File.Delete(oldest);
        for (var index = _maxArchives - 1; index >= 1; index--)
        {
            var from = ArchivePath(index);
            if (File.Exists(from))
                File.Move(from, ArchivePath(index + 1), overwrite: true);
        }

        if (File.Exists(CurrentPath))
            File.Move(CurrentPath, ArchivePath(1), overwrite: true);
    }

    private string ArchivePath(int index) => Path.Combine(DirectoryPath, $"session.{index}.log");
}
