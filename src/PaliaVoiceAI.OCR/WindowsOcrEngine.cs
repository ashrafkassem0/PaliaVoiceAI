using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using OcrResult = PaliaVoiceAI.Core.OcrResult;

namespace PaliaVoiceAI.OCR;

public sealed class WindowsOcrEngine : IOcrEngine
{
    public const string InstallHelp =
        "English Windows OCR is not installed. Open Settings > Time & language > Language & region, add English if needed, then install the Optical character recognition language feature.";

    private OcrEngine? _engine;

    public string Name => "Windows OCR";

    public static bool IsEnglishAvailable => FindEnglishLanguage() is not null;

    public async Task<OcrResult> RecognizeAsync(CapturedImage image, CancellationToken cancellationToken = default)
    {
        var engine = GetEngine();
        using var bitmap = await CreateBitmapAsync(image).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var operation = engine.RecognizeAsync(bitmap);
        using var registration = cancellationToken.Register(() => operation.Cancel());
        var result = await operation;
        cancellationToken.ThrowIfCancellationRequested();
        var text = string.Join('\n', result.Lines.Select(line => line.Text));
        return new OcrResult(text, Name);
    }

    private OcrEngine GetEngine()
    {
        if (_engine is not null)
            return _engine;

        var language = FindEnglishLanguage()
            ?? throw new InvalidOperationException(InstallHelp);
        _engine = OcrEngine.TryCreateFromLanguage(language)
            ?? throw new InvalidOperationException(InstallHelp);
        return _engine;
    }

    private static Language? FindEnglishLanguage()
    {
        return OcrEngine.AvailableRecognizerLanguages.FirstOrDefault(language =>
            language.LanguageTag.StartsWith("en", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<SoftwareBitmap> CreateBitmapAsync(CapturedImage image)
    {
        var encoded = BgraImage.EncodeBmp(image);
        using var stream = new InMemoryRandomAccessStream();
        var writer = new DataWriter(stream);
        writer.WriteBytes(encoded);
        await writer.StoreAsync();
        writer.DetachStream();
        writer.Dispose();
        stream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(stream);
        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
    }
}
