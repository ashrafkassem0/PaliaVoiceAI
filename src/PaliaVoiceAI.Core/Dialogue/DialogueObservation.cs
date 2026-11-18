namespace PaliaVoiceAI.Core;

public enum DialogueSignal
{
    None,
    Started,
    Changed,
    Ended,
    DuplicateSuppressed
}

public sealed record DialogueObservation(DialogueSignal Signal, string Text, bool PrepareSpeech = false)
{
    public bool HasNewDialogue => Signal is DialogueSignal.Started or DialogueSignal.Changed;
}
