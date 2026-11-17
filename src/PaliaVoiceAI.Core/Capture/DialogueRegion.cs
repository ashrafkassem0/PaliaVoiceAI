namespace PaliaVoiceAI.Core;

public static class DialogueRegion
{
    // The dialogue card on a 1024×576 Palia frame, including the name plate and excluding the chat chip.
    private const double OriginX = 70d / 1024d;
    private const double OriginY = 330d / 576d;
    private const double SizeX = 520d / 1024d;
    private const double SizeY = 210d / 576d;

    public static CaptureRegion Suggest(int clientWidth, int clientHeight)
    {
        if (clientWidth <= 0 || clientHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(clientWidth), "Client size must be positive.");

        var x = Scale(clientWidth, OriginX);
        var y = Scale(clientHeight, OriginY);
        var width = Scale(clientWidth, SizeX);
        var height = Scale(clientHeight, SizeY);
        if (x >= clientWidth || y >= clientHeight)
            return new CaptureRegion(0, 0, Math.Min(clientWidth, width), Math.Min(clientHeight, height));
        if (x + width > clientWidth)
            width = clientWidth - x;
        if (y + height > clientHeight)
            height = clientHeight - y;
        return new CaptureRegion(x, y, width, height);
    }

    private static int Scale(int size, double ratio) => Math.Max(1, (int)Math.Round(size * ratio));
}
