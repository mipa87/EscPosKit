using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ESCPOS_NET.Tests.Imaging;

public class GoldenCaptureTests
{
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

    public static string GoldenFileName(string image, int? maxWidth) =>
        $"{image.Replace('.', '_')}_{(maxWidth.HasValue ? maxWidth.Value.ToString() : "native")}.bin";

    [Fact]
    public void RepositoryRootContainsTheSolution() =>
        Assert.True(File.Exists(Path.Combine(RepositoryRoot.Find(), "EscPosKit.slnx")));

    [Theory]
    [MemberData(nameof(Cases))]
    public void CaptureImageSharpOutput(string image, int? maxWidth)
    {
        Assert.SkipUnless(
            Environment.GetEnvironmentVariable("ESCPOSKIT_CAPTURE_GOLDEN") == "1",
            "Golden capture is opt-in; set ESCPOSKIT_CAPTURE_GOLDEN=1."
        );

        var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Images", image));
        using var img = Image.Load<Rgba32>(bytes);
        var data = img.ToSingleBitPixelByteArray(maxWidth: maxWidth);

        var goldenDirectory = Path.Combine(
            RepositoryRoot.Find(),
            "test",
            "EscPosKit.Tests",
            "Golden"
        );
        Directory.CreateDirectory(goldenDirectory);
        using var writer = new BinaryWriter(
            File.Create(Path.Combine(goldenDirectory, GoldenFileName(image, maxWidth)))
        );
        writer.Write(img.Width);
        writer.Write(img.Height);
        writer.Write(data);

        Assert.Equal((img.Width + 7) / 8 * img.Height, data.Length);
    }
}
