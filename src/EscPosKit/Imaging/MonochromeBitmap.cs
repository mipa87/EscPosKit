namespace ESCPOS_NET.Imaging;

/// <summary>
/// A 1 bit per pixel image as thermal printers expect it: MSB first, each row padded to a
/// whole byte, a set bit is a black dot.
/// </summary>
public sealed class MonochromeBitmap
{
    public MonochromeBitmap(int width, int height, byte[] data)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentNullException.ThrowIfNull(data);

        var expected = ((width + 7) / 8) * height;
        if (data.Length != expected)
        {
            throw new ArgumentException(
                $"Expected {expected} bytes for {width}x{height} pixels, got {data.Length}.",
                nameof(data)
            );
        }

        Width = width;
        Height = height;
        Data = data;
    }

    public int Width { get; }

    public int Height { get; }

    public byte[] Data { get; }

    public int BytesPerRow => (Width + 7) / 8;

    public bool IsBlack(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height);

        return (Data[(y * BytesPerRow) + (x / 8)] & (0x80 >> (x % 8))) != 0;
    }
}
