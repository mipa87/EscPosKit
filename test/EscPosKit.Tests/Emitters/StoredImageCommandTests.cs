using ESCPOS_NET.Emitters;
using ESCPOS_NET.Imaging;

namespace ESCPOS_NET.Tests.Emitters;

public class StoredImageCommandTests
{
    private readonly ICommandEmitter _e = new EPSON();

    [Fact]
    public void DeleteAllStoredImages_IsGsParenL_Fn65() =>
        Assert.Equal(
            [0x1D, 0x28, 0x4C, 0x05, 0x00, 0x30, 0x41, 0x43, 0x4C, 0x52],
            _e.DeleteAllStoredImages()
        );

    [Fact]
    public void DeleteStoredImage_IsGsParenL_Fn66_WithKeyCode() =>
        Assert.Equal(
            [0x1D, 0x28, 0x4C, 0x04, 0x00, 0x30, 0x42, 0x4B, 0x43],
            _e.DeleteStoredImage("KC")
        );

    [Fact]
    public void PrintStoredImage_IsGsParenL_Fn69_WithScale() =>
        Assert.Equal(
            [0x1D, 0x28, 0x4C, 0x06, 0x00, 0x30, 0x45, 0x4B, 0x43, 0x01, 0x02],
            _e.PrintStoredImage("KC", ImageScale.Normal, ImageScale.Double)
        );

    [Fact]
    public void PrintStoredImage_DefaultsToNormalScale() =>
        Assert.Equal(
            [0x1D, 0x28, 0x4C, 0x06, 0x00, 0x30, 0x45, 0x4B, 0x43, 0x01, 0x01],
            _e.PrintStoredImage("KC")
        );

    [Fact]
    public void PrintStoredImage_RejectsUndefinedScale() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _e.PrintStoredImage("KC", (ImageScale)3));

    [Fact]
    public void DefineStoredImage_Bitmap_IsDensityThenFn67Record()
    {
        var bitmap = new MonochromeBitmap(300, 2, new byte[38 * 2]);

        var bytes = _e.DefineStoredImage(bitmap, "KC", isHiDPI: true, color: 2);

        // SetImageDensity first, then GS ( L with pL pH covering 11 header bytes + data.
        Assert.Equal(_e.SetImageDensity(true), bytes[..9]);
        var record = bytes[9..];
        var length = 11 + (38 * 2);
        Assert.Equal([0x1D, 0x28, 0x4C, (byte)(length & 0xFF), (byte)(length >> 8)], record[..5]);
        Assert.Equal(
            [0x30, 0x43, 0x30, 0x4B, 0x43, 0x01, 0x2C, 0x01, 0x02, 0x00, 0x32],
            record[5..16]
        );
        Assert.Equal((38 * 2) + 16, record.Length);
    }

    [Fact]
    public void DefineStoredImage_Encoded_MatchesBitmapPath()
    {
        var png = File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "Images", "pd-logo-300.png")
        );
        var bitmap = ImageRasterizer.FromEncoded(png, maxWidth: 296);

        Assert.Equal(
            _e.DefineStoredImage(png, "KC", isHiDPI: false, maxWidth: 296),
            _e.DefineStoredImage(bitmap, "KC", isHiDPI: false)
        );
    }

    [Theory]
    [InlineData(65536, 1)]
    [InlineData(1, 65536)]
    public void DefineStoredImage_RejectsSizesThatDoNotFitTheHeader(int width, int height)
    {
        var bitmap = new MonochromeBitmap(width, height, new byte[((width + 7) / 8) * height]);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _e.DefineStoredImage(bitmap, "KC", isHiDPI: false)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("ABC")]
    [InlineData("é1")]
    [InlineData("A")]
    public void KeyCode_MustBeTwoPrintableAsciiCharacters(string keyCode)
    {
        var bitmap = new MonochromeBitmap(8, 1, new byte[1]);

        Assert.Throws<ArgumentException>(() => _e.DeleteStoredImage(keyCode));
        Assert.Throws<ArgumentException>(() => _e.PrintStoredImage(keyCode));
        Assert.Throws<ArgumentException>(() =>
            _e.DefineStoredImage(bitmap, keyCode, isHiDPI: false)
        );
        Assert.Throws<ArgumentException>(() =>
            _e.DefineStoredImage([1, 2, 3], keyCode, isHiDPI: false)
        );
    }

    [Fact]
    public void KeyCode_Null_Throws()
    {
        var bitmap = new MonochromeBitmap(8, 1, new byte[1]);

        Assert.Throws<ArgumentNullException>(() => _e.DeleteStoredImage(null));
        Assert.Throws<ArgumentNullException>(() => _e.PrintStoredImage(null));
        Assert.Throws<ArgumentNullException>(() =>
            _e.DefineStoredImage(bitmap, null, isHiDPI: false)
        );
    }
}
