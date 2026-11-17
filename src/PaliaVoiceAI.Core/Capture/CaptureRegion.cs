namespace PaliaVoiceAI.Core;

public readonly record struct CaptureRegion(int X, int Y, int Width, int Height)
{
    public const int MaxEdge = 4096;

    public bool IsValid =>
        Width > 0 && Height > 0 &&
        Width <= MaxEdge && Height <= MaxEdge &&
        X is >= -20000 and <= 20000 &&
        Y is >= -20000 and <= 20000;

    public void EnsureValid()
    {
        if (Width <= 0 || Height <= 0 || Width > MaxEdge || Height > MaxEdge)
            throw new ArgumentOutOfRangeException(nameof(Width), "Capture width and height must be between 1 and 4096 pixels.");
        if (X is < -20000 or > 20000 || Y is < -20000 or > 20000)
            throw new ArgumentOutOfRangeException(nameof(X), "Capture position is outside the supported screen range.");
    }

    public CaptureRegion Offset(int dx, int dy) => new(X + dx, Y + dy, Width, Height);
}
