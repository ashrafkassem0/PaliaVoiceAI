namespace PaliaVoiceAI.Core;

public interface ITtsEngine : IAsyncDisposable
{
    string Name { get; }
    Task InitializeAsync(TtsOptions options, CancellationToken cancellationToken = default);
    Task<TtsResult> GenerateAsync(string text, string voiceId, CancellationToken cancellationToken = default);
}
