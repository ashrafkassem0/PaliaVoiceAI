using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class SpeakerIdentifierTests
{
    [Fact]
    public void Reads_the_name_plate_on_a_dialogue_card()
    {
        var raw = "Kenyatta 0 0\nI'd wish you a hoppu Luna New Year, but I'm so\nready for it to be over already.\n(Ctvt with\nEsc";
        Assert.Equal("kenyatta", SpeakerIdentifier.FindNpcId(raw));
    }

    [Fact]
    public void Reads_a_name_on_its_own_line()
    {
        Assert.Equal("eshe", SpeakerIdentifier.FindNpcId("Eshe\nSkip\nWelcome to Kilima.\nPress F to continue"));
        Assert.Equal("naio", SpeakerIdentifier.FindNpcId("Nai'o\nHello there, friend."));
    }

    [Fact]
    public void Reads_Einar_when_friendship_badges_share_the_name_line()
    {
        var raw = "Einar I -\nSometimes I like to go watch the familial\nunit known as Daiya eat dinner at night.";
        Assert.Equal("einar", SpeakerIdentifier.FindNpcId(raw));
        Assert.Equal("einar", SpeakerIdentifier.FindNpcId("Einar • VI -\nSometimes I like to go watch the familial\nunit known as Daiya eat dinner at night."));
    }

    [Fact]
    public void Does_not_select_a_name_that_only_appears_in_the_sentence()
    {
        var raw = "Einar\nSometimes I like to go watch the familial\nunit known as Daiya eat dinner at night.";
        var roster = NpcCatalog.All.Append(new NpcProfile("daiya", "Daiya", "Farmer", "af_bella", "Test"));
        Assert.Equal("einar", SpeakerIdentifier.FindNpcId(raw, roster));
    }

    [Fact]
    public void Does_not_treat_a_mentioned_name_as_the_speaker()
    {
        Assert.Null(SpeakerIdentifier.FindNpcId("I saw Eshe at the inn today."));
        Assert.Null(SpeakerIdentifier.FindNpcId("Welcome to Kilima Village."));
    }

    [Fact]
    public void A_card_without_a_name_is_not_voiced()
    {
        Assert.Null(SpeakerIdentifier.FindNpcId("Sometimes I like to go watch the fish."));
        Assert.False(SpeakerIdentifier.IsNamedSpeaker(null));
        Assert.False(SpeakerIdentifier.IsNamedSpeaker(string.Empty));
        Assert.False(SpeakerIdentifier.IsNamedSpeaker(NpcCatalog.Unknown.Id));
        Assert.True(SpeakerIdentifier.IsNamedSpeaker("einar"));
    }

    [Fact]
    public void Round_trips_a_voice_assignment()
    {
        var path = Path.Combine(Path.GetTempPath(), "PaliaVoiceAI-tests", Guid.NewGuid().ToString("N"), "voices.json");
        var state = new VoiceLibraryState
        {
            SelectedId = "eshe",
            ApplyVoice = false,
            Voices = new Dictionary<string, string> { ["eshe"] = "bf_lily" },
            Paces = new Dictionary<string, double> { ["eshe"] = 0.9 },
            Favorites = ["eshe"]
        };

        VoiceProfileStore.Save(state, path);
        var loaded = VoiceProfileStore.Load(path);
        var eshe = NpcCatalog.Find("eshe")!;

        Assert.Equal("bf_lily", loaded.VoiceFor(eshe));
        Assert.Equal(0.9f, loaded.PaceFor("eshe"));
        Assert.True(loaded.IsFavorite("eshe"));
        Assert.False(loaded.ApplyVoice);
        Assert.Equal("af_bella", loaded.VoiceFor(NpcCatalog.Unknown));
    }

    [Fact]
    public void Loads_the_roster_from_json_without_replacing_voice_ids()
    {
        var kenyatta = NpcCatalog.Find("kenyatta");
        var auni = NpcCatalog.Find("auni");
        var eshe = NpcCatalog.Find("eshe");

        Assert.NotNull(kenyatta);
        Assert.Equal("af_sky", kenyatta!.VoiceId);
        Assert.Equal("Avatars/kenyatta.png", kenyatta.Avatar);
        Assert.Equal("bf_emma", eshe!.VoiceId);
        Assert.NotNull(auni);
        Assert.Equal(string.Empty, auni!.VoiceId);
        Assert.Equal("Auni", auni.Name);
    }
}
