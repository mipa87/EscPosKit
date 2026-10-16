using ESCPOS_NET.Emitters;
using ESCPOS_NET.Imaging;

namespace ESCPOS_NET.Tests.Emitters;

public class ImageCommandTests
{
    private readonly ICommandEmitter _e = new EPSON();

    [Fact]
    public void BufferImage_RasterHeader_CarriesWidthHeightAndColor()
    {
        var bitmap = new MonochromeBitmap(300, 2, new byte[38 * 2]);

        var bytes = _e.BufferImage(bitmap, isLegacy: false, color: 2);

        // The GS ( L header is followed by: 0x30 0x70 0x30 0x01 0x01 color xL xH yL yH
        var payloadStart = bytes.Length - (38 * 2) - 10;
        Assert.Equal(
            [0x30, 0x70, 0x30, 0x01, 0x01, 0x32, 0x2C, 0x01, 0x02, 0x00],
            bytes[payloadStart..(payloadStart + 10)]
        );
    }

    [Fact]
    public void BufferImage_LegacyHeader_UsesByteWidth()
    {
        var bitmap = new MonochromeBitmap(300, 2, new byte[38 * 2]);

        var bytes = _e.BufferImage(bitmap, isLegacy: true);

        Assert.Equal([0x1D, 0x76, 0x30, 0x00, 0x26, 0x00, 0x02, 0x00], bytes[..8]);
        Assert.Equal(8 + (38 * 2), bytes.Length);
    }

    [Theory]
    [InlineData(65536, 1)]
    [InlineData(1, 65536)]
    public void BufferImage_RejectsSizesThatDoNotFitTheHeader(int width, int height)
    {
        var bitmap = new MonochromeBitmap(width, height, new byte[((width + 7) / 8) * height]);

        Assert.Throws<ArgumentOutOfRangeException>(() => _e.BufferImage(bitmap));
    }

    [Fact]
    public void PrintImage_EncodedAndBitmap_ProduceTheSameBytes()
    {
        var png = File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "Images", "pd-logo-300.png")
        );
        var bitmap = ImageRasterizer.FromEncoded(png, maxWidth: 296);

        Assert.Equal(
            _e.PrintImage(png, isHiDPI: true, maxWidth: 296),
            _e.PrintImage(bitmap, isHiDPI: true)
        );
    }
}
