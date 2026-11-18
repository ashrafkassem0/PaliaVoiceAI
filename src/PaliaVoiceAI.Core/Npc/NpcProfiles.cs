using System.Text.Json;

namespace PaliaVoiceAI.Core;

public enum NpcGender
{
    Unknown,
    Female,
    Male
}

public enum NpcAgeGroup
{
    Unknown,
    Child,
    Teen,
    YoungAdult,
    Adult,
    Elder
}

public enum NpcSpecies
{
    Unknown,
    Majiri,
    Galdur,
    Grimalkin,
    Plumehound,
    Human,
    Dragon
}

public enum VoiceAssignmentStatus
{
    Unassigned,
    Provisional,
    Confirmed
}

public sealed record NpcProfile(
    string Id,
    string Name,
    string Role,
    string VoiceId,
    string Intro,
    string? Avatar = null,
    float Speed = 1f,
    string Gender = "",
    string AgeGroup = "unknown",
    string Species = "Unknown",
    string Personality = "",
    string VoiceTone = "",
    string DeliveryDirection = "",
    string PitchDirection = "",
    string VoiceSelectionNotes = "",
    string VoiceSelectionConfidence = "",
    string VoiceStatus = "")
{
    public bool HasAssignedVoice => KokoroVoices.IsKnown(VoiceId);

    public NpcGender GenderKind => NpcFacts.ParseGender(Gender);

    public NpcAgeGroup AgeGroupKind => NpcFacts.ParseAgeGroup(AgeGroup);

    public NpcSpecies SpeciesKind => NpcFacts.ParseSpecies(Species);

    public VoiceAssignmentStatus AssignmentStatus => NpcFacts.ParseStatus(VoiceStatus, HasAssignedVoice);

    public string SpeakableVoiceId => HasAssignedVoice ? VoiceId : string.Empty;
}

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
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true
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

            var result = Read(File.ReadAllText(path));
            _all = result.Profiles;
            LoadError = result.Error ?? (result.Problems.Count > 0 ? result.Problems[0] : null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _all = [FallbackUnknown];
            LoadError = ex.Message;
        }
    }

    public static RosterLoad Read(string json)
    {
        try
        {
            var roster = JsonSerializer.Deserialize<RosterFile>(json, Json);
            var profiles = new List<NpcProfile>();
            foreach (var character in roster?.Characters ?? [])
            {
                if (character.Enabled == false || string.IsNullOrWhiteSpace(character.Id) || string.IsNullOrWhiteSpace(character.Name))
                    continue;

                profiles.Add(ToProfile(character));
            }

            if (profiles.All(profile => !string.Equals(profile.Id, "unknown", StringComparison.OrdinalIgnoreCase)))
                profiles.Insert(0, FallbackUnknown);

            return new RosterLoad(profiles, null, NpcRosterRules.Problems(profiles));
        }
        catch (JsonException ex)
        {
            return new RosterLoad([FallbackUnknown], ex.Message, []);
        }
    }

    public static string ToJson(IEnumerable<NpcProfile> profiles)
    {
        var roster = new RosterFile
        {
            Characters = profiles.Select(ToCharacter).ToList()
        };
        return JsonSerializer.Serialize(roster, Json);
    }

    private static NpcProfile ToProfile(RosterCharacter character)
    {
        var voiceId = character.VoiceId?.Trim() ?? string.Empty;
        var name = character.Name.Trim();
        var gender = NpcFacts.CanonicalGender(character.Gender);
        var ageGroup = NpcFacts.CanonicalAgeGroup(character.AgeGroup);
        var species = NpcFacts.CanonicalSpecies(character.Species);
        var status = NpcFacts.CanonicalStatus(character.VoiceStatus, KokoroVoices.IsKnown(voiceId));
        return new NpcProfile(
            character.Id.Trim(),
            name,
            string.IsNullOrWhiteSpace(character.Role) ? name : character.Role.Trim(),
            voiceId,
            string.IsNullOrWhiteSpace(character.Intro)
                ? string.IsNullOrWhiteSpace(voiceId)
                    ? "No Kokoro voice is assigned yet. Set voiceId in npc-voices.json."
                    : $"This voice is assigned to {name}."
                : character.Intro.Trim(),
            string.IsNullOrWhiteSpace(character.Avatar) ? null : character.Avatar.Trim().Replace('\\', '/'),
            (float)Math.Clamp(character.Speed <= 0 ? 1 : character.Speed, 0.85, 1.15),
            gender,
            ageGroup,
            species,
            character.Personality?.Trim() ?? string.Empty,
            character.VoiceTone?.Trim() ?? string.Empty,
            character.DeliveryDirection?.Trim() ?? string.Empty,
            character.PitchDirection?.Trim() ?? string.Empty,
            character.VoiceSelectionNotes?.Trim() ?? string.Empty,
            character.VoiceSelectionConfidence?.Trim() ?? string.Empty,
            status);
    }

    private static RosterCharacter ToCharacter(NpcProfile profile) => new()
    {
        Id = profile.Id,
        Name = profile.Name,
        Role = profile.Role,
        Gender = profile.Gender,
        VoiceId = profile.VoiceId,
        Speed = profile.Speed,
        Avatar = profile.Avatar ?? string.Empty,
        Intro = profile.Intro,
        AgeGroup = profile.AgeGroup,
        Species = profile.Species,
        Personality = profile.Personality,
        VoiceTone = profile.VoiceTone,
        DeliveryDirection = profile.DeliveryDirection,
        PitchDirection = profile.PitchDirection,
        VoiceSelectionNotes = profile.VoiceSelectionNotes,
        VoiceSelectionConfidence = profile.VoiceSelectionConfidence,
        VoiceStatus = profile.VoiceStatus,
        Enabled = true
    };

    private sealed class RosterFile
    {
        public int Version { get; set; } = 1;
        public List<RosterCharacter> Characters { get; set; } = [];
    }

    private sealed class RosterCharacter
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? AgeGroup { get; set; }
        public string? Species { get; set; }
        public string? Personality { get; set; }
        public string? VoiceId { get; set; }
        public double Speed { get; set; } = 1;
        public string? Avatar { get; set; }
        public string? Intro { get; set; }
        public string? VoiceTone { get; set; }
        public string? DeliveryDirection { get; set; }
        public string? PitchDirection { get; set; }
        public string? VoiceSelectionNotes { get; set; }
        public string? VoiceSelectionConfidence { get; set; }
        public string? VoiceStatus { get; set; }
        public bool Enabled { get; set; } = true;
    }
}

