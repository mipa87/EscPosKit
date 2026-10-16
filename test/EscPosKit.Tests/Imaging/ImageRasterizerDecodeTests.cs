using ESCPOS_NET.Imaging;

namespace ESCPOS_NET.Tests.Imaging;

public class ImageRasterizerDecodeTests
{
    private static byte[] Image(string name) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Images", name));

    [Theory]
    [InlineData("pd-logo-300.png", 300, 96)]
    [InlineData("pd-logo-300.jpg", 300, 96)]
    [InlineData("pd-logo-300.bmp", 300, 96)]
    [InlineData("pd-logo-300.gif", 300, 96)]
    [InlineData("abe-lincoln.png", 555, 819)]
    public void FromEncoded_DecodesAllSampleFormats(string name, int width, int height)
    {
        var result = ImageRasterizer.FromEncoded(Image(name));

        Assert.Equal((width, height), (result.Width, result.Height));
    }

    [Fact]
    public void FromEncoded_HonoursMaxWidth()
    {
        var result = ImageRasterizer.FromEncoded(Image("pd-logo-600.png"), maxWidth: 300);

        Assert.Equal((300, 96), (result.Width, result.Height));
    }

    [Fact]
    public void FromEncoded_RejectsGarbage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ImageRasterizer.FromEncoded([1, 2, 3, 4, 5])
        );

        Assert.Equal("image", ex.ParamName);
    }

    // stb_image reads past the end of truncated data as zeros, so the container ends are checked.
    [Theory]
    [InlineData("pd-logo-300.png", 0.5)]
    [InlineData("pd-logo-300.png", 0.9)]
    [InlineData("pd-logo-300.jpg", 0.5)]
    [InlineData("pd-logo-300.jpg", 0.9)]
    [InlineData("kitten.jpg", 0.99)]
    [InlineData("pd-logo-300.bmp", 0.5)]
    [InlineData("pd-logo-300.bmp", 0.9)]
    [InlineData("pd-logo-300.gif", 0.5)]
    [InlineData("pd-logo-300.gif", 0.9)]
    public void FromEncoded_RejectsTruncatedImages(string name, double keep)
    {
        var bytes = Image(name);
        var truncated = bytes[..(int)(bytes.Length * keep)];

        var ex = Assert.Throws<ArgumentException>(() => ImageRasterizer.FromEncoded(truncated));

        Assert.Equal("image", ex.ParamName);
    }

    [Fact]
    public void FromEncoded_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ImageRasterizer.FromEncoded(null));
    }
}
