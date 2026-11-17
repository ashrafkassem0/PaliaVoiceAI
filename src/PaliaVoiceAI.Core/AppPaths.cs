namespace PaliaVoiceAI.Core;

public static class AppPaths
{
    public static string Root => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PaliaVoiceAI");

    public static string BenchmarksCsv => Path.Combine(Root, "benchmarks.csv");
    public static string CaptureSettings => Path.Combine(Root, "capture.json");
    public static string Settings => Path.Combine(Root, "settings.json");
    public static string Logs => Path.Combine(Root, "logs");
    public static string LastCaptureBmp => Path.Combine(Root, "last-capture.bmp");
    public static string ModelsDirectory => Path.Combine(Root, "models");
    public static string KokoroOnnx => Path.Combine(ModelsDirectory, "kokoro.onnx");
    public static string VoiceProfiles => Path.Combine(Root, "voices.json");
    public static string NpcVoices => Path.Combine(Root, "npc-voices.json");
    public static string SpeechCache => Path.Combine(Root, "cache");
}
