namespace PaliaVoiceAI.Core;

public static class WindowRegionMapper
{
    public static CaptureRegion Map(ScreenRect windowBounds, ScreenRect clientBounds, int frameWidth, int frameHeight, CaptureRegion clientRegion)
    {
        clientRegion.EnsureValid();
        if (clientBounds.IsEmpty)
            throw new ArgumentException("The game client area is empty.", nameof(clientBounds));

        if (frameWidth == clientBounds.Width && frameHeight == clientBounds.Height)
            return clientRegion;

        if (frameWidth == windowBounds.Width && frameHeight == windowBounds.Height)
        {
            return clientRegion.Offset(clientBounds.X - windowBounds.X, clientBounds.Y - windowBounds.Y);
        }

        var scaleX = frameWidth / (double)clientBounds.Width;
        var scaleY = frameHeight / (double)clientBounds.Height;
        return new CaptureRegion(
            (int)Math.Round(clientRegion.X * scaleX),
            (int)Math.Round(clientRegion.Y * scaleY),
            Math.Max(1, (int)Math.Round(clientRegion.Width * scaleX)),
            Math.Max(1, (int)Math.Round(clientRegion.Height * scaleY)));
    }
}
