using NAudio.Wave;
using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Audio;

public sealed class AudioPlayer : ISpeechOutput, IDisposable
{
    private readonly object _gate = new();
    private WaveOutEvent? _output;
    private MemoryStream? _stream;
    private WaveFileReader? _reader;
    private TaskCompletionSource? _done;
    private int _playId;
    private bool _stopRequested;
    private float _volume = 1f;

    public float Volume
    {
        get
        {
            lock (_gate)
                return _volume;
        }
        set
        {
            lock (_gate)
            {
                _volume = Math.Clamp(value, 0f, 1f);
                if (_output is not null)
                    _output.Volume = _volume;
            }
        }
    }

    public void PlayWav(byte[] wavBytes)
    {
        var ignored = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Start(wavBytes, ignored);
    }

    public async Task PlayAsync(byte[] wavBytes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Start(wavBytes, done);
        var registration = cancellationToken.Register(static state => ((AudioPlayer)state!).Stop(), this);
        try
        {
            await done.Task.ConfigureAwait(false);
        }
        finally
        {
            registration.Dispose();
        }
    }

    public void Stop() => DetachAndStopPrevious();

    public void Dispose() => Stop();

    private void Start(byte[] wavBytes, TaskCompletionSource done)
    {
        DetachAndStopPrevious();
        WaveOutEvent output;
        lock (_gate)
        {
            _stopRequested = false;
            var playId = ++_playId;
            MemoryStream? stream = null;
            WaveFileReader? reader = null;
            try
            {
                stream = new MemoryStream(wavBytes, writable: false);
                reader = new WaveFileReader(stream);
                output = new WaveOutEvent();
                output.PlaybackStopped += (_, _) => FinishNaturally(playId);
                output.Init(reader);
                output.Volume = _volume;
                _stream = stream;
                _reader = reader;
                _output = output;
                _done = done;
            }
            catch
            {
                _stream = null;
                _reader = null;
                _output = null;
                try
                {
                    reader?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }

                stream?.Dispose();
                done.TrySetCanceled();
                throw;
            }
        }

        try
        {
            output.Play();
        }
        catch (Exception ex) when (done.Task.IsCanceled)
        {
            throw new OperationCanceledException("Playback stopped.", ex);
        }
        catch
        {
            Stop();
            throw;
        }
    }

    private void FinishNaturally(int playId)
    {
        TaskCompletionSource? done;
        lock (_gate)
        {
            if (playId != _playId || _stopRequested)
                return;
            done = _done;
            _done = null;
        }

        done?.TrySetResult();
    }

    private void DetachAndStopPrevious()
    {
        WaveOutEvent? output;
        WaveFileReader? reader;
        MemoryStream? stream;
        TaskCompletionSource? done;
        lock (_gate)
        {
            _stopRequested = true;
            done = _done;
            _done = null;
            output = _output;
            reader = _reader;
            stream = _stream;
            _output = null;
            _reader = null;
            _stream = null;
        }

        try
        {
            output?.Stop();
        }
        catch (ObjectDisposedException)
        {
        }

        output?.Dispose();
        reader?.Dispose();
        stream?.Dispose();
        done?.TrySetCanceled();
    }
}
