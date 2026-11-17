using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;

namespace PaliaVoiceAI.Capture;

internal sealed class GameWindowCaptureSession : IDisposable
{
    private readonly IDirect3DDevice _device;
    private readonly GraphicsCaptureItem _item;
    private Direct3D11CaptureFramePool _framePool;
    private GraphicsCaptureSession _session;
    private bool _disposed;

    private GameWindowCaptureSession(IDirect3DDevice device, GraphicsCaptureItem item, Direct3D11CaptureFramePool framePool, GraphicsCaptureSession session)
    {
        _device = device;
        _item = item;
        _framePool = framePool;
        _session = session;
    }

    public static GameWindowCaptureSession Start(nint hwnd)
    {
        var device = Direct3D11Device.Create();
        var item = GraphicsCaptureItemInterop.CreateForWindow(hwnd);
        var size = item.Size;
        if (size.Width <= 0 || size.Height <= 0)
            throw new InvalidOperationException("Palia returned an empty capture size. Restore the game window and try again.");

        var pool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            device,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            2,
            size);
        var session = pool.CreateCaptureSession(item);
        session.IsCursorCaptureEnabled = false;
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            session.IsBorderRequired = false;
        session.StartCapture();
        return new GameWindowCaptureSession(device, item, pool, session);
    }

    public async Task<CapturedImage> WaitForImageAsync(CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var frame = TakeLatestFrame();
            if (frame is not null)
            {
                var content = frame.ContentSize;
                if (content.Width > 0 && content.Height > 0 &&
                    (content.Width != _item.Size.Width || content.Height != _item.Size.Height))
                {
                    _framePool.Recreate(_device, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, content);
                }

                using var bitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(frame.Surface);
                return await SoftwareBitmapCopy.ToCapturedImageAsync(bitmap).ConfigureAwait(false);
            }

            await Task.Delay(30, cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException("No frame arrived from the Palia window. Restore the game if it is minimized, then try again.");
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _session.Dispose();
        _framePool.Dispose();
        (_device as IDisposable)?.Dispose();
    }

    private Direct3D11CaptureFrame? TakeLatestFrame()
    {
        Direct3D11CaptureFrame? latest = null;
        while (_framePool.TryGetNextFrame() is { } frame)
        {
            latest?.Dispose();
            latest = frame;
        }

        return latest;
    }
}

internal static class SoftwareBitmapCopy
{
    public static async Task<CapturedImage> ToCapturedImageAsync(SoftwareBitmap bitmap)
    {
        using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
        encoder.SetSoftwareBitmap(bitmap);
        await encoder.FlushAsync();
        stream.Seek(0);

        var length = (uint)stream.Size;
        var bytes = new byte[length];
        var reader = new Windows.Storage.Streams.DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync(length);
        reader.ReadBytes(bytes);
        reader.Dispose();
        return BgraImage.DecodeBmp(bytes);
    }
}
