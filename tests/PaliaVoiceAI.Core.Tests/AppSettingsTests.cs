using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class AppSettingsTests
{
    [Fact]
    public void Missing_file_uses_cpu_three_threads_bella_and_full_volume()
    {
        var directory = CreateDirectory();
        try
        {
            var loaded = AppSettingsStore.Load(Path.Combine(directory, "settings.json"));
            Assert.Equal(nameof(TtsBackend.Cpu), loaded.Backend);
            Assert.Equal(3, loaded.Threads);
            Assert.Equal(AppSettings.DefaultVoiceId, loaded.StudioVoiceId);
            Assert.Equal(100, loaded.VolumePercent);
            Assert.Equal(1f, loaded.VolumeLevel);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Settings_round_trip_and_a_sentence_is_not_kept_as_a_voice()
    {
        var directory = CreateDirectory();
        try
        {
            var path = Path.Combine(directory, "settings.json");
            AppSettingsStore.Save(new AppSettings
            {
                Backend = "cuda",
                Threads = 4,
                StudioVoiceId = "bm_lewis",
                VolumePercent = 40
            }, path);

            var loaded = AppSettingsStore.Load(path);
            Assert.Equal(nameof(TtsBackend.Cuda), loaded.Backend);
            Assert.Equal(TtsBackend.Cuda, loaded.BackendKind);
            Assert.Equal(4, loaded.Threads);
            Assert.Equal("bm_lewis", loaded.StudioVoiceId);
            Assert.Equal(40, loaded.VolumePercent);
            Assert.Equal(0.4f, loaded.VolumeLevel);
            Assert.False(File.Exists(path + ".tmp"));
            var stored = File.ReadAllText(path);
            Assert.DoesNotContain("BackendKind", stored);
            Assert.DoesNotContain("VolumeLevel", stored);

            AppSettingsStore.Save(new AppSettings
            {
                Backend = "gpu",
                Threads = 99,
                StudioVoiceId = "Hello! Welcome to Kilima Village. How can I help you today?",
                VolumePercent = 400
            }, path);
            var repaired = AppSettingsStore.Load(path);
            Assert.Equal(nameof(TtsBackend.Cpu), repaired.Backend);
            Assert.Equal(3, repaired.Threads);
            Assert.Equal(AppSettings.DefaultVoiceId, repaired.StudioVoiceId);
            Assert.Equal(100, repaired.VolumePercent);
            var json = File.ReadAllText(path);
            Assert.DoesNotContain("Kilima", json);
            Assert.DoesNotContain("Hello", json);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Broken_json_loads_the_starting_values()
    {
        var directory = CreateDirectory();
        try
        {
            var path = Path.Combine(directory, "settings.json");
            File.WriteAllText(path, "{");
            var loaded = AppSettingsStore.Load(path);
            Assert.Equal(nameof(TtsBackend.Cpu), loaded.Backend);
            Assert.Equal(3, loaded.ClampedThreads);
            Assert.Equal(AppSettings.DefaultVoiceId, loaded.ClampedVoiceId);
            Assert.Equal(100, loaded.ClampedVolumePercent);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "palia-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
