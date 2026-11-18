using System.Text.Json;

namespace PaliaVoiceAI.Core;

public sealed record NpcProfile(
    string Id,
    string Name,
    string Role,
    string VoiceId,
    string Intro,
    string? Avatar = null,
    float Speed = 1f,
    string Gender = "");

public sealed record KokoroVoiceChoice(string Id, string Label, string Group)
{
    public override string ToString() => Label;
}

public static class NpcCatalog
{
    private static readonly NpcProfile FallbackUnknown = new(
        "unknown",
        "Unknown",
        "Fallback when the name is missing",
        "af_bella",
        "I can't tell who is speaking, so this fallback voice will read the line.");

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static IReadOnlyList<NpcProfile>? _all;

    public static IReadOnlyList<NpcProfile> All
    {
        get
        {
            if (_all is null)
                Load(Path.Combine(AppContext.BaseDirectory, "npc-voices.json"));
            return _all!;
        }
    }

    public static string? LoadError { get; private set; }

    public static NpcProfile Unknown =>
        All.FirstOrDefault(profile => string.Equals(profile.Id, "unknown", StringComparison.OrdinalIgnoreCase))
        ?? FallbackUnknown;

    public static NpcProfile? Find(string? id) =>
        All.FirstOrDefault(profile => string.Equals(profile.Id, id, StringComparison.OrdinalIgnoreCase));

    public static void Load(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                _all = [FallbackUnknown];
                LoadError = $"npc-voices.json was not found at {path}.";
                return;
            }

            var roster = JsonSerializer.Deserialize<RosterFile>(File.ReadAllText(path), Json);
            var profiles = new List<NpcProfile>();
            foreach (var character in roster?.Characters ?? [])
            {
                if (character.Enabled == false || string.IsNullOrWhiteSpace(character.Id) || string.IsNullOrWhiteSpace(character.Name))
                    continue;

                var voiceId = character.VoiceId?.Trim() ?? string.Empty;
                var name = character.Name.Trim();
                profiles.Add(new NpcProfile(
                    character.Id.Trim(),
                    name,
                    string.IsNullOrWhiteSpace(character.Role) ? name : character.Role.Trim(),
                    voiceId,
                    string.IsNullOrWhiteSpace(character.Intro)
                        ? string.IsNullOrWhiteSpace(voiceId)
                            ? $"No Kokoro voice is assigned yet. Set voiceId in npc-voices.json."
                            : $"This voice is assigned to {name}."
                        : character.Intro.Trim(),
                    string.IsNullOrWhiteSpace(character.Avatar) ? null : character.Avatar.Trim().Replace('\\', '/'),
                    (float)Math.Clamp(character.Speed <= 0 ? 1 : character.Speed, 0.85, 1.15),
                    character.Gender?.Trim().ToLowerInvariant() ?? string.Empty));
            }

            if (profiles.All(profile => !string.Equals(profile.Id, "unknown", StringComparison.OrdinalIgnoreCase)))
                profiles.Insert(0, FallbackUnknown);

            _all = profiles;
            LoadError = null;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            _all = [FallbackUnknown];
            LoadError = ex.Message;
        }
    }

    private sealed class RosterFile
    {
        public List<RosterCharacter> Characters { get; set; } = [];
    }

    private sealed class RosterCharacter
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? VoiceId { get; set; }
        public double Speed { get; set; } = 1;
        public string? Avatar { get; set; }
        public string? Intro { get; set; }
        public bool Enabled { get; set; } = true;
    }
}

public static class KokoroVoices
{
    public static IReadOnlyList<KokoroVoiceChoice> English { get; } =
    [
        new("af_bella", "Bella", "American woman"),
        new("af_heart", "Heart", "American woman"),
        new("af_jessica", "Jessica", "American woman"),
        new("af_kore", "Kore", "American woman"),
        new("af_nicole", "Nicole", "American woman"),
        new("af_nova", "Nova", "American woman"),
        new("af_sarah", "Sarah", "American woman"),
        new("af_sky", "Sky", "American woman"),
        new("bf_alice", "Alice", "British woman"),
        new("bf_emma", "Emma", "British woman"),
        new("bf_isabella", "Isabella", "British woman"),
        new("bf_lily", "Lily", "British woman"),
        new("am_adam", "Adam", "American man"),
        new("am_echo", "Echo", "American man"),
        new("am_eric", "Eric", "American man"),
        new("am_fenrir", "Fenrir", "American man"),
        new("am_liam", "Liam", "American man"),
        new("am_michael", "Michael", "American man"),
        new("am_onyx", "Onyx", "American man"),
        new("am_puck", "Puck", "American man"),
        new("bm_daniel", "Daniel", "British man"),
        new("bm_george", "George", "British man"),
        new("bm_lewis", "Lewis", "British man")
    ];

