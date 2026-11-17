using System.Diagnostics;
using System.Text;

namespace PaliaVoiceAI.Capture;

public sealed class GameWindowDetector
{
    public GameWindowInfo? Find()
    {
        var windows = EnumerateVisibleWindows();
        var index = GameWindowSelector.SelectIndex(windows.Select(window => window.Candidate).ToArray());
        return index is int chosen ? windows[chosen].Info : null;
    }

    public static bool IsAlive(nint handle)
    {
        return handle != 0 && IsWindow(handle) && !IsIconic(handle);
    }

    public static bool IsMinimized(nint handle) => handle != 0 && IsWindow(handle) && IsIconic(handle);

    public static GameWindowInfo Read(nint handle)
    {
        if (handle == 0 || !IsWindow(handle))
            throw new InvalidOperationException("The Palia window is no longer open.");

        GetWindowThreadProcessId(handle, out var pid);
        var processName = ReadProcessName((int)pid);
        return new GameWindowInfo(handle, processName, ReadTitle(handle), ReadWindowBounds(handle), ReadClientBounds(handle));
    }

    private static List<(WindowCandidate Candidate, GameWindowInfo Info)> EnumerateVisibleWindows()
    {
        var windows = new List<(WindowCandidate, GameWindowInfo)>();
        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd) || IsIconic(hwnd))
                return true;

            GetWindowThreadProcessId(hwnd, out var pid);
            var processName = ReadProcessName((int)pid);
            if (processName.Length == 0)
                return true;

            var client = ReadClientBounds(hwnd);
            if (client.IsEmpty)
                return true;

            var title = ReadTitle(hwnd);
            var info = new GameWindowInfo(hwnd, processName, title, ReadWindowBounds(hwnd), client);
            windows.Add((new WindowCandidate(processName, title, client.Width, client.Height, true, false), info));
            return true;
        }, 0);

        return windows;
    }

    private static string ReadProcessName(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.ProcessName;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return string.Empty;
        }
    }

    private static string ReadTitle(nint handle)
    {
        var length = GetWindowTextLength(handle);
        if (length <= 0)
            return string.Empty;

        var buffer = new StringBuilder(length + 1);
        _ = GetWindowText(handle, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static ScreenRect ReadWindowBounds(nint handle)
    {
        if (!GetWindowRect(handle, out var rect))
            return default;
        return new ScreenRect(rect.Left, rect.Top, Math.Max(0, rect.Right - rect.Left), Math.Max(0, rect.Bottom - rect.Top));
    }

    private static ScreenRect ReadClientBounds(nint handle)
    {
        if (!GetClientRect(handle, out var rect))
            return default;

        var origin = new POINT();
        if (!ClientToScreen(handle, ref origin))
            return default;

        return new ScreenRect(origin.X, origin.Y, Math.Max(0, rect.Right - rect.Left), Math.Max(0, rect.Bottom - rect.Top));
    }

    private delegate bool EnumWindowsProc(nint hwnd, nint lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool IsWindow(nint hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool IsIconic(nint hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetClientRect(nint hWnd, out RECT lpRect);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ClientToScreen(nint hWnd, ref POINT lpPoint);

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int GetWindowTextLength(nint hWnd);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
