using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class BenchmarkCsvTests
{
    [Fact]
    public void Roundtrips_records_and_keeps_commas_inside_quotes()
    {
        var path = Path.Combine(Path.GetTempPath(), "PaliaVoiceAI-tests", Guid.NewGuid().ToString("N"), "benchmarks.csv");
        var records = new[]
        {
            new BenchmarkRecord(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero), "Mock, \"pipeline\"", "Mock", 3, 42, 250.5, 700, 0.3579),
            new BenchmarkRecord(new DateTimeOffset(2026, 11, 17, 10, 1, 0, TimeSpan.Zero), "Windows OCR", "CPU", 2, 8, 12.25, 80.5, 0.1522)
        };

        BenchmarkCsv.Write(path, records);
        var loaded = BenchmarkCsv.Read(path);

        Assert.Equal(records, loaded);
    }

    [Fact]
    public void Missing_file_is_an_empty_history()
    {
        var path = Path.Combine(Path.GetTempPath(), "PaliaVoiceAI-tests", Guid.NewGuid().ToString("N"), "missing.csv");
        Assert.Empty(BenchmarkCsv.Read(path));
    }
}
