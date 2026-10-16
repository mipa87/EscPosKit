using ESCPOS_NET.Imaging;

namespace ESCPOS_NET.Tests.Imaging;

/// <summary>
/// Compares the rasterizer with 1bpp outputs captured from the previous implementation
/// (ImageSharp 2.1). Dithered images cannot be compared pixel by pixel (a 1-level luminance
/// difference moves dots without changing what is printed), so the primary metric is ink density
/// per 8x8 block; logos reproduce closely enough to carry a pixel-agreement floor as well.
/// </summary>
public class GoldenComparisonTests
{
    // Frozen on 2026-10-16 from the measured maxima plus a margin: mean block 0.0216 (pd-logo-100),
    // single block 0.375 (kitten.jpg, a hard edge in a photo), ink 0.0037. Logos reach 96-99 %
    // pixel agreement, photos 69-75 % because the JPEG decoder and dither details differ.
    private const double MaxMeanBlockDensityDifference = 0.04;
    private const double MaxSingleBlockDensityDifference = 0.43;
    private const double MaxOverallInkDifference = 0.01;

    // The block metric is nearly blind to a 1 px shift. Logos reproduce almost exactly, so they
    // also carry a pixel-agreement floor (measured minimum 96.3 %, pd-logo-100 at native size).
    private const double MinLogoPixelAgreement = 0.95;

    public static TheoryData<string, int?> Cases()
    {
        var data = new TheoryData<string, int?>();
        foreach (var width in new[] { 100, 200, 300, 400, 500, 600 })
        {
            data.Add($"pd-logo-{width}.png", null);
        }

        data.Add("pd-logo-600.png", 300);
        data.Add("abe-lincoln.png", 300);
        for (var width = 296; width <= 303; width++)
        {
            data.Add("pd-logo-300.png", width);
        }

        data.Add("pd-logo-300.jpg", null);
        data.Add("pd-logo-300.bmp", null);
        data.Add("pd-logo-300.gif", null);
        data.Add("kitten.jpg", 500);
        data.Add("Portal_Companion_Cube.jpg", 500);
        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void RasterizerMatchesGoldenOutput(string image, int? maxWidth)
    {
        var golden = ReadGolden(image, maxWidth);
        var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Images", image));

        var actual = ImageRasterizer.FromEncoded(bytes, maxWidth: maxWidth);

        Assert.Equal((golden.Width, golden.Height), (actual.Width, actual.Height));

        var (meanBlock, maxBlock) = BlockDensityDifference(golden, actual);
        var inkDifference = Math.Abs(InkRatio(golden) - InkRatio(actual));
        var pixelAgreement = PixelAgreement(golden, actual);

        if (image.StartsWith("pd-logo", StringComparison.Ordinal))
        {
            Assert.True(
                pixelAgreement >= MinLogoPixelAgreement,
                $"{image} @{maxWidth?.ToString() ?? "native"}: pixel agreement {pixelAgreement:P2}"
            );
        }

        Assert.True(
            meanBlock <= MaxMeanBlockDensityDifference
                && maxBlock <= MaxSingleBlockDensityDifference
                && inkDifference <= MaxOverallInkDifference,
            $"{image} @{maxWidth?.ToString() ?? "native"}: mean block {meanBlock:F4}, "
                + $"max block {maxBlock:F3}, ink diff {inkDifference:F4}, "
                + $"pixel agreement {pixelAgreement:P2}"
        );
    }

    [Fact]
    public void PixelAgreementFloor_CatchesAOnePixelShift()
    {
        var golden = ReadGolden("pd-logo-300.png", null);

        var shifted = ShiftRightByOnePixel(golden);

        Assert.True(PixelAgreement(golden, shifted) < MinLogoPixelAgreement);
    }

    private static MonochromeBitmap ShiftRightByOnePixel(MonochromeBitmap source)
    {
        var data = new byte[source.Data.Length];
        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 1; x < source.Width; x++)
            {
                if (source.IsBlack(x - 1, y))
                {
                    data[(y * source.BytesPerRow) + (x / 8)] |= (byte)(0x80 >> (x % 8));
                }
            }
        }

        return new MonochromeBitmap(source.Width, source.Height, data);
    }

    private static MonochromeBitmap ReadGolden(string image, int? maxWidth)
    {
        var name =
            $"{image.Replace('.', '_')}_{(maxWidth.HasValue ? maxWidth.Value.ToString() : "native")}.bin";
        using var reader = new BinaryReader(
            File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Golden", name))
        );
        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var data = reader.ReadBytes(((width + 7) / 8) * height);
        return new MonochromeBitmap(width, height, data);
    }

    private static double InkRatio(MonochromeBitmap bitmap)
    {
        var black = 0L;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                black += bitmap.IsBlack(x, y) ? 1 : 0;
            }
        }

        return black / (double)(bitmap.Width * bitmap.Height);
    }

    private static double PixelAgreement(MonochromeBitmap a, MonochromeBitmap b)
    {
        var same = 0L;
        for (var y = 0; y < a.Height; y++)
        {
            for (var x = 0; x < a.Width; x++)
            {
                same += a.IsBlack(x, y) == b.IsBlack(x, y) ? 1 : 0;
            }
        }

        return same / (double)(a.Width * a.Height);
    }

    private static (double Mean, double Max) BlockDensityDifference(
        MonochromeBitmap a,
        MonochromeBitmap b
    )
    {
        const int block = 8;
        var sum = 0.0;
        var max = 0.0;
        var count = 0;
        for (var by = 0; by < a.Height; by += block)
        {
            for (var bx = 0; bx < a.Width; bx += block)
            {
                var blackA = 0;
                var blackB = 0;
                var pixels = 0;
                for (var y = by; y < Math.Min(by + block, a.Height); y++)
                {
                    for (var x = bx; x < Math.Min(bx + block, a.Width); x++)
                    {
                        blackA += a.IsBlack(x, y) ? 1 : 0;
                        blackB += b.IsBlack(x, y) ? 1 : 0;
                        pixels++;
                    }
                }

                var difference = Math.Abs(blackA - blackB) / (double)pixels;
                sum += difference;
                max = Math.Max(max, difference);
                count++;
            }
        }

        return (sum / count, max);
    }
}
