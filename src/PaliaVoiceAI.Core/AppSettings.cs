using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaliaVoiceAI.Core;

public sealed class AppSettings
{
    public const string DefaultVoiceId = "af_bella";

    public string Backend { get; set; } = nameof(TtsBackend.Cpu);
    public int Threads { get; set; } = 3;
    public string StudioVoiceId { get; set; } = DefaultVoiceId;
    public int VolumePercent { get; set; } = 100;

    [JsonIgnore]
    public TtsBackend BackendKind => Backend?.Trim().ToLowerInvariant() switch
    {
        "mock" => TtsBackend.Mock,
        "cuda" => TtsBackend.Cuda,
        _ => TtsBackend.Cpu
    };

    [JsonIgnore]
    public int ClampedThreads => Threads is 2 or 4 ? Threads : 3;

    [JsonIgnore]
    public string ClampedVoiceId
    {
        get
        {
            var match = KokoroVoices.English.FirstOrDefault(voice =>
                string.Equals(voice.Id, StudioVoiceId, StringComparison.OrdinalIgnoreCase));
            return match?.Id ?? DefaultVoiceId;
        }
    }

    [JsonIgnore]
    public int ClampedVolumePercent => Math.Clamp(VolumePercent, 0, 100);

    [JsonIgnore]
    public float VolumeLevel => ClampedVolumePercent / 100f;

    public AppSettings Normalized() => new()
    {
        Backend = BackendKind.ToString(),
        Threads = ClampedThreads,
        StudioVoiceId = ClampedVoiceId,
        VolumePercent = ClampedVolumePercent
    };
}

public static class AppSettingsStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static AppSettings Load(string? path = null)
    {
        path ??= AppPaths.Settings;
        try
        {
            if (!File.Exists(path))
                return new AppSettings();
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), Json)?.Normalized() ?? new AppSettings();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings, string? path = null)
    {
        path ??= AppPaths.Settings;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize((settings ?? new AppSettings()).Normalized(), Json));
        File.Move(temporary, path, overwrite: true);
    }
}
