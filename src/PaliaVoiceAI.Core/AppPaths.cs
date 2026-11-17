namespace PaliaVoiceAI.Core;

public static class AppPaths
{
    public static string Root => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PaliaVoiceAI");

    public static string BenchmarksCsv => Path.Combine(Root, "benchmarks.csv");
    public static string CaptureSettings => Path.Combine(Root, "capture.json");
    public static string LastCaptureBmp => Path.Combine(Root, "last-capture.bmp");
}
