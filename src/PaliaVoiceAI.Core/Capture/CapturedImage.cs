namespace PaliaVoiceAI.Core;

public sealed class CapturedImage
{
    public CapturedImage(int width, int height, byte[] pixels)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Image dimensions must be positive.");
        if (pixels is null || pixels.Length != width * height * 4)
            throw new ArgumentException("Pixel buffer must be BGRA with width × height × 4 bytes.", nameof(pixels));

        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public int Width { get; }
    public int Height { get; }
    public byte[] Pixels { get; }
    public int Stride => Width * 4;
}
