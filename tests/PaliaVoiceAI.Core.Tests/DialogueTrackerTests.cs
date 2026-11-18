using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class DialogueTrackerTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Waits_until_the_line_is_stable()
    {
        var tracker = new DialogueTracker(2, TimeSpan.FromSeconds(8));

        Assert.Equal(DialogueSignal.None, tracker.Observe("Hello there.", Start).Signal);
        Assert.Equal(DialogueSignal.None, tracker.Observe("Goodbye.", Start.AddMilliseconds(200)).Signal);

        var started = tracker.Observe("Hello there.", Start.AddMilliseconds(400));
        Assert.Equal(DialogueSignal.None, started.Signal);
        var accepted = tracker.Observe("Hello there.", Start.AddMilliseconds(600));
        Assert.Equal(DialogueSignal.Started, accepted.Signal);
        Assert.Equal("Hello there.", accepted.Text);
        Assert.Equal(DialogueSignal.None, tracker.Observe("Hello there.", Start.AddMilliseconds(800)).Signal);
    }

    [Fact]
    public void Reports_a_change_then_the_end_and_suppresses_a_repeat()
    {
        var tracker = new DialogueTracker(1, TimeSpan.FromSeconds(8));

        Assert.Equal(DialogueSignal.Started, tracker.Observe("Hello there.", Start).Signal);
        Assert.Equal(DialogueSignal.None, tracker.Observe("Hello there!", Start.AddSeconds(1)).Signal);
        Assert.Equal(DialogueSignal.Changed, tracker.Observe("Come back tomorrow.", Start.AddSeconds(2)).Signal);
        Assert.Equal(DialogueSignal.Ended, tracker.Observe(" ", Start.AddSeconds(3)).Signal);
        Assert.Equal(DialogueSignal.DuplicateSuppressed, tracker.Observe("Come back tomorrow.", Start.AddSeconds(4)).Signal);
        Assert.Equal(DialogueSignal.None, tracker.Observe("Come back tomorrow!", Start.AddSeconds(5)).Signal);
        Assert.Equal(DialogueSignal.Ended, tracker.Observe(string.Empty, Start.AddSeconds(6)).Signal);
        Assert.Equal(DialogueSignal.Started, tracker.Observe("Come back tomorrow.", Start.AddSeconds(20)).Signal);
    }

    [Fact]
    public void One_blank_frame_does_not_end_a_stable_line()
    {
        var tracker = new DialogueTracker(2, TimeSpan.FromSeconds(8));
        tracker.Observe("Hello there.", Start);
        Assert.Equal(DialogueSignal.Started, tracker.Observe("Hello there.", Start.AddMilliseconds(100)).Signal);
        Assert.Equal(DialogueSignal.None, tracker.Observe(string.Empty, Start.AddMilliseconds(200)).Signal);
        Assert.Equal(DialogueSignal.Ended, tracker.Observe(string.Empty, Start.AddMilliseconds(300)).Signal);
    }

    [Fact]
    public void Prepares_a_finished_line_before_the_confirming_read()
    {
        var tracker = new DialogueTracker(2, TimeSpan.FromSeconds(8));

        var first = tracker.Observe("Hello there.", Start);
        Assert.Equal(DialogueSignal.None, first.Signal);
        Assert.True(first.PrepareSpeech);

        var ready = tracker.Observe("Hello there.", Start.AddMilliseconds(400));
        Assert.Equal(DialogueSignal.Started, ready.Signal);
        Assert.False(ready.PrepareSpeech);

        var growing = new DialogueTracker(2, TimeSpan.FromSeconds(8)).Observe("Hello", Start);
        Assert.False(growing.PrepareSpeech);
    }
}
