using KokoroSharp;
using KokoroSharp.Core;
using KokoroSharp.Processing;
using Microsoft.ML.OnnxRuntime;
using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.TTS;

public sealed class KokoroTtsEngine : ITtsEngine
{
    public const string EngineName = "Kokoro 82M";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private KokoroWavSynthesizer? _synth;
    private TtsBackend _loadedBackend;
    private int _loadedThreads = -1;
    private bool _cudaUnavailable;

    public string Name => EngineName;

    public TtsBackend ActiveBackend { get; private set; } = TtsBackend.Cpu;

    public event Action<string>? Progress;

    public async Task InitializeAsync(TtsOptions options, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var backend = ResolveBackend(options.Backend);
            if (_synth is not null && _loadedBackend == backend && _loadedThreads == options.IntraOpThreads)
            {
                ActiveBackend = backend;
                return;
            }

            var modelPath = await KokoroModelDownloader.EnsureAsync(Report, cancellationToken).ConfigureAwait(false);
            Report(backend == TtsBackend.Cuda ? "Loading Kokoro on CUDA." : "Loading Kokoro on CPU.");
            await Task.Run(() => ReplaceSynth(modelPath, options, backend), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<TtsResult> GenerateAsync(string text, string voiceId, CancellationToken cancellationToken = default, float speed = 1f)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));
        if (_synth is null)
            throw new InvalidOperationException("Kokoro is not loaded. Generate again so the model can download.");

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            KokoroVoice voice;
            try
            {
                voice = KokoroVoiceManager.GetVoice(voiceId);
            }
            catch (Exception ex) when (ex is InvalidOperationException or DirectoryNotFoundException)
            {
                throw new InvalidOperationException($"Kokoro voice '{voiceId}' is not next to the app. Expected a voices folder containing {voiceId}.npy.", ex);
            }

            var pace = Math.Clamp(speed, 0.7f, 1.4f);
            KokoroTTSPipelineConfig? pipeline = null;
            if (Math.Abs(pace - 1f) > 0.01f)
            {
                pipeline = new KokoroTTSPipelineConfig { Speed = pace };
            }

            var started = System.Diagnostics.Stopwatch.StartNew();
            var pcm = await _synth.SynthesizeAsync(text, voice, pipeline).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            started.Stop();

            var wav = PcmWav.Wrap(pcm, sampleRate: 24_000);
            var duration = TimeSpan.FromSeconds(pcm.Length / 2d / 24_000d);
            return new TtsResult(wav, "audio/wav", started.Elapsed, duration, $"{EngineName} ({ActiveBackend})");
        }
        finally
        {
            _gate.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        _synth?.Dispose();
        _synth = null;
        _gate.Dispose();
        return ValueTask.CompletedTask;
    }

    private TtsBackend ResolveBackend(TtsBackend requested)
    {
        if (requested == TtsBackend.Cuda && _cudaUnavailable)
        {
            Report("CUDA was already unavailable in this session. Using CPU.");
            return TtsBackend.Cpu;
        }

        return requested == TtsBackend.Cuda ? TtsBackend.Cuda : TtsBackend.Cpu;
    }

    private void ReplaceSynth(string modelPath, TtsOptions options, TtsBackend backend)
    {
        _synth?.Dispose();
        _synth = null;

        var session = CreateSession(options, backend);
        if (backend == TtsBackend.Cuda && _cudaUnavailable)
            backend = TtsBackend.Cpu;

        try
        {
            _synth = new KokoroWavSynthesizer(modelPath, session);
        }
        catch when (backend == TtsBackend.Cuda)
        {
            session.Dispose();
            _cudaUnavailable = true;
            Report("CUDA could not load the Kokoro model. Using CPU.");
            session = CreateSession(options, TtsBackend.Cpu);
            _synth = new KokoroWavSynthesizer(modelPath, session);
            backend = TtsBackend.Cpu;
        }

        _loadedBackend = backend;
        _loadedThreads = options.IntraOpThreads;
        ActiveBackend = backend;
    }

    private SessionOptions CreateSession(TtsOptions options, TtsBackend backend)
    {
        var session = new SessionOptions
        {
            IntraOpNumThreads = Math.Clamp(options.IntraOpThreads, 1, 16),
            InterOpNumThreads = Math.Clamp(options.InterOpThreads, 1, 4),
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
        };

        if (backend != TtsBackend.Cuda)
            return session;

        if (TryEnableCuda(session, out var error))
            return session;

        session.Dispose();
        _cudaUnavailable = true;
        Report(string.IsNullOrWhiteSpace(error)
            ? "CUDA is not available. Using CPU."
            : $"CUDA is not available ({error}). Using CPU.");
        return CreateSession(options, TtsBackend.Cpu);
    }

    private static bool TryEnableCuda(SessionOptions session, out string error)
    {
        var method = typeof(SessionOptions).GetMethods().FirstOrDefault(candidate =>
            candidate.Name == "AppendExecutionProvider_CUDA" && candidate.GetParameters().Length <= 1);
        if (method is null)
        {
            error = "the ONNX runtime in this build does not include the CUDA provider";
            return false;
        }

        try
        {
            var parameter = method.GetParameters();
            method.Invoke(session, parameter.Length == 1 ? [0] : null);
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
    }

    private void Report(string message) => Progress?.Invoke(message);
}

internal static class PcmWav
{
    public static byte[] Wrap(byte[] pcm, int sampleRate)
    {
        if (pcm.Length >= 12 && pcm[0] == (byte)'R' && pcm[1] == (byte)'I' && pcm[2] == (byte)'F' && pcm[3] == (byte)'F')
            return pcm;

        const short channels = 1;
        const short bits = 16;
        var dataLength = pcm.Length;
        var file = new byte[44 + dataLength];
        Write(file, 0, "RIFF"u8);
        WriteInt(file, 4, 36 + dataLength);
        Write(file, 8, "WAVE"u8);
        Write(file, 12, "fmt "u8);
        WriteInt(file, 16, 16);
        WriteShort(file, 20, 1);
        WriteShort(file, 22, channels);
        WriteInt(file, 24, sampleRate);
        WriteInt(file, 28, sampleRate * channels * bits / 8);
        WriteShort(file, 32, (short)(channels * bits / 8));
        WriteShort(file, 34, bits);
        Write(file, 36, "data"u8);
        WriteInt(file, 40, dataLength);
        Buffer.BlockCopy(pcm, 0, file, 44, dataLength);
        return file;
    }

    private static void Write(byte[] buffer, int offset, ReadOnlySpan<byte> value) => value.CopyTo(buffer.AsSpan(offset));

    private static void WriteInt(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    private static void WriteShort(byte[] buffer, int offset, short value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }
}
