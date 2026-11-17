namespace PaliaVoiceAI.Core;

public interface IOcrEngine
{
    string Name { get; }
    Task<OcrResult> RecognizeAsync(CapturedImage image, CancellationToken cancellationToken = default);
}
