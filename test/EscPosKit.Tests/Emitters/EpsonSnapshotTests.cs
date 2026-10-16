using ESCPOS_NET.Emitters;
using ESCPOS_NET.Imaging;
using ESCPOS_NET.Utilities;
using static ESCPOS_NET.Tests.Emitters.SnapshotHelpers;

namespace ESCPOS_NET.Tests.Emitters;

public class EpsonSnapshotTests
{
    private readonly ICommandEmitter _e = new EPSON();

    [Fact]
    public Task Initialize() => VerifyBytes(_e.Initialize());

    [Fact]
    public Task PrintLine() => VerifyBytes(_e.PrintLine("Hello"));

    [Fact]
    public Task SetStyles_BoldUnderline() =>
        VerifyBytes(_e.SetStyles(PrintStyle.Bold | PrintStyle.Underline));

    [Fact]
    public Task Alignments() =>
        VerifyBytes(ByteSplicer.Combine(_e.LeftAlign(), _e.CenterAlign(), _e.RightAlign()));

    [Fact]
    public Task FeedAndCut() =>
        VerifyBytes(
            ByteSplicer.Combine(
                _e.FeedLines(2),
                _e.FeedDots(10),
                _e.PartialCutAfterFeed(5),
                _e.FullCut()
            )
        );

    [Fact]
    public Task CodePage() => VerifyBytes(_e.CodePage(ESCPOS_NET.Emitters.CodePage.PC852_LATIN2));

    [Fact]
    public Task PrintBarcode_Code128() =>
        VerifyBytes(
            ByteSplicer.Combine(
                _e.SetBarcodeHeightInDots(60),
                _e.SetBarWidth(BarWidth.Default),
                _e.SetBarLabelPosition(BarLabelPrintPosition.Below),
                _e.PrintBarcode(BarcodeType.CODE128, "1234567890")
            )
        );

    [Fact]
    public Task PrintQRCode() => VerifyBytes(_e.PrintQRCode("https://example.com"));

    [Fact]
    public Task CashDrawer() =>
        VerifyBytes(ByteSplicer.Combine(_e.CashDrawerOpenPin2(), _e.CashDrawerOpenPin5()));

    [Fact]
    public Task StatusCommands() =>
        VerifyBytes(
            ByteSplicer.Combine(
                _e.EnableAutomaticStatusBack(),
                _e.RequestOnlineStatus(),
                _e.RequestPaperStatus(),
                _e.DisableAutomaticStatusBack()
            )
        );

    [Fact]
    public Task ImageHeader_LegacyAndRaster()
    {
        var png = File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "Images", "black-16x8.png")
        );

        return VerifyBytes(
            ByteSplicer.Combine(
                _e.PrintImage(png, isHiDPI: true, isLegacy: false),
                _e.PrintImage(png, isHiDPI: false, isLegacy: true)
            )
        );
    }

    [Fact]
    public Task ImageHeader_FromBitmap_MatchesEncodedPath()
    {
        var bitmap = new MonochromeBitmap(16, 8, [.. Enumerable.Repeat((byte)0xFF, 16)]);

        return VerifyBytes(
            ByteSplicer.Combine(
                _e.PrintImage(bitmap, isHiDPI: true, isLegacy: false),
                _e.PrintImage(bitmap, isHiDPI: false, isLegacy: true)
            )
        );
    }
}
