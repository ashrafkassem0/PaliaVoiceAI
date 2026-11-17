using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using WinRT;

namespace PaliaVoiceAI.Capture;

internal static class GraphicsCaptureItemInterop
{
    private static readonly Guid ItemId = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    public static GraphicsCaptureItem CreateForWindow(nint hwnd)
    {
        if (!GraphicsCaptureSession.IsSupported())
            throw new InvalidOperationException("Windows Graphics Capture is not available on this system.");

        var factory = ActivationFactory.Get("Windows.Graphics.Capture.GraphicsCaptureItem");
        var interop = (IGraphicsCaptureItemInterop)factory;
        var iid = ItemId;
        var hr = interop.CreateForWindow(hwnd, ref iid, out var pointer);
        if (hr < 0 || pointer == IntPtr.Zero)
            Marshal.ThrowExceptionForHR(hr < 0 ? hr : unchecked((int)0x80004005));

        try
        {
            return GraphicsCaptureItem.FromAbi(pointer);
        }
        finally
        {
            Marshal.Release(pointer);
        }
    }

    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        [PreserveSig]
        int CreateForWindow(nint window, ref Guid iid, out IntPtr result);

        [PreserveSig]
        int CreateForMonitor(nint monitor, ref Guid iid, out IntPtr result);
    }
}
