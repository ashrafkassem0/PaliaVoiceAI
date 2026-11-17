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

    public const string ExpectedSha256 = "0cfd5e79aab70a3d8c1a57dc639835110ddb32c9f5ff4fdd1f4db202ea43bb05";
    public const long MinimumDownloadBytes = 50_000_000;

    public static void AcceptDownload(string temporaryPath, string destinationPath, string? expectedSha256 = null, long minimumBytes = MinimumDownloadBytes)
    {
        var info = new FileInfo(temporaryPath);
        if (!info.Exists)
            throw new FileNotFoundException("The Kokoro download is missing.", temporaryPath);

        var length = info.Length;
        if (length < minimumBytes)
        {
            info.Delete();
            throw new InvalidOperationException($"The Kokoro download stopped early ({length:N0} bytes).");
        }

        var expected = string.IsNullOrWhiteSpace(expectedSha256) ? ExpectedSha256 : expectedSha256;
        var hash = ModelFileHash.Sha256(info.FullName);
        if (!string.Equals(hash, expected, StringComparison.OrdinalIgnoreCase))
        {
            info.Delete();
            throw new InvalidOperationException($"The Kokoro download hash was {hash}. Expected {expected}.");
        }

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.Move(info.FullName, destinationPath, overwrite: true);
    }

    public static IEnumerable<string> DefaultDirectories()
    {
        yield return AppPaths.ModelsDirectory;
        yield return AppContext.BaseDirectory;
        yield return Environment.CurrentDirectory;
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }
}
