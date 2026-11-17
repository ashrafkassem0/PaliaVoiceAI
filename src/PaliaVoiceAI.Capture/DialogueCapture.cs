namespace PaliaVoiceAI.Capture;

public sealed record CaptureTarget(bool UseGameWindow, CaptureRegion Region);

public sealed class DialogueCapture : IDisposable
{
    private readonly GameWindowDetector _detector = new();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private GameWindowCaptureSession? _session;
    private nint _sessionHandle;

    public GameWindowInfo? FindGameWindow() => _detector.Find();

    public async Task<CapturedImage> CaptureAsync(CaptureTarget target, CancellationToken cancellationToken = default)
    {
        target.Region.EnsureValid();
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!target.UseGameWindow)
                return GdiScreenCapture.Capture(target.Region);

            var window = EnsureGameWindow();
            if (GameWindowDetector.IsMinimized(window.Handle))
                throw new InvalidOperationException("Palia is minimized. Restore the game window.");

            var frame = await _session!.WaitForImageAsync(cancellationToken).ConfigureAwait(false);
            var current = GameWindowDetector.Read(window.Handle);
            var crop = WindowRegionMapper.Map(current.WindowBounds, current.ClientBounds, frame.Width, frame.Height, target.Region);
            return BgraImage.Crop(frame, crop);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Release()
    {
        _gate.Wait();
        try
        {
            DisposeSession();
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        Release();
        _gate.Dispose();
    }

    private GameWindowInfo EnsureGameWindow()
    {
        if (_session is not null && GameWindowDetector.IsAlive(_sessionHandle))
            return GameWindowDetector.Read(_sessionHandle);

        DisposeSession();
        var window = _detector.Find()
            ?? throw new InvalidOperationException("Palia is not running. Launch the game, or turn off game-window capture and use a manual screen region.");
        _session = GameWindowCaptureSession.Start(window.Handle);
        _sessionHandle = window.Handle;
        return window;
    }

    private void DisposeSession()
    {
        _session?.Dispose();
        _session = null;
        _sessionHandle = 0;
    }
}