public readonly record struct RosterLoad(
    IReadOnlyList<NpcProfile> Profiles,
    string? Error,
    IReadOnlyList<string> Problems);

public static class NpcFacts
{
    public static NpcGender ParseGender(string? value) => CanonicalGender(value) switch
    {
        "female" => NpcGender.Female,
        "male" => NpcGender.Male,
        _ => NpcGender.Unknown
    };

    public static NpcAgeGroup ParseAgeGroup(string? value) => CanonicalAgeGroup(value) switch
    {
        "child" => NpcAgeGroup.Child,
        "teen" => NpcAgeGroup.Teen,
        "youngAdult" => NpcAgeGroup.YoungAdult,
        "adult" => NpcAgeGroup.Adult,
        "elder" => NpcAgeGroup.Elder,
        _ => NpcAgeGroup.Unknown
    };

    public static NpcSpecies ParseSpecies(string? value) => CanonicalSpecies(value) switch
    {
        "Majiri" => NpcSpecies.Majiri,
        "Galdur" => NpcSpecies.Galdur,
        "Grimalkin" => NpcSpecies.Grimalkin,
        "Plumehound" => NpcSpecies.Plumehound,
        "Human" => NpcSpecies.Human,
        "Dragon" => NpcSpecies.Dragon,
        _ => NpcSpecies.Unknown
    };

