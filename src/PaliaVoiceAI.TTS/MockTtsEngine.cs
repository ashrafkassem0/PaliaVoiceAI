using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.TTS;

// This engine validates the application pipeline only. It does NOT generate speech.
public sealed class MockTtsEngine : ITtsEngine
{
    public string Name => "Pipeline Mock (no speech synthesis)";

    public Task InitializeAsync(TtsOptions options, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public async Task<TtsResult> GenerateAsync(string text, string voiceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));

        var started = System.Diagnostics.Stopwatch.StartNew();
        await Task.Delay(Math.Clamp(text.Length * 8, 250, 2500), cancellationToken);
        started.Stop();

        // A short valid silent WAV lets us verify playback wiring without pretending to synthesize speech.
        var wav = SilentWav.Create(TimeSpan.FromMilliseconds(Math.Clamp(text.Length * 65, 700, 8000)));
        return new TtsResult(wav, "audio/wav", started.Elapsed,
            TimeSpan.FromMilliseconds(Math.Clamp(text.Length * 65, 700, 8000)), Name);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal static class SilentWav
{
    public static byte[] Create(TimeSpan duration)
    {
        const int sampleRate = 22050, channels = 1, bits = 16;
        int dataLength = (int)(duration.TotalSeconds * sampleRate * channels * bits / 8);
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + dataLength);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16); bw.Write((short)1); bw.Write((short)channels);
        bw.Write(sampleRate); bw.Write(sampleRate * channels * bits / 8);
        bw.Write((short)(channels * bits / 8)); bw.Write((short)bits);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(dataLength);
        bw.Write(new byte[dataLength]);
        return ms.ToArray();
    }
}
