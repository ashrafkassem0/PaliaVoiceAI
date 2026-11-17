namespace PaliaVoiceAI.Core;

public enum TtsBackend { Mock, Cpu, Cuda }

public sealed record TtsOptions(
    TtsBackend Backend,
    int IntraOpThreads = 3,
    int InterOpThreads = 1);

public sealed record TtsResult(
    byte[] AudioBytes,
    string ContentType,
    TimeSpan GenerationTime,
    TimeSpan AudioDuration,
    string EngineName)
{
    public double RealTimeFactor =>
        AudioDuration.TotalSeconds <= 0 ? 0 :
        GenerationTime.TotalSeconds / AudioDuration.TotalSeconds;
}

public sealed record BenchmarkRecord(
    DateTimeOffset Timestamp,
    string Engine,
    string Backend,
    int Threads,
    int TextLength,
    double GenerationMs,
    double AudioDurationMs,
    double RealTimeFactor);
