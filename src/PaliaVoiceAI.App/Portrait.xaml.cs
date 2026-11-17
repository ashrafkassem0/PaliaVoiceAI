using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PaliaVoiceAI.App;

public partial class Portrait : UserControl
{
    public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
        nameof(AccentBrush), typeof(Brush), typeof(Portrait), new PropertyMetadata(Brushes.Purple, Refresh));
    public static readonly DependencyProperty HairBrushProperty = DependencyProperty.Register(
        nameof(HairBrush), typeof(Brush), typeof(Portrait), new PropertyMetadata(Brushes.Black, Refresh));
    public static readonly DependencyProperty SkinBrushProperty = DependencyProperty.Register(
        nameof(SkinBrush), typeof(Brush), typeof(Portrait), new PropertyMetadata(Brushes.Bisque, Refresh));
    public static readonly DependencyProperty ShirtBrushProperty = DependencyProperty.Register(
        nameof(ShirtBrush), typeof(Brush), typeof(Portrait), new PropertyMetadata(Brushes.Navy, Refresh));
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(int), typeof(Portrait), new PropertyMetadata(0, Refresh));

    public Portrait()
    {
        InitializeComponent();
        Loaded += (_, _) => Apply();
    }

    public Brush AccentBrush { get => (Brush)GetValue(AccentBrushProperty); set => SetValue(AccentBrushProperty, value); }
    public Brush HairBrush { get => (Brush)GetValue(HairBrushProperty); set => SetValue(HairBrushProperty, value); }
    public Brush SkinBrush { get => (Brush)GetValue(SkinBrushProperty); set => SetValue(SkinBrushProperty, value); }
    public Brush ShirtBrush { get => (Brush)GetValue(ShirtBrushProperty); set => SetValue(ShirtBrushProperty, value); }
    public int Variant { get => (int)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }

    private static void Refresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Portrait portrait && portrait.IsLoaded)
            portrait.Apply();
    }

    private void Apply()
    {
        Bun.Visibility = Variant % 4 == 2 ? Visibility.Visible : Visibility.Collapsed;
        Hair.Margin = (Variant % 4) switch
        {
            1 => new Thickness(18, 8, 18, 46),
            2 => new Thickness(14, 16, 14, 40),
            3 => new Thickness(4, 6, 4, 22),
            _ => new Thickness(8, 0, 8, 14)
        };
    }
}
