using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.App;

public partial class MainWindow
{
    private readonly ObservableCollection<NpcCard> _cards = new();
    private readonly ObservableCollection<SpokenClip> _spoken = new();
    private VoiceLibraryState _library = new();
    private ICollectionView? _voices;
    private string _filter = "all";
    private bool _loadingProfile;
    private bool _syncingCapture;

    private void LoadVoiceLibrary()
    {
        NpcCatalog.Load(RosterPath());
        _library = VoiceProfileStore.Load();
        foreach (var profile in NpcCatalog.All)
            _cards.Add(NpcCard.Create(profile, _library));

        _voices = CollectionViewSource.GetDefaultView(_cards);
        _voices.Filter = MatchesFilter;
        VoiceList.ItemsSource = _voices;
        SoundList.ItemsSource = _spoken;
        SoundList.MouseDoubleClick += (_, _) => ReplaySelected();

        ProfileVoiceBox.DisplayMemberPath = nameof(KokoroVoiceChoice.Label);
        ProfileVoiceBox.SelectedValuePath = nameof(KokoroVoiceChoice.Id);
        ProfileVoiceBox.ItemsSource = KokoroVoices.Assignable;
        VoiceBox.Items.Clear();
        foreach (var voice in KokoroVoices.English)
            VoiceBox.Items.Add(new ComboBoxItem { Content = voice.Label, Tag = voice.Id });
        VoiceBox.SelectedIndex = 0;

        _loadingProfile = true;
        HearYourselfBox.IsChecked = _library.HearYourself;
        ApplyVoiceBox.IsChecked = _library.ApplyVoice;
        var selected = _cards.FirstOrDefault(card => card.Id == _library.SelectedId) ?? _cards.First(card => card.Id == "kenyatta");
        VoiceList.SelectedItem = selected;
        ShowCard(selected);
        _loadingProfile = false;
        if (NpcCatalog.LoadError is not null)
            StatusText.Text = NpcCatalog.LoadError;
    }

    private static string RosterPath()
    {
        var shipped = Path.Combine(AppContext.BaseDirectory, "npc-voices.json");
        var user = AppPaths.NpcVoices;
        if (!File.Exists(user))
            return shipped;
        if (!File.Exists(shipped))
            return user;
        return File.GetLastWriteTimeUtc(user) >= File.GetLastWriteTimeUtc(shipped) ? user : shipped;
    }

    private static void ApplyAvatar(Portrait portrait, System.Windows.Shapes.Ellipse avatar, ImageSource? image)
    {
        if (image is null)
        {
            portrait.Visibility = Visibility.Visible;
            avatar.Visibility = Visibility.Collapsed;
            avatar.Fill = null;
            return;
        }

        portrait.Visibility = Visibility.Collapsed;
        avatar.Visibility = Visibility.Visible;
        avatar.Fill = new ImageBrush(image) { Stretch = Stretch.UniformToFill };
    }

    private bool MatchesFilter(object item)
    {
        if (item is not NpcCard card)
            return false;

        var query = SearchBox.Text.Trim();
        if (query.Length > 0 &&
            card.Name.Contains(query, StringComparison.OrdinalIgnoreCase) == false &&
            card.Role.Contains(query, StringComparison.OrdinalIgnoreCase) == false &&
            card.VoiceLabel.Contains(query, StringComparison.OrdinalIgnoreCase) == false)
            return false;

        return _filter switch
        {
            "women" => string.IsNullOrWhiteSpace(card.VoiceId)
                ? card.Gender == "female"
                : card.VoiceId.StartsWith("af_", StringComparison.OrdinalIgnoreCase) || card.VoiceId.StartsWith("bf_", StringComparison.OrdinalIgnoreCase),
            "men" => string.IsNullOrWhiteSpace(card.VoiceId)
                ? card.Gender == "male"
                : card.VoiceId.StartsWith("am_", StringComparison.OrdinalIgnoreCase) || card.VoiceId.StartsWith("bm_", StringComparison.OrdinalIgnoreCase),
            "british" => card.VoiceId.StartsWith("bf_", StringComparison.OrdinalIgnoreCase) || card.VoiceId.StartsWith("bm_", StringComparison.OrdinalIgnoreCase),
            "favorite" => card.IsFavorite,
            "custom" => card.IsCustom,
            _ => true
        };
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchHint.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        _voices?.Refresh();
    }

