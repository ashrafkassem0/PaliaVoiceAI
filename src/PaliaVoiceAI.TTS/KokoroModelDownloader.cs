using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.TTS;

public static class KokoroModelDownloader
{
    public static async Task<string> EnsureAsync(Action<string>? progress, CancellationToken cancellationToken)
    {
        var existing = KokoroModelLocator.Find();
        if (existing is not null)
            return existing;

        Directory.CreateDirectory(AppPaths.ModelsDirectory);
        var destination = AppPaths.KokoroOnnx;
        var temporary = destination + ".download";
        progress?.Invoke("Kokoro was not found. Downloading the 82M model (about 310 MB). This happens once.");

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("PaliaVoiceAI");
            using var response = await client.GetAsync(KokoroModelLocator.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var total = response.Content.Headers.ContentLength;
            await using (var input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var output = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[128 * 1024];
                long readTotal = 0;
                var lastPercent = -1;
                int read;
                while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    readTotal += read;
                    if (total is not > 0)
                        continue;

                    var percent = (int)(readTotal * 100 / total.Value);
                    if (percent == lastPercent)
                        continue;
                    lastPercent = percent;
                    progress?.Invoke($"Downloading Kokoro 82M… {percent}%");
                }
            }

            var downloaded = new FileInfo(temporary);
            if (downloaded.Length < 50_000_000)
                throw new InvalidOperationException($"The Kokoro download stopped early ({downloaded.Length:N0} bytes).");

            File.Move(temporary, destination, overwrite: true);
        }
        catch
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
            throw;
        }

        progress?.Invoke("Kokoro download finished.");
        return destination;
    }
}
