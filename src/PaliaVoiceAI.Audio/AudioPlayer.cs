using NAudio.Wave;

namespace PaliaVoiceAI.Audio;

public sealed class AudioPlayer : IDisposable
{
    private WaveOutEvent? _output;
    private MemoryStream? _stream;
    private WaveFileReader? _reader;

    public void PlayWav(byte[] wavBytes)
    {
        Stop();
        _stream = new MemoryStream(wavBytes, writable: false);
        _reader = new WaveFileReader(_stream);
        _output = new WaveOutEvent();
        _output.Init(_reader);
        _output.Play();
    }

    public void Stop()
    {
        _output?.Stop();
        _output?.Dispose();
        _reader?.Dispose();
        _stream?.Dispose();
        _output = null;
        _reader = null;
        _stream = null;
    }

    public void Dispose() => Stop();
}
