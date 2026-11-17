using System.Runtime.InteropServices;
using Windows.Graphics.Capture;

namespace PaliaVoiceAI.Capture;

internal static class GraphicsCaptureItemInterop
{
    private static readonly Guid ItemId = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    public static GraphicsCaptureItem CreateForWindow(nint hwnd)
    {
        if (!GraphicsCaptureSession.IsSupported())
            throw new InvalidOperationException("Windows Graphics Capture is not available on this system.");

        // ActivationFactory.Get returns a CsWinRT wrapper. That wrapper cannot be cast
        // to this COM interface, so ask combase for the factory pointer directly.
        var className = "Windows.Graphics.Capture.GraphicsCaptureItem";
        var factoryId = typeof(IGraphicsCaptureItemInterop).GUID;
        var itemId = ItemId;
        var classNamePtr = IntPtr.Zero;
        var factoryPtr = IntPtr.Zero;
        object? factoryObject = null;
        var itemPtr = IntPtr.Zero;
        try
        {
            var hr = WindowsCreateString(className, (uint)className.Length, out classNamePtr);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);

            hr = RoGetActivationFactory(classNamePtr, ref factoryId, out factoryPtr);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);

            factoryObject = Marshal.GetObjectForIUnknown(factoryPtr);
            hr = ((IGraphicsCaptureItemInterop)factoryObject).CreateForWindow(hwnd, ref itemId, out itemPtr);
            if (hr < 0 || itemPtr == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(hr < 0 ? hr : unchecked((int)0x80004005));

            return GraphicsCaptureItem.FromAbi(itemPtr);
        }
        finally
        {
            if (itemPtr != IntPtr.Zero)
                Marshal.Release(itemPtr);
            if (factoryObject is not null)
                Marshal.ReleaseComObject(factoryObject);
            if (factoryPtr != IntPtr.Zero)
                Marshal.Release(factoryPtr);
            if (classNamePtr != IntPtr.Zero)
                WindowsDeleteString(classNamePtr);
        }
    }

    [DllImport("combase.dll", ExactSpelling = true)]
    private static extern int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, out IntPtr factory);

    [DllImport("combase.dll", ExactSpelling = true)]
    private static extern int WindowsCreateString([MarshalAs(UnmanagedType.LPWStr)] string sourceString, uint length, out IntPtr hstring);

    [DllImport("combase.dll", ExactSpelling = true)]
    private static extern int WindowsDeleteString(IntPtr hstring);

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
