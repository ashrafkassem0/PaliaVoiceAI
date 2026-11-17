using System.Text;
using System.Text.RegularExpressions;

namespace PaliaVoiceAI.Core;

public static partial class TextNormalizer
{
    private static readonly HashSet<string> UiLines = new(StringComparer.OrdinalIgnoreCase)
    {
        "skip", "continue", "close", "esc", "back", "exit"
    };

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var text = raw
            .Replace('\u2019', '\'')
            .Replace('\u2018', '\'')
            .Replace('\u201C', '"')
            .Replace('\u201D', '"')
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

        var lines = text.Split('\n');
        var kept = new List<string>(lines.Length);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = CleanLine(lines[i]);
            if (line.Length == 0)
                continue;

            if (line.Length >= 2 && line[^1] == '-' && char.IsLetter(line[^2]) && i + 1 < lines.Length)
            {
                var next = CleanLine(lines[i + 1]);
                if (next.Length > 0 && char.IsLetter(next[0]))
                {
                    line = string.Concat(line.AsSpan(0, line.Length - 1), next);
                    i++;
                }
            }

            if (IsUiChrome(line) || IsPrompt(line) || IsOnlyNumbers(line) || !ContainsLetterOrDigit(line) || IsBadgePlate(line))
                continue;

            kept.Add(line);
        }

        var sentenceIndex = kept.FindIndex(IsSentence);
        if (sentenceIndex > 0)
            kept.RemoveRange(0, CountLeadingSpeakerPlates(kept, sentenceIndex));

        return CollapseWhitespace(string.Join(' ', kept));
    }

    private static int CountLeadingSpeakerPlates(List<string> lines, int sentenceIndex)
    {
        var count = 0;
        while (count < sentenceIndex && IsSpeakerPlate(lines[count]))
            count++;
        return count;
    }

    public static string Key(string normalized)
    {
        var buffer = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
                buffer.Append(char.ToLowerInvariant(ch));
        }

        return buffer.ToString();
    }

    private static string CleanLine(string line)
    {
        var buffer = new StringBuilder(line.Length);
        foreach (var ch in line.Trim())
        {
            if (ch is '\u200B' or '\u200C' or '\u200D' or '\uFEFF')
                continue;
            if (char.IsWhiteSpace(ch))
            {
                buffer.Append(' ');
                continue;
            }
            if (char.IsControl(ch))
                continue;
            buffer.Append(ch);
        }

        return CollapseWhitespace(buffer.ToString());
    }

    private static string CollapseWhitespace(string value)
    {
        var buffer = new StringBuilder(value.Length);
        var pendingSpace = false;
        foreach (var ch in value.Trim())
        {
            if (char.IsWhiteSpace(ch))
            {
                pendingSpace = buffer.Length > 0;
                continue;
            }

            if (pendingSpace)
                buffer.Append(' ');
            pendingSpace = false;
            buffer.Append(ch);
        }

        return buffer.ToString();
    }

    private static bool ContainsLetterOrDigit(string value)
    {
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
                return true;
        }

        return false;
    }

    private static bool IsSentence(string line)
    {
        return line.Contains('.') || line.Contains('?') || line.Contains('!') || line.Contains(',') || WordCount(line) >= 4;
    }

    private static bool IsSpeakerPlate(string line)
    {
        var trimmed = NamePlate.TrimBadge(line);
        return trimmed.Length > 0 && SpeakerPlate().IsMatch(trimmed);
    }

    private static bool IsBadgePlate(string line)
    {
        var trimmed = NamePlate.TrimBadge(line);
        return trimmed.Length > 0 && trimmed.Length < line.Length && SpeakerPlate().IsMatch(trimmed);
    }

    private static bool IsPrompt(string line)
    {
        return line.StartsWith('(') || line.StartsWith('[') ||
            line.StartsWith("chat with ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOnlyNumbers(string line)
    {
        var anyDigit = false;
        foreach (var ch in line)
        {
            if (char.IsWhiteSpace(ch))
                continue;
            if (!char.IsDigit(ch))
                return false;
            anyDigit = true;
        }

        return anyDigit;
    }

    private static int WordCount(string line)
    {
        var words = 0;
        var inWord = false;
        foreach (var ch in line)
        {
            if (char.IsWhiteSpace(ch))
            {
                inWord = false;
                continue;
            }

            if (!inWord)
            {
                words++;
                inWord = true;
            }
        }

        return words;
    }

    private static bool IsUiChrome(string line)
    {
        return UiLines.Contains(line) ||
            line.StartsWith("press ", StringComparison.OrdinalIgnoreCase) ||
            PressToContinue().IsMatch(line);
    }

    [GeneratedRegex(@"^press\s+\S+\s+to\s+(continue|skip|close)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PressToContinue();

    [GeneratedRegex(@"^(?:[A-Z][A-Za-z'\-]*)(?:\s+[A-Z][A-Za-z'\-]*){0,2}(?:\s+(?:\d{1,3}|[A-Za-z]|-+))*$", RegexOptions.CultureInvariant)]
    private static partial Regex SpeakerPlate();
}
