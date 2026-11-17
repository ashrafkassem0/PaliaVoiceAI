using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.App;

public partial class MainWindow
{
    private void LoadCaptureSettings()
    {
        var settings = CaptureSettingsStore.Load();
        RegionX.Text = settings.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RegionY.Text = settings.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RegionWidth.Text = settings.Width.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RegionHeight.Text = settings.Height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        PollIntervalBox.Text = settings.PollIntervalMs.ToString(System.Globalization.CultureInfo.InvariantCulture);
        StableReadsBox.Text = settings.StableReads.ToString(System.Globalization.CultureInfo.InvariantCulture);
        DuplicateWindowBox.Text = settings.DuplicateWindowSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        UseGameWindowBox.IsChecked = settings.UseGameWindow;
        CopyDialogueBox.IsChecked = settings.CopyAcceptedText;
        UpdateRegionHint();
    }

    private void TrySaveCaptureSettings()
    {
        if (TryReadCaptureSettings(out var settings, out _))
            CaptureSettingsStore.Save(settings);
    }

    private void CaptureMode_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded)
            UpdateRegionHint();
    }

    private void Detect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var window = _capture.FindGameWindow();
            GameWindowStatus.Text = window is null
                ? "Palia is not running. The lookup checks the Steam and standalone shipping clients, then a window titled Palia."
                : $"{window.ProcessName} — {window.Title} — client {window.ClientBounds.Width}×{window.ClientBounds.Height} at ({window.ClientBounds.X}, {window.ClientBounds.Y}).";
        }
        catch (Exception ex)
        {
            GameWindowStatus.Text = ex.Message;
        }
    }

    private void Suggest_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var window = UseGameWindowBox.IsChecked == true ? _capture.FindGameWindow() : null;
            var region = window is null
                ? DialogueRegion.Suggest(1920, 1200)
                : DialogueRegion.Suggest(window.ClientBounds.Width, window.ClientBounds.Height);
            SetRegion(region);
            CaptureStatus.Text = window is null
                ? "Suggested the lower-left dialogue card for 1920×1200. Preview it and move the rectangle if the line is cut off."
                : $"Suggested the lower-left dialogue card for the {window.ClientBounds.Width}×{window.ClientBounds.Height} client. Preview it and move the rectangle if the line is cut off.";
        }
        catch (Exception ex)
        {
            CaptureStatus.Text = ex.Message;
        }
    }

    private async void Preview_Click(object sender, RoutedEventArgs e)
    {
        if (_monitorCts is not null)
            return;
        if (!TryReadCaptureSettings(out var settings, out var error))
        {
            CaptureStatus.Text = error;
            return;
        }

        PreviewButton.IsEnabled = false;
        try
        {
            CaptureSettingsStore.Save(settings);
            if (!settings.UseGameWindow)
                _capture.Release();

            var image = await Task.Run(() => _capture.CaptureAsync(settings.ToTarget()));
            var ocr = await Task.Run(() => _ocr.RecognizeAsync(image));
            ShowImage(image);
            File.WriteAllBytes(AppPaths.LastCaptureBmp, BgraImage.EncodeBmp(image));
            var normalized = TextNormalizer.Normalize(ocr.Text);
            OcrPreviewText.Text = normalized.Length == 0 ? "(no text)" : normalized;
            CaptureStatus.Text = normalized.Length == 0
                ? $"Preview captured {image.Width}×{image.Height}, and OCR found no text. Saved {AppPaths.LastCaptureBmp}."
                : $"Preview OCR captured {image.Width}×{image.Height}. Saved {AppPaths.LastCaptureBmp}.";
        }
        catch (Exception ex)
        {
            CaptureStatus.Text = ex.Message;
        }
        finally
        {
            PreviewButton.IsEnabled = _monitorCts is null;
        }
    }

    private void Monitor_Click(object sender, RoutedEventArgs e)
    {
        if (_monitorCts is not null)
        {
            _monitorCts.Cancel();
            return;
        }

        if (!TryReadCaptureSettings(out var settings, out var error))
        {
            CaptureStatus.Text = error;
            return;
        }

        CaptureSettingsStore.Save(settings);
        _monitorCts = new CancellationTokenSource();
        SetMonitoring(true);
        var token = _monitorCts.Token;
        _ = Task.Run(() => MonitorLoopAsync(settings, token));
    }

    private async Task MonitorLoopAsync(CaptureSettings settings, CancellationToken token)
    {
        var tracker = new DialogueTracker(settings.StableReads, TimeSpan.FromSeconds(settings.DuplicateWindowSeconds));
        try
        {
            while (!token.IsCancellationRequested)
            {
                var started = DateTime.UtcNow;
                try
                {
                    var image = await _capture.CaptureAsync(settings.ToTarget(), token).ConfigureAwait(false);
                    var ocr = await _ocr.RecognizeAsync(image, token).ConfigureAwait(false);
                    var observation = tracker.Observe(ocr.Text, DateTimeOffset.Now);
                    var saveImage = observation.HasNewDialogue;
                    var speakerId = observation.HasNewDialogue ? SpeakerIdentifier.FindNpcId(ocr.Text) : null;
                    await Dispatcher.InvokeAsync(() => ApplyObservation(image, observation, saveImage, speakerId));
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    try
                    {
                        await Dispatcher.InvokeAsync(() => CaptureStatus.Text = ex.Message);
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }

                var remaining = TimeSpan.FromMilliseconds(settings.PollIntervalMs) - (DateTime.UtcNow - started);
                if (remaining > TimeSpan.Zero)
                    await Task.Delay(remaining, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _capture.Release();
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    SetMonitoring(false);
                    _monitorCts?.Dispose();
                    _monitorCts = null;
                    CaptureStatus.Text = "Monitoring stopped.";
                });
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    private void ApplyObservation(CapturedImage image, DialogueObservation observation, bool saveImage, string? speakerId = null)
    {
        ShowImage(image);
        OcrPreviewText.Text = observation.Text.Length == 0 ? "(no text)" : observation.Text;
        if (saveImage)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.Root);
                File.WriteAllBytes(AppPaths.LastCaptureBmp, BgraImage.EncodeBmp(image));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                CaptureStatus.Text = ex.Message;
                return;
            }
        }

        CaptureStatus.Text = observation.Signal switch
        {
            DialogueSignal.Started => "New dialogue.",
            DialogueSignal.Changed => "Dialogue changed.",
            DialogueSignal.Ended => "Dialogue ended.",
            DialogueSignal.DuplicateSuppressed => "This line was already read inside the duplicate window.",
            _ => observation.Text.Length == 0 ? "No dialogue in the region." : "Text is still changing. Waiting until it stays still."
        };

        if (observation.HasNewDialogue)
            SpeakCapturedLine(observation.Text, speakerId);
    }

    private void ShowImage(CapturedImage image)
    {
        var bitmap = new BitmapImage();
        using var stream = new MemoryStream(BgraImage.EncodeBmp(image));
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        PreviewImage.Source = bitmap;
    }

    private void SetMonitoring(bool running)
    {
        MonitorButton.Content = running ? "Stop monitoring" : "Start monitoring";
        PreviewButton.IsEnabled = !running;
        DetectButton.IsEnabled = !running;
        SuggestButton.IsEnabled = !running;
        UseGameWindowBox.IsEnabled = !running;
        _syncingCapture = true;
        LiveCaptureBox.IsChecked = running;
        _syncingCapture = false;
    }

    private void SetRegion(CaptureRegion region)
    {
        RegionX.Text = region.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RegionY.Text = region.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RegionWidth.Text = region.Width.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RegionHeight.Text = region.Height.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private void UpdateRegionHint()
    {
        RegionHint.Text = UseGameWindowBox.IsChecked == true
            ? "Region is relative to the Palia client area, in pixels."
            : "Region is an absolute screen rectangle, in pixels. Use this when Palia is not running.";
    }

    private bool TryReadCaptureSettings(out CaptureSettings settings, out string error)
    {
        settings = new CaptureSettings();
        error = string.Empty;
        if (!TryReadInt(RegionX, "X", -20000, 20000, out var x, out error) ||
            !TryReadInt(RegionY, "Y", -20000, 20000, out var y, out error) ||
            !TryReadInt(RegionWidth, "Width", 1, CaptureRegion.MaxEdge, out var width, out error) ||
            !TryReadInt(RegionHeight, "Height", 1, CaptureRegion.MaxEdge, out var height, out error) ||
            !TryReadInt(PollIntervalBox, "Poll interval", 100, 5000, out var poll, out error) ||
            !TryReadInt(StableReadsBox, "Stable reads", 1, 8, out var stable, out error) ||
            !TryReadInt(DuplicateWindowBox, "Duplicate window", 0, 120, out var duplicate, out error))
        {
            return false;
        }

        settings = new CaptureSettings
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            PollIntervalMs = poll,
            StableReads = stable,
            DuplicateWindowSeconds = duplicate,
            UseGameWindow = UseGameWindowBox.IsChecked == true,
            CopyAcceptedText = CopyDialogueBox.IsChecked == true
        };
        return true;
    }

    private static bool TryReadInt(System.Windows.Controls.TextBox box, string label, int min, int max, out int value, out string error)
    {
        if (!int.TryParse(box.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value) ||
            value < min || value > max)
        {
            error = $"{label} must be a whole number from {min} to {max}.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
