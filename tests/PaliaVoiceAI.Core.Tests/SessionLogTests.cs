using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class SessionLogTests
{
    private const string SpokenLine = "Hello! Welcome to Kilima Village. How can I help you today?";

    [Fact]
    public void Error_and_timing_lines_omit_the_spoken_line()
    {
        var directory = CreateDirectory();
        try
        {
            var log = new SessionLog(directory, clock: () => new DateTimeOffset(2026, 11, 17, 16, 0, 0, TimeSpan.Zero));
            log.Error("speech", SpokenLine);
            log.Timing(new SpeechTiming(SpokenLine, SpokenLine, 3, 1598, 4510, 0.354323, false, SpokenLine.Length));

            var text = File.ReadAllText(log.CurrentPath);
            Assert.Contains(SessionLog.Header, text);
            Assert.Contains("2026-11-17T16:00:00.0000000+00:00 error speech Exception", text);
            Assert.Contains($"timing backend=Unknown voice=unknown threads=3 generationMs=1598.00 audioMs=4510.00 rtf=0.3543 cache=false chars={SpokenLine.Length}", text);
            Assert.DoesNotContain("Kilima", text);
            Assert.DoesNotContain("Hello", text);
            Assert.DoesNotContain("Welcome", text);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Known_timing_fields_are_written_and_the_file_rolls_over()
    {
        var directory = CreateDirectory();
        try
        {
            var log = new SessionLog(directory, maxBytes: 700, maxArchives: 2);
            log.Timing(new SpeechTiming("Cpu", "af_bella", 3, 12.5, 40, 0.3125, true, 8));
            var first = File.ReadAllText(log.CurrentPath);
            Assert.Contains("backend=Cpu voice=af_bella threads=3 generationMs=12.50 audioMs=40.00 rtf=0.3125 cache=true chars=8", first);

            for (var i = 0; i < 40; i++)
                log.Error("capture", "InvalidOperationException");

            var files = Directory.GetFiles(directory).Select(Path.GetFileName).OrderBy(name => name).ToArray();
            Assert.Equal(new[] { "session.1.log", "session.2.log", "session.log" }, files);
            foreach (var file in Directory.GetFiles(directory))
            {
                Assert.True(new FileInfo(file).Length <= 700);
                Assert.DoesNotContain("Kilima", File.ReadAllText(file));
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void A_blocked_log_folder_does_not_throw()
    {
        var blocked = Path.Combine(Path.GetTempPath(), "palia-log-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(blocked, "not a directory");
        try
        {
            var log = new SessionLog(blocked);
            log.Error("speech", "InvalidOperationException");
            SessionLog.WriteError(blocked, "app", "InvalidOperationException");
        }
        finally
        {
            File.Delete(blocked);
        }
    }

    private static string CreateDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "palia-log-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
