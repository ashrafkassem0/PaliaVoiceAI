using System.IO;
using System.Text.Json;
using PaliaVoiceAI.Capture;
using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.App;

public sealed class CaptureSettings
{
    public int X { get; set; } = 131;
    public int Y { get; set; } = 688;
    public int Width { get; set; } = 975;
    public int Height { get; set; } = 438;
    public int PollIntervalMs { get; set; } = 400;
    public int StableReads { get; set; } = 2;
    public int DuplicateWindowSeconds { get; set; } = 8;
    public bool UseGameWindow { get; set; } = true;
    public bool CopyAcceptedText { get; set; } = true;

    public CaptureTarget ToTarget() => new(UseGameWindow, new CaptureRegion(X, Y, Width, Height));
}

public static class CaptureSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static CaptureSettings Load()
    {
        try
        {
            if (!File.Exists(AppPaths.CaptureSettings))
                return new CaptureSettings();
            return JsonSerializer.Deserialize<CaptureSettings>(File.ReadAllText(AppPaths.CaptureSettings)) ?? new CaptureSettings();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new CaptureSettings();
        }
    }

    public static void Save(CaptureSettings settings)
    {
        Directory.CreateDirectory(AppPaths.Root);
        File.WriteAllText(AppPaths.CaptureSettings, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
