using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PaliaVoiceAI.Core;
using PaliaVoiceAI.OCR;

namespace PaliaVoiceAI.OCR.Tests;

public class PaliaDialogueFixtureTests
{
    private const string SpokenLine = "I'd wish you a hoppu Luna New Year, but I'm so ready for it to be over already.";

    [Fact]
    public async Task Reads_the_spoken_line_from_the_kenyatta_frame()
    {
        Assert.True(WindowsOcrEngine.IsEnglishAvailable, WindowsOcrEngine.InstallHelp);

        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "palia-kenyatta-dialogue.jpg");
        var frame = Load(path);
        var card = BgraImage.Crop(frame, DialogueRegion.Suggest(frame.Width, frame.Height));
        var engine = new WindowsOcrEngine();

        var fromCard = TextNormalizer.Normalize((await engine.RecognizeAsync(card)).Text);
        var fromFrame = TextNormalizer.Normalize((await engine.RecognizeAsync(frame)).Text);

        Assert.Equal(SpokenLine, fromCard);
        Assert.Equal(SpokenLine, fromFrame);
    }

    private static CapturedImage Load(string path)
    {
        using var bitmap = new Bitmap(path);
        var pixels = new byte[bitmap.Width * bitmap.Height * 4];
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var rowBytes = bitmap.Width * 4;
            for (var y = 0; y < bitmap.Height; y++)
                Marshal.Copy(data.Scan0 + (y * data.Stride), pixels, y * rowBytes, rowBytes);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return new CapturedImage(bitmap.Width, bitmap.Height, pixels);
    }
}
