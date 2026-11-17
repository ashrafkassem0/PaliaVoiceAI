namespace PaliaVoiceAI.Core;

public sealed class DialogueTracker
{
    public const int MinimumTextLength = 2;

    private readonly int _stableReads;
    private readonly TimeSpan _duplicateWindow;
    private string? _candidateKey;
    private string _candidateDisplay = string.Empty;
    private int _candidateCount;
    private string? _activeKey;
    private string? _lastAcceptedKey;
    private DateTimeOffset _lastAcceptedAt;
    private int _blankCount;
    private bool _duplicateNotified;

    public DialogueTracker(int stableReads, TimeSpan duplicateWindow)
    {
        if (stableReads < 1)
            throw new ArgumentOutOfRangeException(nameof(stableReads), "Stable reads must be at least 1.");
        if (duplicateWindow < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duplicateWindow));

        _stableReads = stableReads;
        _duplicateWindow = duplicateWindow;
    }

    public DialogueObservation Observe(string? rawText, DateTimeOffset now)
    {
        var display = TextNormalizer.Normalize(rawText);
        var key = TextNormalizer.Key(display);
        if (key.Length < MinimumTextLength)
            return ObserveBlank();

        _blankCount = 0;
        if (string.Equals(key, _candidateKey, StringComparison.Ordinal))
        {
            _candidateCount++;
        }
        else
        {
            _candidateKey = key;
            _candidateDisplay = display;
            _candidateCount = 1;
        }

        if (_candidateCount < _stableReads)
            return new DialogueObservation(DialogueSignal.None, display);

        if (string.Equals(key, _activeKey, StringComparison.Ordinal))
            return new DialogueObservation(DialogueSignal.None, _candidateDisplay);

        if (string.Equals(key, _lastAcceptedKey, StringComparison.Ordinal) && now - _lastAcceptedAt < _duplicateWindow)
        {
            _activeKey = key;
            if (_duplicateNotified)
                return new DialogueObservation(DialogueSignal.None, _candidateDisplay);

            _duplicateNotified = true;
            return new DialogueObservation(DialogueSignal.DuplicateSuppressed, _candidateDisplay);
        }

        var hadActive = _activeKey is not null;
        _activeKey = key;
        _lastAcceptedKey = key;
        _lastAcceptedAt = now;
        _duplicateNotified = false;
        return new DialogueObservation(hadActive ? DialogueSignal.Changed : DialogueSignal.Started, _candidateDisplay);
    }

    private DialogueObservation ObserveBlank()
    {
        _candidateKey = null;
        _candidateDisplay = string.Empty;
        _candidateCount = 0;
        _blankCount++;
        if (_activeKey is null || _blankCount < _stableReads)
            return new DialogueObservation(DialogueSignal.None, string.Empty);

        _activeKey = null;
        _duplicateNotified = false;
        return new DialogueObservation(DialogueSignal.Ended, string.Empty);
    }
}
