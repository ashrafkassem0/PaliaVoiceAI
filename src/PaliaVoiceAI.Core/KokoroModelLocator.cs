namespace PaliaVoiceAI.Core;

public static class KokoroModelLocator
{
    public static readonly string[] FileNames = ["kokoro.onnx", "kokoro-v1.0.onnx"];

    public const string DownloadUrl = "https://github.com/Lyrcaxis/KokoroSharpBinaries/releases/download/v2.0.0/kokoro.onnx";

    public static string? Find() => Find(DefaultDirectories());

    public static string? Find(IEnumerable<string> directories)
    {
        foreach (var directory in directories)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                continue;

            foreach (var name in FileNames)
            {
                var path = Path.Combine(directory, name);
                if (File.Exists(path))
                    return path;
            }
        }

        return null;
    }

    public static IEnumerable<string> DefaultDirectories()
    {
        yield return AppPaths.ModelsDirectory;
        yield return AppContext.BaseDirectory;
        yield return Environment.CurrentDirectory;
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }
}
