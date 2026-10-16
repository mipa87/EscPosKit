namespace ESCPOS_NET.Imaging;

/// <summary>
/// Turns images into <see cref="MonochromeBitmap"/>: alpha composited over white, BT.709
/// luminance, Stucki error diffusion.
/// </summary>
public static class ImageRasterizer
{
    // Stucki kernel as (dx, dy, weight), divisor 42.
    private static readonly int[,] StuckiKernel =
    {
        { 1, 0, 8 },
        { 2, 0, 4 },
        { -2, 1, 2 },
        { -1, 1, 4 },
        { 0, 1, 8 },
        { 1, 1, 4 },
        { 2, 1, 2 },
        { -2, 2, 1 },
        { -1, 2, 2 },
        { 0, 2, 4 },
        { 1, 2, 2 },
        { 2, 2, 1 },
    };

    public static MonochromeBitmap FromRgba(
        ReadOnlySpan<byte> rgba,
        int width,
        int height,
        int? maxWidth = null,
        int? maxHeight = null,
        float threshold = 0.5f
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(threshold, 0f);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 1f);
        if (rgba.Length != width * height * 4)
        {
            throw new ArgumentException(
                $"Expected {width * height * 4} bytes of RGBA data for {width}x{height} pixels, got {rgba.Length}.",
                nameof(rgba)
            );
        }

        if (maxWidth.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxWidth.Value, 1, nameof(maxWidth));
        }

        if (maxHeight.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxHeight.Value, 1, nameof(maxHeight));
        }

        var luminance = ToLuminance(rgba, width, height);
        var (targetWidth, targetHeight) = FitSize(width, height, maxWidth, maxHeight);
        if (targetWidth != width || targetHeight != height)
        {
            luminance = Resample(luminance, width, height, targetWidth, targetHeight);
            width = targetWidth;
            height = targetHeight;
        }

        var black = Dither(luminance, width, height, threshold * 255f);
        return new MonochromeBitmap(width, height, Pack(black, width, height));
    }

    // Scale by the smaller ratio, round half to even, never below 1 px. Scaling up is allowed.
    internal static (int Width, int Height) FitSize(
        int width,
        int height,
        int? maxWidth,
        int? maxHeight
    )
    {
        if (!maxWidth.HasValue && !maxHeight.HasValue)
        {
            return (width, height);
        }

        var widthRatio = maxWidth.HasValue
            ? maxWidth.Value / (double)width
            : double.PositiveInfinity;
        var heightRatio = maxHeight.HasValue
            ? maxHeight.Value / (double)height
            : double.PositiveInfinity;
        var ratio = Math.Min(widthRatio, heightRatio);

        var targetWidth = Math.Max(1, (int)Math.Round(width * ratio, MidpointRounding.ToEven));
        var targetHeight = Math.Max(1, (int)Math.Round(height * ratio, MidpointRounding.ToEven));
        return (targetWidth, targetHeight);
    }

    // Separable Catmull-Rom. When shrinking, the kernel is widened by the scale factor so every
    // source pixel contributes.
    private static float[] Resample(
        float[] source,
        int width,
        int height,
        int targetWidth,
        int targetHeight
    )
    {
        var horizontal = ResampleAxis(source, width, height, targetWidth, horizontal: true);
        return ResampleAxis(horizontal, targetWidth, height, targetHeight, horizontal: false);
    }

    private static float[] ResampleAxis(
        float[] source,
        int width,
        int height,
        int targetLength,
        bool horizontal
    )
    {
        var sourceLength = horizontal ? width : height;
        var otherLength = horizontal ? height : width;
        var scale = sourceLength / (double)targetLength;
        var support = Math.Max(1.0, scale);
        var radius = 2.0 * support;

        var result = new float[horizontal ? targetLength * height : width * targetLength];

        for (var t = 0; t < targetLength; t++)
        {
            var center = ((t + 0.5) * scale) - 0.5;
            var first = Math.Max(0, (int)Math.Floor(center - radius));
            var last = Math.Min(sourceLength - 1, (int)Math.Ceiling(center + radius));

            var weights = new double[last - first + 1];
            var sum = 0.0;
            for (var s = first; s <= last; s++)
            {
                var w = CatmullRom((s - center) / support);
                weights[s - first] = w;
                sum += w;
            }

            for (var o = 0; o < otherLength; o++)
            {
                var acc = 0.0;
                for (var s = first; s <= last; s++)
                {
                    var index = horizontal ? (o * width) + s : (s * width) + o;
                    acc += source[index] * weights[s - first];
                }

                var value = (float)(acc / sum);
                var target = horizontal ? (o * targetLength) + t : (t * width) + o;
                result[target] = Math.Clamp(value, 0f, 255f);
            }
        }

        return result;
    }

    private static double CatmullRom(double x)
    {
        x = Math.Abs(x);
        if (x < 1.0)
        {
            return (((1.5 * x) - 2.5) * x * x) + 1.0;
        }

        if (x < 2.0)
        {
            return ((((-0.5 * x) + 2.5) * x) - 4.0) * x + 2.0;
        }

        return 0.0;
    }

    // Alpha composited over white, then BT.709 luma on the resulting sRGB values (0..255).
    private static float[] ToLuminance(ReadOnlySpan<byte> rgba, int width, int height)
    {
        var luminance = new float[width * height];
        for (var i = 0; i < luminance.Length; i++)
        {
            var p = i * 4;
            var alpha = rgba[p + 3] / 255f;
            var r = (rgba[p] * alpha) + (255f * (1f - alpha));
            var g = (rgba[p + 1] * alpha) + (255f * (1f - alpha));
            var b = (rgba[p + 2] * alpha) + (255f * (1f - alpha));
            luminance[i] = (0.2126f * r) + (0.7152f * g) + (0.0722f * b);
        }

        return luminance;
    }

    private static bool[] Dither(float[] luminance, int width, int height, float threshold)
    {
        var black = new bool[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = (y * width) + x;
                var value = luminance[i];
                var isBlack = value < threshold;
                black[i] = isBlack;

                var error = value - (isBlack ? 0f : 255f);
                for (var k = 0; k < StuckiKernel.GetLength(0); k++)
                {
                    var nx = x + StuckiKernel[k, 0];
                    var ny = y + StuckiKernel[k, 1];
                    if (nx >= 0 && nx < width && ny < height)
                    {
                        luminance[(ny * width) + nx] += error * StuckiKernel[k, 2] / 42f;
                    }
                }
            }
        }

        return black;
    }

    private static byte[] Pack(bool[] black, int width, int height)
    {
        var bytesPerRow = (width + 7) / 8;
        var data = new byte[bytesPerRow * height];
        for (var y = 0; y < height; y++)
        {
            var rowStart = y * bytesPerRow;
            for (var x = 0; x < width; x++)
            {
                if (black[(y * width) + x])
                {
                    data[rowStart + (x / 8)] |= (byte)(0x80 >> (x % 8));
                }
            }
        }

        return data;
    }
}
