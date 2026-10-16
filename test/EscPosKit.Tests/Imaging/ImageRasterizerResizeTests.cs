using ESCPOS_NET.Imaging;

namespace ESCPOS_NET.Tests.Imaging;

public class ImageRasterizerResizeTests
{
    private static byte[] SolidRgba(int width, int height, byte value)
    {
        var rgba = new byte[width * height * 4];
        for (var i = 0; i < rgba.Length; i += 4)
        {
            rgba[i] = value;
            rgba[i + 1] = value;
            rgba[i + 2] = value;
            rgba[i + 3] = 255;
        }

        return rgba;
    }

    private static byte[] VerticalStripes(int width, int height, int stripeWidth)
    {
        var rgba = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var value = (x / stripeWidth) % 2 == 0 ? (byte)0 : (byte)255;
                var p = ((y * width) + x) * 4;
                rgba[p] = value;
                rgba[p + 1] = value;
                rgba[p + 2] = value;
                rgba[p + 3] = 255;
            }
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
                black += bitmap.IsBlack(x, y) ? 1 : 0;
            }
        }

        return black / (double)(bitmap.Width * bitmap.Height);
    }

    // Dimensions captured from the golden files of the previous implementation (bankers rounding).
    [Theory]
    [InlineData(600, 193, 300, null, 300, 96)]
    [InlineData(600, 375, 500, null, 500, 312)]
    [InlineData(555, 819, 300, null, 300, 443)]
    [InlineData(931, 524, 500, null, 500, 281)]
    [InlineData(300, 96, 296, null, 296, 95)]
    [InlineData(300, 96, 299, null, 299, 96)]
    [InlineData(300, 96, 301, null, 301, 96)]
    [InlineData(300, 96, 302, null, 302, 97)]
    [InlineData(300, 96, 303, null, 303, 97)]
    [InlineData(300, 96, null, 48, 150, 48)]
    [InlineData(300, 96, 150, 96, 150, 48)]
    [InlineData(300, 96, null, null, 300, 96)]
    public void FitSize_MatchesGoldenDimensions(
        int width,
        int height,
        int? maxWidth,
        int? maxHeight,
        int expectedWidth,
        int expectedHeight
    )
    {
        var (w, h) = ImageRasterizer.FitSize(width, height, maxWidth, maxHeight);

        Assert.Equal((expectedWidth, expectedHeight), (w, h));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxWidth_BelowOne_Throws(int maxWidth)
    {
        var rgba = SolidRgba(4, 4, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ImageRasterizer.FromRgba(rgba, 4, 4, maxWidth: maxWidth)
        );
    }

    [Fact]
    public void Downscale_OfSolidBlack_StaysSolidBlack()
    {
        var result = ImageRasterizer.FromRgba(SolidRgba(600, 193, 0), 600, 193, maxWidth: 300);

        Assert.Equal(300, result.Width);
        Assert.Equal(96, result.Height);
        for (var y = 0; y < result.Height; y++)
        {
            for (var x = 0; x < result.Width; x++)
            {
                Assert.True(result.IsBlack(x, y), $"({x},{y}) is white");
            }
        }
    }

    [Fact]
    public void Upscale_OfSolidWhite_StaysSolidWhite()
    {
        // Catmull-Rom overshoots at edges; without clamping a white image could gain black dots.
        var result = ImageRasterizer.FromRgba(SolidRgba(300, 96, 255), 300, 96, maxWidth: 303);

        Assert.Equal(303, result.Width);
        Assert.Equal(97, result.Height);
        Assert.All(result.Data, b => Assert.Equal(0, b));
    }

    [Fact]
    public void HardEdge_DoesNotOvershootIntoNeighbours()
    {
        // Left half black, right half white, downscaled 2x: beyond a 2 px transition band the
        // white half must stay fully white and the black half fully black.
        var rgba = VerticalStripes(64, 8, 32);

        var result = ImageRasterizer.FromRgba(rgba, 64, 8, maxWidth: 32);

        for (var y = 0; y < result.Height; y++)
        {
            for (var x = 0; x < 14; x++)
            {
                Assert.True(result.IsBlack(x, y), $"({x},{y}) should be black");
            }

            for (var x = 18; x < 32; x++)
            {
                Assert.False(result.IsBlack(x, y), $"({x},{y}) should be white");
            }
        }
    }

    [Fact]
    public void Downscale_OfFineStripes_BecomesMidGray()
    {
        // 1 px black / 1 px white stripes shrunk 2x average to mid gray, which dithers to ~50 %.
        var rgba = VerticalStripes(128, 64, 1);

        var result = ImageRasterizer.FromRgba(rgba, 128, 64, maxWidth: 64);

        Assert.InRange(BlackRatio(result), 0.40, 0.60);
    }
}