    private void Filter_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || sender is not RadioButton button)
            return;
        _filter = button.Tag as string ?? "all";
        FilterTitle.Text = button.Content as string ?? "All";
        _voices?.Refresh();
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || sender is not RadioButton button)
            return;
        var page = button.Tag as string;
        VoicePage.Visibility = page == "voices" ? Visibility.Visible : Visibility.Collapsed;
        SoundPage.Visibility = page == "sound" ? Visibility.Visible : Visibility.Collapsed;
        StudioPage.Visibility = page == "studio" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPage.Visibility = page == "settings" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void VoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingProfile || VoiceList.SelectedItem is not NpcCard card)
            return;
        ShowCard(card);
        SaveLibrary();
    }

    private void ShowCard(NpcCard card)
    {
        _loadingProfile = true;
        DetailPortrait.AccentBrush = card.Accent;
        DetailPortrait.HairBrush = card.Hair;
        DetailPortrait.SkinBrush = card.Skin;
        DetailPortrait.ShirtBrush = card.Shirt;
        DetailPortrait.Variant = card.Variant;
        ApplyAvatar(DetailPortrait, DetailAvatar, card.Avatar);
        BottomPortrait.AccentBrush = card.Accent;
        BottomPortrait.HairBrush = card.Hair;
        BottomPortrait.SkinBrush = card.Skin;
        BottomPortrait.ShirtBrush = card.Shirt;
        BottomPortrait.Variant = card.Variant;
        ApplyAvatar(BottomPortrait, BottomAvatar, card.Avatar);
        DetailName.Text = card.Name;
        DetailRole.Text = card.Role;
        DetailIntro.Text = card.Intro;
        DetailVoiceStatus.Text = card.VoiceStatusText;
        BottomName.Text = card.Name;
        BottomRole.Text = card.VoiceLabel;
        ProfileVoiceBox.SelectedValue = card.VoiceId;
        PaceSlider.Value = card.Pace;
        _loadingProfile = false;
    }

    private void ProfileVoiceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingProfile || VoiceList.SelectedItem is not NpcCard card || ProfileVoiceBox.SelectedValue is not string voiceId)
            return;
        card.VoiceId = voiceId;
        BottomRole.Text = card.VoiceLabel;
        SaveLibrary();
        _voices?.Refresh();
    }

    private void PaceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_loadingProfile || !IsLoaded || VoiceList.SelectedItem is not NpcCard card)
            return;
        card.Pace = (float)PaceSlider.Value;
        SaveLibrary();
    }

    private void Favorite_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not NpcCard card)
            return;
        card.IsFavorite = !card.IsFavorite;
        VoiceList.SelectedItem = card;
        SaveLibrary();
        _voices?.Refresh();
    }

    private void LibraryToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (_loadingProfile || !IsLoaded || _cards.Count == 0)
            return;
        SaveLibrary();
    }

    private async void PreviewVoice_Click(object sender, RoutedEventArgs e)
    {
        if (VoiceList.SelectedItem is not NpcCard card)
        {
            StatusText.Text = "Choose a villager first.";
            return;
        }

        if (!KokoroVoices.IsKnown(card.VoiceId))
        {
            StatusText.Text = string.IsNullOrWhiteSpace(card.VoiceId)
                ? $"{card.Name} has no Kokoro voice yet. Choose one in the list, or set voiceId in npc-voices.json."
                : $"{card.Name} voice '{card.VoiceId}' is not a Kokoro voice in this app.";
            return;
        }

        var (threads, backend) = ReadEngineChoice();
        await SpeakAsync(card.Intro, card.VoiceId, threads, backend, card.Pace, play: true, speaker: card.Name);
    }

    private void LiveCapture_Changed(object sender, RoutedEventArgs e)
    {
        if (_syncingCapture || !IsLoaded)
            return;
        var want = LiveCaptureBox.IsChecked == true;
        if (want == (_monitorCts is not null))
            return;

        Monitor_Click(sender, e);
        if (want && _monitorCts is null)
        {
            _syncingCapture = true;
            LiveCaptureBox.IsChecked = false;
            _syncingCapture = false;
            StatusText.Text = CaptureStatus.Text;
        }
    }

    private void RememberLine(string speaker, string text, byte[] audio)
    {
        _spoken.Insert(0, new SpokenClip(speaker, text, audio));
        if (_spoken.Count > 30)
            _spoken.RemoveAt(_spoken.Count - 1);
        SoundEmpty.Visibility = _spoken.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ReplaySelected()
    {
        if (SoundList.SelectedItem is not SpokenClip clip)
            return;
        Interlocked.Increment(ref _speakSerial);
        _speech.Stop();
        try
        {
            _lastAudio = clip.Audio;
            _player.PlayWav(clip.Audio);
            StatusText.Text = $"Playing {clip.Speaker}.";
        }
        catch (Exception ex)
        {
            _log.Error("playback", ex.GetType().Name);
            StatusText.Text = ex.Message;
        }
    }

    private void SaveLibrary()
    {
        var selected = (VoiceList.SelectedItem as NpcCard)?.Id ?? _library.SelectedId;
        var state = new VoiceLibraryState
        {
            SelectedId = selected,
            ApplyVoice = ApplyVoiceBox.IsChecked == true,
            HearYourself = HearYourselfBox.IsChecked == true
        };
        foreach (var card in _cards)
        {
            if (card.IsCustom)
                state.Voices[card.Id] = card.VoiceId;
            if (Math.Abs(card.Pace - card.DefaultPace) > 0.01f)
                state.Paces[card.Id] = Math.Round(card.Pace, 2);
            if (card.IsFavorite)
                state.Favorites.Add(card.Id);
        }

        _library = state;
        try
        {
            VoiceProfileStore.Save(state);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _log.Error("voices", ex.GetType().Name);
            StatusText.Text = ex.Message;
        }
    }

    private (int threads, TtsBackend backend) ReadEngineChoice()
    {
        var threads = 3;
        if (ThreadsBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            threads = parsed;
        var backend = BackendBox.SelectedIndex switch
        {
            2 => TtsBackend.Cuda,
            0 => TtsBackend.Mock,
            _ => TtsBackend.Cpu
        };
        return (threads, backend);
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void SpeakCapturedLine(string text, string? speakerId)
    {
        var card = SpeakerIdentifier.IsNamedSpeaker(speakerId)
            ? _cards.FirstOrDefault(item => item.Id == speakerId)
            : null;
        if (card is null)
        {
            var skipped = "No name was on the card. The line was not spoken.";
            StatusText.Text = skipped;
            CaptureStatus.Text = skipped;
            return;
        }
        _filter = "all";
        _voices?.Refresh();
        VoiceList.SelectedItem = card;
        if (CopyDialogueBox.IsChecked == true)
            DialogueBox.Text = text;

        StatusText.Text = KokoroVoices.IsKnown(card.VoiceId)
            ? $"{card.Name} is speaking with {card.VoiceLabel}."
            : string.IsNullOrWhiteSpace(card.VoiceId)
                ? $"{card.Name} has no Kokoro voice yet. The line was not spoken."
                : $"{card.Name} voice '{card.VoiceId}' is not a Kokoro voice in this app. The line was not spoken.";
        if (ApplyVoiceBox.IsChecked != true || !KokoroVoices.IsKnown(card.VoiceId))
            return;

        var (threads, backend) = ReadEngineChoice();
        _ = SpeakAsync(text, card.VoiceId, threads, backend, card.Pace, HearYourselfBox.IsChecked == true, card.Name);
    }
}

public sealed class SpokenClip
{
    public SpokenClip(string speaker, string text, byte[] audio)
    {
        Speaker = speaker;
        Text = text;
        Audio = audio;
    }

    public string Speaker { get; }
    public string Text { get; }
    public byte[] Audio { get; }
    public string Caption => $"{Speaker} — {Text}";
}

public sealed class NpcCard : INotifyPropertyChanged
{
    private string _voiceId = string.Empty;
    private bool _favorite;
    private float _pace = 1f;

    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public required string Intro { get; init; }
    public required string DefaultVoiceId { get; init; }
    public required float DefaultPace { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string VoiceStatusText { get; init; } = "No Kokoro voice is configured.";
    public ImageSource? Avatar { get; init; }
    public Visibility AvatarVisibility => Avatar is null ? Visibility.Collapsed : Visibility.Visible;
    public Visibility PortraitVisibility => Avatar is null ? Visibility.Visible : Visibility.Collapsed;
    public required Brush Accent { get; init; }
    public required Brush Hair { get; init; }
    public required Brush Skin { get; init; }
    public required Brush Shirt { get; init; }
    public int Variant { get; init; }

    public string VoiceId
    {
        get => _voiceId;
        set
        {
            if (_voiceId == value)
                return;
            _voiceId = value;
            Notify();
            Notify(nameof(VoiceLabel));
            Notify(nameof(IsCustom));
        }
    }

    public float Pace
    {
        get => _pace;
        set => _pace = value;
    }

    public bool IsFavorite
    {
        get => _favorite;
        set
        {
            if (_favorite == value)
                return;
            _favorite = value;
            Notify();
            Notify(nameof(HeartGlyph));
            Notify(nameof(HeartBrush));
        }
    }

    public string VoiceLabel => KokoroVoices.Label(VoiceId);
    public bool IsCustom => !string.Equals(VoiceId, DefaultVoiceId, StringComparison.OrdinalIgnoreCase);
    public string HeartGlyph => IsFavorite ? "\uEB52" : "\uEB51";
    public Brush HeartBrush => IsFavorite ? HeartOn : HeartOff;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static NpcCard Create(NpcProfile profile, VoiceLibraryState library)
    {
        var look = Look(profile.Id);
        return new NpcCard
        {
            Id = profile.Id,
            Name = profile.Name,
            Role = profile.Role,
            Intro = profile.Intro,
            DefaultVoiceId = profile.VoiceId,
            DefaultPace = profile.Speed,
            Gender = profile.Gender,
            VoiceStatusText = profile.HasAssignedVoice
                ? profile.VoiceSelectionConfidence
                : string.IsNullOrWhiteSpace(profile.VoiceId)
                    ? "No Kokoro voice is configured."
                    : $"Voice '{profile.VoiceId}' is not available in this app.",
            Avatar = LoadAvatar(profile.Avatar),
            VoiceId = library.VoiceFor(profile),
            Pace = library.PaceFor(profile.Id, profile.Speed),
            IsFavorite = library.IsFavorite(profile.Id),
            Accent = look.Accent,
            Hair = look.Hair,
            Skin = look.Skin,
            Shirt = look.Shirt,
            Variant = look.Variant
        };
    }

    private static ImageSource? LoadAvatar(string? relative)
    {
        if (string.IsNullOrWhiteSpace(relative))
            return null;

        var path = Path.IsPathRooted(relative)
            ? relative
            : Path.Combine(AppContext.BaseDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
            return null;

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 256;
            image.UriSource = new Uri(path);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static readonly SolidColorBrush HeartOn = Freeze("#FF6B9A");
    private static readonly SolidColorBrush HeartOff = Freeze("#6E6888");

    private static (Brush Accent, Brush Hair, Brush Skin, Brush Shirt, int Variant) Look(string id) => id switch
    {
        "kenyatta" => (Freeze("#F0A0C4"), Freeze("#C44B86"), Freeze("#F6C7A8"), Freeze("#7C5CFF"), 0),
        "eshe" => (Freeze("#C9B6F2"), Freeze("#3A3158"), Freeze("#E8B896"), Freeze("#5C4A86"), 0),
        "tish" => (Freeze("#F6C98A"), Freeze("#E2A04A"), Freeze("#F4C9A6"), Freeze("#E07A3D"), 0),
        "jina" => (Freeze("#9FD0F2"), Freeze("#2C3E55"), Freeze("#F2C3A4"), Freeze("#3E7CA8"), 1),
        "sifuu" => (Freeze("#F0A070"), Freeze("#8A3E2A"), Freeze("#E8B08C"), Freeze("#C45A32"), 1),
        "najuma" => (Freeze("#F2B4C8"), Freeze("#6B3A4A"), Freeze("#F6CDB4"), Freeze("#D36A8C"), 2),
        "tamala" => (Freeze("#C7E0A8"), Freeze("#2E4A32"), Freeze("#E8C0A0"), Freeze("#5E8A4A"), 0),
        "caleri" => (Freeze("#D9D3F2"), Freeze("#4A4560"), Freeze("#F0C8B0"), Freeze("#6A6288"), 2),
        "elouisa" => (Freeze("#F6E2A0"), Freeze("#E8C15A"), Freeze("#F8D0B4"), Freeze("#E0A040"), 0),
        "subira" => (Freeze("#E8D8C8"), Freeze("#F2E6D4"), Freeze("#C9956E"), Freeze("#8A6A4A"), 3),
        "hekla" => (Freeze("#B7C4D4"), Freeze("#D8DEE6"), Freeze("#E8C4A4"), Freeze("#6E7E90"), 1),
        "delaila" => (Freeze("#F2C2A0"), Freeze("#A85A38"), Freeze("#F0C0A0"), Freeze("#C46A48"), 0),
        "ashura" => (Freeze("#8E7358"), Freeze("#3A2A22"), Freeze("#C48A62"), Freeze("#5C4030"), 1),
        "reth" => (Freeze("#F0A060"), Freeze("#D45A28"), Freeze("#F0B890"), Freeze("#E07030"), 1),
        "hassian" => (Freeze("#6E8A62"), Freeze("#243028"), Freeze("#C9956E"), Freeze("#3E5A40"), 1),
        "hodari" => (Freeze("#8A6A52"), Freeze("#2A211C"), Freeze("#B88868"), Freeze("#5A4034"), 3),
        "jel" => (Freeze("#E8C8F0"), Freeze("#F2E4C8"), Freeze("#F4CDB4"), Freeze("#C9A0D8"), 0),
        "zeki" => (Freeze("#7A6A58"), Freeze("#4A3A30"), Freeze("#C4A080"), Freeze("#6A5344"), 1),
        "chayne" => (Freeze("#F2E2C0"), Freeze("#E8D2A0"), Freeze("#E8B890"), Freeze("#C8A060"), 3),
        "einar" => (Freeze("#8AA0B0"), Freeze("#5A6E7C"), Freeze("#C8B8A4"), Freeze("#607888"), 1),
        "badruu" => (Freeze("#E8C878"), Freeze("#C89840"), Freeze("#E8B888"), Freeze("#D0A050"), 1),
        "kenli" => (Freeze("#7EC8C0"), Freeze("#3A6A66"), Freeze("#E8B890"), Freeze("#3E8A84"), 1),
        "naio" => (Freeze("#F2D2A8"), Freeze("#C4844A"), Freeze("#F0C4A0"), Freeze("#E0A060"), 1),
        _ => (Freeze("#3A3458"), Freeze("#6E6888"), Freeze("#E8C4B0"), Freeze("#7C5CFF"), 1)
    };

    private static SolidColorBrush Freeze(string hex)
    {
        var brush = (SolidColorBrush)new BrushConverter().ConvertFrom(hex)!;
        brush.Freeze();
        return brush;
    }
}