    public static string Label(string? voiceId) =>
        English.FirstOrDefault(voice => string.Equals(voice.Id, voiceId, StringComparison.OrdinalIgnoreCase))?.Label
        ?? voiceId
        ?? string.Empty;

    public static bool IsKnown(string? voiceId) =>
        English.Any(voice => string.Equals(voice.Id, voiceId, StringComparison.OrdinalIgnoreCase));
}

public static class SpeakerIdentifier
{
    public static string? FindNpcId(string? raw, IEnumerable<NpcProfile>? npcs = null)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var named = (npcs ?? NpcCatalog.All)
            .Where(profile => !string.Equals(profile.Id, NpcCatalog.Unknown.Id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(profile => profile.Name.Length)
            .ToArray();

        foreach (var line in Lines(raw))
        {
            var match = MatchLine(line, named);
            if (match is not null)
                return match;
            if (LooksSpoken(line))
                break;
        }

        return null;
    }

    public static bool IsNamedSpeaker(string? npcId) =>
        !string.IsNullOrWhiteSpace(npcId) &&
        !string.Equals(npcId, NpcCatalog.Unknown.Id, StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> Lines(string raw)
    {
        var normalized = raw.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        foreach (var line in normalized.Split('\n'))
        {
            var cleaned = Collapse(line);
            if (cleaned.Length > 0)
                yield return cleaned;
        }
    }

    private static string? MatchLine(string line, IReadOnlyList<NpcProfile> npcs)
    {
        var plate = NamePlate.TrimBadge(line);
        foreach (var npc in npcs)
        {
            if (plate.Equals(npc.Name, StringComparison.OrdinalIgnoreCase))
                return npc.Id;
            if (NamePlate.HasBadgeSuffix(line, npc.Name))
                return npc.Id;
        }

        return null;
    }

    private static bool LooksSpoken(string line)
    {
        if (line.Contains('.') || line.Contains('?') || line.Contains('!') || line.Contains(','))
            return true;

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

        return words >= 4;
    }

    private static string Collapse(string value)
    {
        var buffer = new System.Text.StringBuilder(value.Length);
        var pending = false;
        foreach (var ch in value.Trim())
        {
            if (char.IsWhiteSpace(ch))
            {
                pending = buffer.Length > 0;
                continue;
            }

            if (pending)
                buffer.Append(' ');
            pending = false;
            buffer.Append(ch);
        }

        return buffer.ToString();
    }
}

internal static class NamePlate
{
    public static string TrimBadge(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var end = parts.Length;
        while (end > 0 && IsBadgeToken(parts[end - 1]))
            end--;
        return string.Join(' ', parts[..end]);
    }

    public static bool HasBadgeSuffix(string line, string name)
    {
        if (name.Length == 0 || line.Length <= name.Length)
            return false;
        if (!line.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!char.IsWhiteSpace(line[name.Length]))
            return false;
        return TrimBadge(line).Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBadgeToken(string token)
    {
        if (token.All(char.IsDigit))
            return true;
        if (token.Length == 1 && char.IsLetter(token[0]))
            return true;
        if (token.All(ch => char.IsPunctuation(ch) || char.IsSymbol(ch)))
            return true;
        if (token.Length is 0 or > 4)
            return false;
        foreach (var ch in token)
        {
            if (ch is not ('I' or 'V' or 'X'))
                return false;
        }

        return true;
    }
}

public sealed class VoiceLibraryState
{
    public Dictionary<string, string> Voices { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Paces { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Favorites { get; set; } = [];
    public string SelectedId { get; set; } = "kenyatta";
    public bool ApplyVoice { get; set; } = true;
    public bool HearYourself { get; set; } = true;

    public string VoiceFor(NpcProfile profile) =>
        Voices.TryGetValue(profile.Id, out var voice) && KokoroVoices.IsKnown(voice) ? voice : profile.VoiceId;

    public float PaceFor(string npcId, float fallback = 1f)
    {
        if (!Paces.TryGetValue(npcId, out var pace))
            return (float)Math.Clamp(fallback, 0.85, 1.15);
        return (float)Math.Clamp(pace, 0.85, 1.15);
    }

    public bool IsFavorite(string npcId) =>
        Favorites.Any(id => string.Equals(id, npcId, StringComparison.OrdinalIgnoreCase));
}

public static class VoiceProfileStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static VoiceLibraryState Load(string? path = null)
    {
        path ??= AppPaths.VoiceProfiles;
        try
        {
            if (!File.Exists(path))
                return new VoiceLibraryState();
            return JsonSerializer.Deserialize<VoiceLibraryState>(File.ReadAllText(path), Json) ?? new VoiceLibraryState();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new VoiceLibraryState();
        }
    }

    public static void Save(VoiceLibraryState state, string? path = null)
    {
        path ??= AppPaths.VoiceProfiles;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(state, Json));
    }
}
