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
    }
}
