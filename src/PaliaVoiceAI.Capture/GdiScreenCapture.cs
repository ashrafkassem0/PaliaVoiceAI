using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaliaVoiceAI.Capture;

public static class GdiScreenCapture
{
    public static CapturedImage Capture(CaptureRegion region)
    {
        region.EnsureValid();
        try
        {
            using var bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(region.X, region.Y, 0, 0, new Size(region.Width, region.Height), CopyPixelOperation.SourceCopy);
            }

            return Copy(bitmap);
        }
        catch (Exception ex) when (ex is not ArgumentOutOfRangeException)
        {
            throw new InvalidOperationException(
                $"Could not copy the screen region ({region.X}, {region.Y}, {region.Width}×{region.Height}). {ex.Message}",
                ex);
        }
    }

    private static CapturedImage Copy(Bitmap bitmap)
    {
        var pixels = new byte[bitmap.Width * bitmap.Height * 4];
        var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var rowBytes = bitmap.Width * 4;
            for (var y = 0; y < bitmap.Height; y++)
                Marshal.Copy(data.Scan0 + y * data.Stride, pixels, y * rowBytes, rowBytes);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return new CapturedImage(bitmap.Width, bitmap.Height, pixels);
    }
}