    public static VoiceAssignmentStatus ParseStatus(string? value, bool hasAssignedVoice) => CanonicalStatus(value, hasAssignedVoice) switch
    {
        "confirmed" => VoiceAssignmentStatus.Confirmed,
        "provisional" => VoiceAssignmentStatus.Provisional,
        _ => VoiceAssignmentStatus.Unassigned
    };

    public static string CanonicalGender(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "female" or "f" => "female",
        "male" or "m" => "male",
        _ => "unknown"
    };

    public static string CanonicalAgeGroup(string? value)
    {
        var token = Compact(value);
        return token switch
        {
            "child" => "child",
            "teen" or "teenager" => "teen",
            "youngadult" or "youngteen" => "youngAdult",
            "adult" or "matureadult" or "olderadult" => "adult",
            "elder" => "elder",
            _ => "unknown"
        };
    }

    public static string CanonicalSpecies(string? value)
    {
        var token = Compact(value);
        return token switch
        {
            "majiri" => "Majiri",
            "galdur" => "Galdur",
            "grimalkin" => "Grimalkin",
            "plumehound" => "Plumehound",
            "human" => "Human",
            "dragon" => "Dragon",
            _ => "Unknown"
        };
    }

    public static string CanonicalStatus(string? value, bool hasAssignedVoice)
    {
        var token = Compact(value);
        if (token is "confirmed" or "provisional" or "unassigned")
            return token;
        return hasAssignedVoice ? "provisional" : "unassigned";
    }

    private static string Compact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var buffer = new System.Text.StringBuilder(value.Length);
        foreach (var ch in value.Trim())
        {
            if (char.IsLetterOrDigit(ch))
                buffer.Append(char.ToLowerInvariant(ch));
        }

        return buffer.ToString();
    }
}

public static class NpcRosterRules
{
    public static IReadOnlyList<string> Problems(IEnumerable<NpcProfile> profiles)
    {
        var problems = new List<string>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var profile in profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Id) || string.IsNullOrWhiteSpace(profile.Name))
            {
                problems.Add("A profile is missing an id or a name.");
                continue;
            }

            if (!ids.Add(profile.Id))
                problems.Add($"Duplicate id '{profile.Id}'.");

            if (!string.IsNullOrWhiteSpace(profile.VoiceId) && !KokoroVoices.IsKnown(profile.VoiceId))
                problems.Add($"{profile.Id} voice '{profile.VoiceId}' is not a Kokoro voice in this app.");

            if (profile.AssignmentStatus == VoiceAssignmentStatus.Unassigned && profile.HasAssignedVoice)
                problems.Add($"{profile.Id} is marked unassigned but has voice '{profile.VoiceId}'.");

            if (profile.AssignmentStatus != VoiceAssignmentStatus.Unassigned && !profile.HasAssignedVoice)
                problems.Add($"{profile.Id} is marked {profile.VoiceStatus} but has no supported voice.");
        }

        return problems;
    }
}

public static class KokoroVoices
{
    public static IReadOnlyList<KokoroVoiceChoice> English { get; } =
    [
        new("af_aoede", "Aoede", "American woman"),
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

    public static IReadOnlyList<KokoroVoiceChoice> Assignable { get; } =
    [
        new("", "Unassigned", "None"),
        ..English
    ];

    public static string Label(string? voiceId)
    {
        if (string.IsNullOrWhiteSpace(voiceId))
            return "Unassigned";
        return English.FirstOrDefault(voice => string.Equals(voice.Id, voiceId, StringComparison.OrdinalIgnoreCase))?.Label
            ?? voiceId;
    }

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

    public string VoiceFor(NpcProfile profile)
    {
        if (Voices.TryGetValue(profile.Id, out var voice))
        {
            if (string.IsNullOrWhiteSpace(voice))
                return string.Empty;
            if (KokoroVoices.IsKnown(voice))
                return voice;
        }

        return profile.SpeakableVoiceId;
    }

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
