using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class KokoroModelLocatorTests
{
    [Fact]
    public void Finds_a_model_that_is_already_on_disk()
    {
        var directory = Path.Combine(Path.GetTempPath(), "PaliaVoiceAI-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var model = Path.Combine(directory, "kokoro-v1.0.onnx");
        File.WriteAllBytes(model, [1, 2, 3]);

        Assert.Equal(model, KokoroModelLocator.Find([directory]));
        Assert.Null(KokoroModelLocator.Find([Path.Combine(directory, "missing")]));
        Directory.Delete(directory, recursive: true);
    }

    [Fact]
    public void A_short_download_is_deleted()
    {
        var directory = CreateDirectory();
        try
        {
            var temporary = Path.Combine(directory, "kokoro.onnx.download");
            File.WriteAllBytes(temporary, [1, 2, 3]);
            var error = Assert.Throws<InvalidOperationException>(() =>
                KokoroModelLocator.AcceptDownload(temporary, Path.Combine(directory, "kokoro.onnx"), minimumBytes: 50));
            Assert.Contains("stopped early", error.Message);
            Assert.False(File.Exists(temporary));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void A_hash_mismatch_is_deleted_and_a_match_is_moved()
    {
        var directory = CreateDirectory();
        try
        {
            var temporary = Path.Combine(directory, "kokoro.onnx.download");
            var destination = Path.Combine(directory, "kokoro.onnx");
            File.WriteAllBytes(temporary, "kokoro-model"u8.ToArray());
            var hash = ModelFileHash.Sha256(temporary);
            var error = Assert.Throws<InvalidOperationException>(() =>
                KokoroModelLocator.AcceptDownload(temporary, destination, "deadbeef", minimumBytes: 1));
            Assert.Contains(hash, error.Message);
            Assert.False(File.Exists(temporary));
            Assert.False(File.Exists(destination));

            File.WriteAllBytes(temporary, "kokoro-model"u8.ToArray());
            KokoroModelLocator.AcceptDownload(temporary, destination, hash, minimumBytes: 1);
            Assert.False(File.Exists(temporary));
            Assert.Equal("kokoro-model"u8.ToArray(), File.ReadAllBytes(destination));
            Assert.Equal("0cfd5e79aab70a3d8c1a57dc639835110ddb32c9f5ff4fdd1f4db202ea43bb05", KokoroModelLocator.ExpectedSha256);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "palia-model-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
