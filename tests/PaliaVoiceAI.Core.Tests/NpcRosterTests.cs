using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class NpcRosterTests
{
    private static readonly string[] UnassignedIds = ["auni", "eshelon", "tau", "taylin", "gardener", "ulfe"];

    [Fact]
    public void Shipped_roster_has_unique_villagers_and_only_supported_voices()
    {
        var profiles = NpcCatalog.All;
        var problems = NpcRosterRules.Problems(profiles);
        Assert.Empty(problems);

        var villagers = profiles.Where(profile => !string.Equals(profile.Id, "unknown", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.Equal(29, villagers.Count);
        Assert.Equal(villagers.Count, villagers.Select(profile => profile.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(villagers, profile => Assert.False(string.IsNullOrWhiteSpace(profile.Name)));

        foreach (var id in UnassignedIds)
        {
            var profile = Assert.Single(villagers, item => item.Id == id);
            Assert.Equal(string.Empty, profile.VoiceId);
            Assert.Equal(string.Empty, profile.SpeakableVoiceId);
            Assert.Equal(VoiceAssignmentStatus.Unassigned, profile.AssignmentStatus);
            Assert.NotEqual("af_bella", new VoiceLibraryState().VoiceFor(profile));
        }

        var assigned = villagers.Where(profile => !UnassignedIds.Contains(profile.Id)).ToList();
        Assert.Equal(23, assigned.Count);
        Assert.All(assigned, profile =>
        {
            Assert.True(KokoroVoices.IsKnown(profile.VoiceId));
            Assert.Equal(VoiceAssignmentStatus.Provisional, profile.AssignmentStatus);
            Assert.Equal(1f, profile.Speed);
        });
        Assert.Equal(assigned.Count, assigned.Select(profile => profile.VoiceId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.DoesNotContain(villagers, profile => profile.VoiceId == "af_aoede");
        Assert.True(KokoroVoices.IsKnown("af_aoede"));
        Assert.Equal("af_bella", NpcCatalog.Unknown.VoiceId);
    }

    [Fact]
    public void Missing_metadata_does_not_invent_a_voice()
    {
        const string json = """
            {
              "characters": [
                { "id": "sample", "name": "Sample", "voiceId": "" }
              ]
            }
            """;

        var result = NpcCatalog.Read(json);
        Assert.Null(result.Error);
        var sample = Assert.Single(result.Profiles, profile => profile.Id == "sample");
        Assert.Equal(string.Empty, sample.VoiceId);
        Assert.Equal("unknown", sample.Gender);
        Assert.Equal(NpcAgeGroup.Unknown, sample.AgeGroupKind);
        Assert.Equal(NpcSpecies.Unknown, sample.SpeciesKind);
        Assert.Equal(string.Empty, sample.Personality);
        Assert.Equal(VoiceAssignmentStatus.Unassigned, sample.AssignmentStatus);
        Assert.Equal(1f, sample.Speed);
        Assert.DoesNotContain(result.Problems, problem => problem.Contains("sample", StringComparison.Ordinal));
    }

    [Fact]
    public void Invalid_voice_ids_are_rejected_instead_of_replaced()
    {
        const string json = """
            {
              "characters": [
                { "id": "sample", "name": "Sample", "voiceId": "af_not_real", "voiceStatus": "provisional" }
              ]
            }
            """;

        var result = NpcCatalog.Read(json);
        var sample = Assert.Single(result.Profiles, profile => profile.Id == "sample");
        Assert.Equal("af_not_real", sample.VoiceId);
        Assert.Equal(string.Empty, sample.SpeakableVoiceId);
        Assert.Contains(result.Problems, problem => problem.Contains("af_not_real", StringComparison.Ordinal));
        Assert.Contains(result.Problems, problem => problem.Contains("no supported voice", StringComparison.Ordinal));
        Assert.Equal(string.Empty, new VoiceLibraryState().VoiceFor(sample));
    }

    [Fact]
    public void Metadata_round_trips_and_a_blank_override_stays_unassigned()
    {
        var original = NpcCatalog.Find("kenyatta");
        Assert.NotNull(original);
        var json = NpcCatalog.ToJson(NpcCatalog.All);
        var result = NpcCatalog.Read(json);
        Assert.Null(result.Error);
        Assert.Empty(result.Problems);
        var loaded = Assert.Single(result.Profiles, profile => profile.Id == "kenyatta");
        Assert.Equal(original!.VoiceId, loaded.VoiceId);
        Assert.Equal(original.Gender, loaded.Gender);
        Assert.Equal(original.AgeGroup, loaded.AgeGroup);
        Assert.Equal(original.Species, loaded.Species);
        Assert.Equal(original.Personality, loaded.Personality);
        Assert.Equal(original.VoiceTone, loaded.VoiceTone);
        Assert.Equal(original.DeliveryDirection, loaded.DeliveryDirection);
        Assert.Equal(original.PitchDirection, loaded.PitchDirection);
        Assert.Equal(original.VoiceSelectionNotes, loaded.VoiceSelectionNotes);
        Assert.Equal(original.VoiceSelectionConfidence, loaded.VoiceSelectionConfidence);
        Assert.Equal(original.VoiceStatus, loaded.VoiceStatus);
        Assert.Equal(original.Avatar, loaded.Avatar);
        Assert.Equal(original.Speed, loaded.Speed);

        var library = new VoiceLibraryState
        {
            Voices = new Dictionary<string, string> { ["kenyatta"] = "" }
        };
        Assert.Equal(string.Empty, library.VoiceFor(loaded));
        library.Voices["kenyatta"] = "not-a-voice";
        Assert.Equal("af_sky", library.VoiceFor(loaded));
    }

    [Fact]
    public void Voice_list_keeps_unassigned_separate_from_studio_voices()
    {
        Assert.Equal(string.Empty, KokoroVoices.Assignable[0].Id);
        Assert.Equal("Unassigned", KokoroVoices.Label(""));
        Assert.Equal("Aoede", KokoroVoices.Label("af_aoede"));
        Assert.DoesNotContain(KokoroVoices.English, voice => string.IsNullOrEmpty(voice.Id));
    }

    [Fact]
    public void Cache_identity_changes_when_voice_or_pace_changes()
    {
        var line = SpeechIdentity.Create("Hello there.", "am_eric", 1f, "abc", "KokoroSharp 0.8.4");
        var otherVoice = SpeechIdentity.Create("Hello there.", "af_sky", 1f, "abc", "KokoroSharp 0.8.4");
        var otherPace = SpeechIdentity.Create("Hello there.", "am_eric", 1.1f, "abc", "KokoroSharp 0.8.4");
        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(otherVoice));
        Assert.NotEqual(SpeechCacheKey.Create(line), SpeechCacheKey.Create(otherPace));
    }

    [Fact]
    public void Duplicate_ids_are_reported()
    {
        const string json = """
            {
              "characters": [
                { "id": "sample", "name": "Sample", "voiceId": "af_bella" },
                { "id": "sample", "name": "Sample again", "voiceId": "af_sky" }
              ]
            }
            """;

        var result = NpcCatalog.Read(json);
        Assert.Contains(result.Problems, problem => problem.Contains("Duplicate id 'sample'", StringComparison.Ordinal));
    }
}
