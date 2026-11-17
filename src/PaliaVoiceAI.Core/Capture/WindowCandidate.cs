namespace PaliaVoiceAI.Core;

public sealed record WindowCandidate(
    string ProcessName,
    string Title,
    int ClientWidth,
    int ClientHeight,
    bool IsVisible,
    bool IsMinimized);
