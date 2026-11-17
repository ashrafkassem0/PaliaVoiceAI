using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PaliaVoiceAI.Capture;
using PaliaVoiceAI.Core;
using PaliaVoiceAI.OCR;

namespace PaliaVoiceAI.OCR.Tests;

public class WindowsOcrEngineTests
{
    [Fact]
    public async Task Recognizes_painted_english_text()
    {
        Assert.True(WindowsOcrEngine.IsEnglishAvailable, WindowsOcrEngine.InstallHelp);

        using var bitmap = new Bitmap(720, 160, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        using (var font = new Font("Segoe UI", 36, FontStyle.Bold, GraphicsUnit.Pixel))
        {
            graphics.Clear(Color.White);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            graphics.DrawString("Welcome to Kilima Village.", font, Brushes.Black, new PointF(24, 48));
        }

        var image = Copy(bitmap);
        var result = await new WindowsOcrEngine().RecognizeAsync(image);

        Assert.Contains("Kilima", result.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Windows OCR", result.EngineName);
    }

    [Fact]
    public void Copies_a_small_screen_region()
    {
        var image = GdiScreenCapture.Capture(new CaptureRegion(0, 0, 24, 24));
        Assert.Equal(24, image.Width);
        Assert.Equal(24, image.Height);
        Assert.Equal(24 * 24 * 4, image.Pixels.Length);
    }

    private static CapturedImage Copy(Bitmap bitmap)
    {
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
