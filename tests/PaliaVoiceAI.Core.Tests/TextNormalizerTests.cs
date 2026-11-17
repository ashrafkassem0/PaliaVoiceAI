using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class TextNormalizerTests
{
    [Fact]
    public void Collapses_whitespace_and_joins_lines()
    {
        var text = TextNormalizer.Normalize("  Hello\r\n\r\nKilima\tVillage  ");
        Assert.Equal("Hello Kilima Village", text);
    }

    [Fact]
    public void Joins_a_hyphenated_line_break()
    {
        Assert.Equal("Welcome to Kilima.", TextNormalizer.Normalize("Wel-\ncome to Kilima."));
    }

    [Fact]
    public void Drops_skip_and_continue_prompts()
    {
        Assert.Equal("Welcome to Kilima.", TextNormalizer.Normalize("Eshe\nSkip\nWelcome to Kilima.\nPress F to continue"));
    }

    [Fact]
    public void Keeps_the_spoken_line_from_a_palia_dialogue_card()
    {
        var raw = "Kenyatta 0 0\nI'd wish you a hoppu Luna New Year, but I'm so\nready for it to be over already.\n(Ctvt with\nEsc";
        Assert.Equal(
            "I'd wish you a hoppu Luna New Year, but I'm so ready for it to be over already.",
            TextNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalizes_curly_quotes()
    {
        Assert.Equal("It's \"home.\"", TextNormalizer.Normalize("It’s “home.”"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("...")]
    public void Blank_input_is_empty(string? raw)
    {
        Assert.Equal(string.Empty, TextNormalizer.Normalize(raw));
    }
}
