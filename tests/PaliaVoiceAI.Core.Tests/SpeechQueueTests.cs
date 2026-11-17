using System.Globalization;
using System.Text;
using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class SpeechCacheTests
{
    [Fact]
    public void Key_changes_when_voice_pace_model_or_engine_changes()
    {
        var line = SpeechIdentity.Create("Hello there.", "bm_lewis", 1f, "abc", "KokoroSharp 0.8.4");
        var same = SpeechIdentity.Create("  Hello there.  ", "bm_lewis", 1.004f, "ABC", "KokoroSharp 0.8.4");
        Assert.Equal(SpeechCacheKey.Create(line), SpeechCacheKey.Create(same));
        Assert.Equal(1f, same.Pace);

        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(line with { VoiceId = "af_bella" }));
        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(SpeechIdentity.Create("Hello there.", "bm_lewis", 0.85f, "abc", "KokoroSharp 0.8.4")));
        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(SpeechIdentity.Create("Hello there.", "bm_lewis", 1.15f, "abc", "KokoroSharp 0.8.4")));
        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(line with { ModelHash = "def" }));
        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(line with { EngineVersion = "KokoroSharp 0.8.5" }));
        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(line with { Text = "Hello there!" }));
    }

    [Fact]
    public void Pace_outside_the_engine_range_uses_the_clamped_value()
    {
        var fast = SpeechIdentity.Create("Hello there.", "bm_lewis", 2f, "abc", "KokoroSharp 0.8.4");
        var clamped = SpeechIdentity.Create("Hello there.", "bm_lewis", 1.4f, "abc", "KokoroSharp 0.8.4");
        Assert.Equal(1.4f, fast.Pace);
        Assert.Equal(SpeechCacheKey.Create(clamped), SpeechCacheKey.Create(fast));
    }

    [Fact]
    public void Model_hash_matches_the_file_bytes()
    {
        var directory = CreateDirectory();
        try
        {
            var path = Path.Combine(directory, "model.bin");
            File.WriteAllBytes(path, "abc"u8.ToArray());
            var hash = ModelFileHash.Sha256(path);
            Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
            Assert.Equal(hash, ModelFileHash.Sha256(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Stored_audio_round_trips_and_old_files_can_be_deleted()
    {
        var cache = new SpeechCache(CreateDirectory());
        try
        {
            var first = SpeechCacheKey.Create(SpeechIdentity.Create("Hello there.", "bm_lewis", 1f, "abc", "KokoroSharp 0.8.4"));
            var second = SpeechCacheKey.Create(SpeechIdentity.Create("Come back tomorrow.", "bm_lewis", 1f, "abc", "KokoroSharp 0.8.4"));
            Assert.True(cache.TryStore(first, new byte[] { 1, 2, 3 }));
            Assert.True(cache.TryStore(second, new byte[] { 4, 5 }));
            Assert.False(cache.TryStore("../escape", new byte[] { 9 }));
            Assert.True(cache.TryRead(first, out var audio));
            Assert.Equal(new byte[] { 1, 2, 3 }, audio);

            File.SetLastWriteTimeUtc(Path.Combine(cache.DirectoryPath, first + ".wav"), DateTime.UtcNow.AddDays(-2));
            Assert.Equal(1, cache.DeleteOlderThan(DateTimeOffset.UtcNow.AddDays(-1)));
            Assert.False(cache.TryRead(first, out _));
            Assert.True(cache.TryRead(second, out _));
            Assert.Equal(1, cache.DeleteAll());
            Assert.False(cache.TryRead(second, out _));
        }
        finally
        {
            Directory.Delete(cache.DirectoryPath, recursive: true);
        }
    }

    [Fact]
    public void Wav_duration_reads_the_data_chunk()
    {
        var wav = new byte[44];
        Encoding.ASCII.GetBytes("RIFF").CopyTo(wav, 0);
        Encoding.ASCII.GetBytes("WAVE").CopyTo(wav, 8);
        Encoding.ASCII.GetBytes("fmt ").CopyTo(wav, 12);
        wav[16] = 16;
        wav[20] = 1;
        wav[22] = 1;
        wav[24] = 0xC0;
        wav[25] = 0x5D;
        wav[34] = 16;
        Encoding.ASCII.GetBytes("data").CopyTo(wav, 36);
        wav[40] = 0x80;
        wav[41] = 0xBB;

        Assert.Equal(TimeSpan.FromSeconds(1), WavDuration.Read(wav));
        Assert.Equal(TimeSpan.Zero, WavDuration.Read(new byte[] { 1, 2, 3 }));
    }

    private static string CreateDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "PaliaVoiceAI-cache-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

public class SpeechQueueTests
{
    [Fact]
    public async Task A_new_line_waits_until_the_current_line_finishes_playing()
    {
        var events = new List<string>();
        var playing = 0;
        var maxPlaying = 0;
        var generatedWhilePlaying = false;
        using var queue = new SpeechQueue(new SpeechCache(TempDirectory()), (request, _) =>
        {
            lock (events)
                events.Add("prepare:" + request.Text);
            var identity = SpeechIdentity.Create(request.Text, request.VoiceId, request.Pace, "abc", "KokoroSharp 0.8.4");
            return Task.FromResult(new PreparedSpeech(identity, async cancellationToken =>
            {
                if (Volatile.Read(ref playing) > 0)
                    generatedWhilePlaying = true;
                lock (events)
                    events.Add("generate:" + request.Text);
                await Task.Delay(40, cancellationToken);
                return new TtsResult(Encoding.UTF8.GetBytes(request.Text), "audio/wav", TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(500), "test");
            }));
        }, new ScriptOutput(async (wav, cancellationToken) =>
        {
            var now = Interlocked.Increment(ref playing);
            lock (events)
            {
                maxPlaying = Math.Max(maxPlaying, now);
                events.Add("play:" + Encoding.UTF8.GetString(wav));
            }
            try
            {
                await Task.Delay(40, cancellationToken);
            }
            finally
            {
                Interlocked.Decrement(ref playing);
            }
        }));

        var first = queue.SubmitAsync(Line("one"));
        var second = queue.SubmitAsync(Line("two"));
        await Task.WhenAll(first, second);

        Assert.False(generatedWhilePlaying);
        Assert.Equal(1, maxPlaying);
        Assert.Equal(
            new[] { "prepare:one", "generate:one", "play:one", "prepare:two", "generate:two", "play:two" },
            events);
    }

    [Fact]
    public async Task A_repeated_line_is_not_generated_again()
    {
        var generated = 0;
        var cache = new SpeechCache(TempDirectory());
        using var queue = new SpeechQueue(cache, (request, _) =>
        {
            var identity = SpeechIdentity.Create(request.Text, request.VoiceId, request.Pace, "abc", "KokoroSharp 0.8.4");
            return Task.FromResult(new PreparedSpeech(identity, _ =>
            {
                Interlocked.Increment(ref generated);
                var bytes = Encoding.UTF8.GetBytes(identity.Text + identity.Pace.ToString("0.00", CultureInfo.InvariantCulture));
                return Task.FromResult(new TtsResult(bytes, "audio/wav", TimeSpan.FromMilliseconds(25), TimeSpan.FromMilliseconds(25), "test"));
            }));
        }, new ScriptOutput((_, _) => Task.CompletedTask));

        var first = await queue.SubmitAsync(Line("Hello there."));
        var second = await queue.SubmitAsync(Line("Hello there."));
        var paced = await queue.SubmitAsync(Line("Hello there.", pace: 0.85f));

        Assert.False(first.FromCache);
        Assert.True(second.FromCache);
        Assert.False(paced.FromCache);
        Assert.Equal(2, generated);
        Assert.Equal(first.Audio, second.Audio);
        Assert.NotEqual(first.Audio, paced.Audio);
    }

    [Fact]
    public async Task Stop_drops_a_line_that_is_still_waiting()
    {
        var generated = 0;
        var output = new HoldOutput();
        using var queue = new SpeechQueue(new SpeechCache(TempDirectory()), (request, _) =>
        {
            Interlocked.Increment(ref generated);
            var identity = SpeechIdentity.Create(request.Text, request.VoiceId, request.Pace, "abc", "KokoroSharp 0.8.4");
            return Task.FromResult(new PreparedSpeech(identity, _ =>
                Task.FromResult(new TtsResult(new byte[] { 1 }, "audio/wav", TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1), "test"))));
        }, output);

        var first = queue.SubmitAsync(Line("one"));
        await output.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var second = queue.SubmitAsync(Line("two"));
        queue.Stop();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await first);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await second);
        await Task.Delay(50);
        Assert.Equal(1, generated);
    }

    private static SpeechRequest Line(string text, float pace = 1f) =>
        new(text, "bm_lewis", pace, TtsBackend.Cpu, 3, Play: true);

    private static string TempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "PaliaVoiceAI-queue-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class ScriptOutput : ISpeechOutput
    {
        private readonly Func<byte[], CancellationToken, Task> _play;

        public ScriptOutput(Func<byte[], CancellationToken, Task> play) => _play = play;

        public Task PlayAsync(byte[] wavBytes, CancellationToken cancellationToken) => _play(wavBytes, cancellationToken);

        public void Stop()
        {
        }
    }

    private sealed class HoldOutput : ISpeechOutput
    {
        private readonly CancellationTokenSource _hold = new();

        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task PlayAsync(byte[] wavBytes, CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _hold.Token);
            await Task.Delay(Timeout.Infinite, linked.Token);
        }

        public void Stop() => _hold.Cancel();
    }
}
