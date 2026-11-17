namespace PaliaVoiceAI.Core;

public static class BgraImage
{
    public static CapturedImage Crop(CapturedImage source, CaptureRegion region)
    {
        region.EnsureValid();
        if (region.X < 0 || region.Y < 0 || region.X + region.Width > source.Width || region.Y + region.Height > source.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(region),
                $"Region ({region.X}, {region.Y}, {region.Width}×{region.Height}) is outside the captured image ({source.Width}×{source.Height}).");
        }

        var pixels = new byte[region.Width * region.Height * 4];
        var rowBytes = region.Width * 4;
        for (var y = 0; y < region.Height; y++)
        {
            var sourceIndex = ((region.Y + y) * source.Width + region.X) * 4;
            Buffer.BlockCopy(source.Pixels, sourceIndex, pixels, y * rowBytes, rowBytes);
        }

        return new CapturedImage(region.Width, region.Height, pixels);
    }

    public static byte[] EncodeBmp(CapturedImage image)
    {
        var stride = image.Stride;
        var pixelBytes = stride * image.Height;
        var file = new byte[54 + pixelBytes];
        file[0] = (byte)'B';
        file[1] = (byte)'M';
        WriteInt(file, 2, file.Length);
        WriteInt(file, 10, 54);
        WriteInt(file, 14, 40);
        WriteInt(file, 18, image.Width);
        WriteInt(file, 22, image.Height);
        WriteShort(file, 26, 1);
        WriteShort(file, 28, 32);
        WriteInt(file, 34, pixelBytes);

        for (var y = 0; y < image.Height; y++)
        {
            var sourceIndex = (image.Height - 1 - y) * stride;
            Buffer.BlockCopy(image.Pixels, sourceIndex, file, 54 + y * stride, stride);
        }

        return file;
    }

    public static CapturedImage DecodeBmp(byte[] file)
    {
        if (file is null || file.Length < 54 || file[0] != (byte)'B' || file[1] != (byte)'M')
            throw new InvalidOperationException("The image is not a BMP.");

        var pixelOffset = BitConverter.ToInt32(file, 10);
        var width = BitConverter.ToInt32(file, 18);
        var heightRaw = BitConverter.ToInt32(file, 22);
        var planes = BitConverter.ToInt16(file, 26);
        var bits = BitConverter.ToInt16(file, 28);
        if (planes != 1 || bits != 32 || width <= 0 || heightRaw == 0)
            throw new InvalidOperationException($"Unsupported BMP ({width}×{heightRaw}, {bits} bits).");

        var topDown = heightRaw < 0;
        var height = Math.Abs(heightRaw);
        var stride = ((width * bits + 31) / 32) * 4;
        if (pixelOffset < 54 || pixelOffset + (stride * height) > file.Length)
            throw new InvalidOperationException("The BMP pixel data is truncated.");

        var pixels = new byte[width * height * 4];
        var rowBytes = width * 4;
        for (var y = 0; y < height; y++)
        {
            var sourceRow = topDown ? y : height - 1 - y;
            Buffer.BlockCopy(file, pixelOffset + (sourceRow * stride), pixels, y * rowBytes, rowBytes);
        }

        return new CapturedImage(width, height, pixels);
    }

    private static void WriteInt(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    private static void WriteShort(byte[] buffer, int offset, short value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }
}
