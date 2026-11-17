using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;

namespace PaliaVoiceAI.Capture;

internal static class Direct3D11Device
{
    public static IDirect3DDevice Create()
    {
        var hr = D3D11CreateDevice(
            IntPtr.Zero,
            1,
            IntPtr.Zero,
            0x20,
            IntPtr.Zero,
            0,
            7,
            out var device,
            out _,
            out var context);
        if (hr < 0)
            Marshal.ThrowExceptionForHR(hr);

        Marshal.Release(context);
        var dxgiDevice = IntPtr.Zero;
        var graphicsDevice = IntPtr.Zero;
        try
        {
            var iid = new Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c");
            hr = Marshal.QueryInterface(device, in iid, out dxgiDevice);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);

            hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice, out graphicsDevice);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);

            return MarshalInterface<IDirect3DDevice>.FromAbi(graphicsDevice);
        }
        finally
        {
            if (graphicsDevice != IntPtr.Zero)
                Marshal.Release(graphicsDevice);
            if (dxgiDevice != IntPtr.Zero)
                Marshal.Release(dxgiDevice);
            Marshal.Release(device);
        }
    }

    [DllImport("d3d11.dll", ExactSpelling = true)]
    private static extern int D3D11CreateDevice(
        IntPtr pAdapter,
        int driverType,
        IntPtr software,
        uint flags,
        IntPtr featureLevels,
        uint featureLevelCount,
        uint sdkVersion,
        out IntPtr device,
        out int featureLevel,
        out IntPtr immediateContext);

    [DllImport("d3d11.dll", ExactSpelling = true)]
    private static extern int CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);
}
