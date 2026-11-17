namespace PaliaVoiceAI.Core;

public interface ISpeechOutput
{
    Task PlayAsync(byte[] wavBytes, CancellationToken cancellationToken);
    void Stop();
}

public sealed record SpeechRequest(
    string Text,
    string VoiceId,
    float Pace,
    TtsBackend Backend,
    int Threads,
    bool Play);

public sealed record SpeechDelivery(
    byte[] Audio,
    bool FromCache,
    TimeSpan GenerationTime,
    TimeSpan AudioDuration,
    string EngineName,
    string Backend,
    string VoiceId,
    float Pace);

public sealed class PreparedSpeech
{
    public PreparedSpeech(SpeechIdentity identity, Func<CancellationToken, Task<TtsResult>> generate)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Generate = generate ?? throw new ArgumentNullException(nameof(generate));
    }

    public SpeechIdentity Identity { get; }
    public Func<CancellationToken, Task<TtsResult>> Generate { get; }
}

/// <summary>
/// One line at a time. A new line waits until the current line has finished playing.
/// Stop drops every waiting line and does not play a line that was cancelled.
/// </summary>
public sealed class SpeechQueue : IDisposable
{
    private readonly SpeechCache _cache;
    private readonly Func<SpeechRequest, CancellationToken, Task<PreparedSpeech>> _prepare;
    private readonly ISpeechOutput _output;
    private readonly CancellationTokenSource _dispose = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly object _gate = new();
    private readonly Queue<Job> _pending = new();
    private CancellationTokenSource _current = new();
    private bool _busy;
    private bool _disposed;

    public SpeechQueue(SpeechCache cache, Func<SpeechRequest, CancellationToken, Task<PreparedSpeech>> prepare, ISpeechOutput output)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _prepare = prepare ?? throw new ArgumentNullException(nameof(prepare));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _ = Task.Run(RunAsync);
    }

    public int PendingCount
    {
        get { lock (_gate) return _pending.Count; }
    }

    public bool IsBusy
    {
        get { lock (_gate) return _busy; }
    }

    public Task<SpeechDelivery> SubmitAsync(SpeechRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Text cannot be empty.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.VoiceId))
            throw new ArgumentException("Voice cannot be empty.", nameof(request));

        var job = new Job(request);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _pending.Enqueue(job);
        }

        _signal.Release();
        return job.Completion.Task;
    }

    public void Stop()
    {
        Job[] dropped;
        lock (_gate)
        {
            dropped = _pending.ToArray();
            _pending.Clear();
            _current.Cancel();
        }

        foreach (var job in dropped)
            job.Completion.TrySetCanceled();
        _output.Stop();
    }

    public void Dispose()
    {
        lock (_gate)
            _disposed = true;
        Stop();
        _dispose.Cancel();
        _signal.Release();
    }

    private async Task RunAsync()
    {
        try
        {
            while (!_dispose.IsCancellationRequested)
            {
                await _signal.WaitAsync(_dispose.Token).ConfigureAwait(false);
                Job? job;
                CancellationToken token;
                lock (_gate)
                {
                    if (!_pending.TryDequeue(out job))
                        continue;
                    _busy = true;
                    _current = new CancellationTokenSource();
                    token = _current.Token;
                }

                try
                {
                    var delivery = await SpeakOneAsync(job.Request, token).ConfigureAwait(false);
                    if (token.IsCancellationRequested)
                        job.Completion.TrySetCanceled(token);
                    else
                        job.Completion.TrySetResult(delivery);
                }
                catch (OperationCanceledException)
                {
                    job.Completion.TrySetCanceled(token);
                }
                catch (Exception ex)
                {
                    job.Completion.TrySetException(ex);
                }
                finally
                {
                    lock (_gate)
                        _busy = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<SpeechDelivery> SpeakOneAsync(SpeechRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var prepared = await _prepare(request, token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        var key = SpeechCacheKey.Create(prepared.Identity);
        TtsResult? generated = null;
        var fromCache = _cache.TryRead(key, out var audio) && audio.Length > 0;
        if (!fromCache)
        {
            generated = await prepared.Generate(token).ConfigureAwait(false);
            audio = generated.AudioBytes;
            _cache.TryStore(key, audio);
        }

        token.ThrowIfCancellationRequested();
        if (request.Play && audio.Length > 0)
            await _output.PlayAsync(audio, token).ConfigureAwait(false);

        var duration = generated?.AudioDuration ?? WavDuration.Read(audio);
        if (duration <= TimeSpan.Zero)
            duration = WavDuration.Read(audio);
        return new SpeechDelivery(
            audio,
            fromCache,
            generated?.GenerationTime ?? TimeSpan.Zero,
            duration,
            generated?.EngineName ?? prepared.Identity.EngineVersion,
            BackendLabel(generated, request),
            prepared.Identity.VoiceId,
            prepared.Identity.Pace);
    }

    private static string BackendLabel(TtsResult? generated, SpeechRequest request)
    {
        if (generated is null)
            return request.Backend.ToString();
        if (generated.EngineName.EndsWith("(Cpu)", StringComparison.Ordinal))
            return nameof(TtsBackend.Cpu);
        if (generated.EngineName.EndsWith("(Cuda)", StringComparison.Ordinal))
            return nameof(TtsBackend.Cuda);
        return request.Backend.ToString();
    }

    private sealed class Job
    {
        public Job(SpeechRequest request)
        {
            Request = request;
            Completion = new TaskCompletionSource<SpeechDelivery>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public SpeechRequest Request { get; }
        public TaskCompletionSource<SpeechDelivery> Completion { get; }
    }
}
