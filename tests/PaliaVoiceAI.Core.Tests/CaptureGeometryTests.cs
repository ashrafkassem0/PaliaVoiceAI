using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.Core.Tests;

public class CaptureGeometryTests
{
    [Fact]
    public void Crops_and_roundtrips_a_bmp()
    {
        var pixels = new byte[16];
        pixels[4] = 0x10;
        pixels[5] = 0x20;
        pixels[6] = 0x30;
        pixels[7] = 0xFF;
        var source = new CapturedImage(2, 2, pixels);

        var crop = BgraImage.Crop(source, new CaptureRegion(1, 0, 1, 1));
        Assert.Equal(new byte[] { 0x10, 0x20, 0x30, 0xFF }, crop.Pixels);

        var decoded = BgraImage.DecodeBmp(BgraImage.EncodeBmp(crop));
        Assert.Equal(crop.Pixels, decoded.Pixels);
    }

    [Fact]
    public void Rejects_a_region_outside_the_image()
    {
        var source = new CapturedImage(2, 2, new byte[16]);
        Assert.Throws<ArgumentOutOfRangeException>(() => BgraImage.Crop(source, new CaptureRegion(1, 0, 2, 1)));
    }

    [Fact]
    public void Suggests_the_bottom_band_of_a_1200p_client()
    {
        var region = DialogueRegion.Suggest(1920, 1200);
        Assert.Equal(new CaptureRegion(131, 688, 975, 438), region);
    }

    [Fact]
    public void Maps_a_client_region_through_the_window_frame()
    {
        var window = new ScreenRect(10, 20, 200, 120);
        var client = new ScreenRect(18, 52, 184, 80);
        var region = new CaptureRegion(4, 6, 20, 10);

        Assert.Equal(region, WindowRegionMapper.Map(window, client, 184, 80, region));
        Assert.Equal(new CaptureRegion(12, 38, 20, 10), WindowRegionMapper.Map(window, client, 200, 120, region));
    }

    [Fact]
    public void Prefers_the_shipping_client_over_a_larger_launcher()
    {
        var windows = new[]
        {
            new WindowCandidate("Palia", "Palia", 1920, 1080, true, false),
            new WindowCandidate("PaliaClientSteam-Win64-Shipping", "Palia", 1280, 720, true, false),
            new WindowCandidate("PaliaClientSteam-Win64-Shipping", "Palia", 800, 600, true, true)
        };

        Assert.Equal(1, GameWindowSelector.SelectIndex(windows));
    }

    [Fact]
    public void Falls_back_to_a_window_titled_Palia()
    {
        var windows = new[]
        {
            new WindowCandidate("notepad", "Notes", 800, 600, true, false),
            new WindowCandidate("GameOverlay", "Palia", 1920, 1080, true, false)
        };

        Assert.Equal(1, GameWindowSelector.SelectIndex(windows));
        Assert.Null(GameWindowSelector.SelectIndex([new WindowCandidate("notepad", "Palia Voice AI", 1920, 1080, true, false)]));
    }
}
