using ESCPOS_NET.Imaging;

namespace ESCPOS_NET.Tests.Imaging;

public class ImageRasterizerTests
{
    private static byte[] SolidRgba(int width, int height, byte r, byte g, byte b, byte a)
    {
        var rgba = new byte[width * height * 4];
        for (var i = 0; i < rgba.Length; i += 4)
        {
            rgba[i] = r;
            rgba[i + 1] = g;
            rgba[i + 2] = b;
            rgba[i + 3] = a;
        }

        return rgba;
    }

    private static double BlackRatio(MonochromeBitmap bitmap)
    {
        var black = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.IsBlack(x, y))
                {
                    black++;
                }
            }
        }

        return black / (double)(bitmap.Width * bitmap.Height);
    }

    [Fact]
    public void SolidBlack_SetsAllPixelBits_AndLeavesRowPaddingClear()
    {
        var result = ImageRasterizer.FromRgba(SolidRgba(10, 3, 0, 0, 0, 255), 10, 3);

        Assert.Equal(10, result.Width);
        Assert.Equal(3, result.Height);
        Assert.Equal(2, result.BytesPerRow);
        Assert.Equal(new byte[] { 0xFF, 0xC0, 0xFF, 0xC0, 0xFF, 0xC0 }, result.Data);
    }

    [Fact]
    public void SolidWhite_LeavesAllBitsClear()
    {
        var result = ImageRasterizer.FromRgba(SolidRgba(16, 4, 255, 255, 255, 255), 16, 4);

        Assert.All(result.Data, b => Assert.Equal(0, b));
    }

    [Fact]
    public void FullyTransparent_IsFlattenedToWhite()
    {
        // Black pixels with alpha 0 must print as paper, not ink.
        var result = ImageRasterizer.FromRgba(SolidRgba(16, 4, 0, 0, 0, 0), 16, 4);

        Assert.All(result.Data, b => Assert.Equal(0, b));
    }

    [Fact]
    public void MidGray_IsDitheredToRoughlyHalfBlack()
    {
        var result = ImageRasterizer.FromRgba(SolidRgba(64, 64, 128, 128, 128, 255), 64, 64);

        Assert.InRange(BlackRatio(result), 0.45, 0.55);
    }

    [Fact]
    public void Luminance_UsesBt709Weights()
    {
        // Pure green is bright (0.7152), pure blue is dark (0.0722): green dithers mostly white, blue mostly black.
        var green = ImageRasterizer.FromRgba(SolidRgba(32, 32, 0, 255, 0, 255), 32, 32);
        var blue = ImageRasterizer.FromRgba(SolidRgba(32, 32, 0, 0, 255, 255), 32, 32);

        Assert.InRange(BlackRatio(green), 0.20, 0.36);
        Assert.InRange(BlackRatio(blue), 0.88, 0.98);
    }

    [Fact]
    public void Threshold_ShiftsTheBlackPoint()
    {
        var rgba = SolidRgba(32, 32, 100, 100, 100, 255);

        var low = ImageRasterizer.FromRgba(rgba, 32, 32, threshold: 0.2f);
        var high = ImageRasterizer.FromRgba(rgba, 32, 32, threshold: 0.8f);

        Assert.True(BlackRatio(low) < BlackRatio(high));
    }

    [Theory]
    [InlineData(-0.01f)]
    [InlineData(1.01f)]
    public void Threshold_OutsideZeroToOne_Throws(float threshold)
    {
        var rgba = SolidRgba(4, 4, 0, 0, 0, 255);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ImageRasterizer.FromRgba(rgba, 4, 4, threshold: threshold)
        );
    }

    [Fact]
    public void RgbaLength_MustMatchDimensions()
    {
        Assert.Throws<ArgumentException>(() => ImageRasterizer.FromRgba(new byte[4 * 3], 2, 2));
    }

    [Fact]
    public void MonochromeBitmap_RejectsDataOfWrongLength()
    {
        Assert.Throws<ArgumentException>(() => new MonochromeBitmap(10, 3, new byte[5]));
    }
}
