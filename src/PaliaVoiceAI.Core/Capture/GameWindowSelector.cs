namespace PaliaVoiceAI.Core;

public static class GameWindowSelector
{
    public const int MinimumClientWidth = 640;
    public const int MinimumClientHeight = 480;

    public static readonly string[] PreferredProcessNames =
    [
        "PaliaClientSteam-Win64-Shipping",
        "PaliaClient-Win64-Shipping",
        "PaliaClientSteam-Win64-Test",
        "PaliaClient-Win64-Test",
        "PaliaClientSteam",
        "Palia"
    ];

    public static int? SelectIndex(IReadOnlyList<WindowCandidate> windows)
    {
        int? best = null;
        var bestRank = int.MaxValue;
        var bestArea = -1;

        for (var i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (!window.IsVisible || window.IsMinimized)
                continue;
            if (window.ClientWidth < MinimumClientWidth || window.ClientHeight < MinimumClientHeight)
                continue;

            var rank = Rank(window);
            if (rank < 0)
                continue;

            var area = window.ClientWidth * window.ClientHeight;
            if (rank < bestRank || (rank == bestRank && area > bestArea))
            {
                best = i;
                bestRank = rank;
                bestArea = area;
            }
        }

        return best;
    }

    public static bool IsPreferredProcess(string processName)
    {
        var name = StripExe(processName);
        return PreferredProcessNames.Any(candidate =>
            string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase));
    }

    private static int Rank(WindowCandidate window)
    {
        var name = StripExe(window.ProcessName);
        for (var i = 0; i < PreferredProcessNames.Length; i++)
        {
            if (string.Equals(PreferredProcessNames[i], name, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        if (string.Equals(window.Title, "Palia", StringComparison.OrdinalIgnoreCase))
            return PreferredProcessNames.Length;

        return -1;
    }

    private static string StripExe(string processName)
    {
        return processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? processName[..^4]
            : processName;
    }
}
