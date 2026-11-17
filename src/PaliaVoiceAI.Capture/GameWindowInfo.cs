namespace PaliaVoiceAI.Capture;

public sealed record GameWindowInfo(
    nint Handle,
    string ProcessName,
    string Title,
    ScreenRect WindowBounds,
    ScreenRect ClientBounds);
