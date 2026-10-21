using System.Text;
using ESCPOS_NET.Imaging;

namespace ESCPOS_NET.Emitters;

public interface ICommandEmitter
{
    public Encoding Encoding { get; set; }

    /* Print Commands */
    public byte[] PrintLine(string line = null);

    public byte[] Print(string line);

    public byte[] FeedLines(int lineCount);

    public byte[] FeedLinesReverse(int lineCount);

    public byte[] FeedDots(int dotCount);

    /* Line Spacing Commands */
    public byte[] ResetLineSpacing();

    public byte[] SetLineSpacingInDots(int dots);

    /* Operational Commands */
    public byte[] Initialize();

    public byte[] Enable();

    public byte[] Disable();

    public byte[] ExecuteTestPrint(TestPrintPattern pattern);

    /* Cash Drawer Commands */
    public byte[] CashDrawerOpenPin2(int pulseOnTimeMs = 120, int pulseOffTimeMs = 240);

    public byte[] CashDrawerOpenPin5(int pulseOnTimeMs = 120, int pulseOffTimeMs = 240);

    /* Character Commands */
    public byte[] SetStyles(PrintStyle style);

    public byte[] LeftAlign();

    public byte[] RightAlign();

    public byte[] CenterAlign();

    public byte[] ReverseMode(bool activate);

    public byte[] RightCharacterSpacing(int spaceCount);

    public byte[] UpsideDownMode(bool activate);

    public byte[] UnderlineMode(bool enable);

    public byte[] CodePage(CodePage codePage);

    public byte[] SelectCharacterEncoding(CharacterEncoding encoding);

    public byte[] Color(Color color);

    /* Action Commands */
    public byte[] FullCut();

    public byte[] PartialCut();

    public byte[] FullCutAfterFeed(int lineCount);

    public byte[] PartialCutAfterFeed(int lineCount);

    /* Image Commands */
    public byte[] SetImageDensity(bool isHiDPI);

    public byte[] BufferImage(byte[] image, int maxWidth, bool isLegacy = false, int color = 1);

    public byte[] BufferImage(MonochromeBitmap bitmap, bool isLegacy = false, int color = 1);

    public byte[] WriteImageFromBuffer();

    public byte[] PrintImage(
        MonochromeBitmap bitmap,
        bool isHiDPI,
        bool isLegacy = false,
        int color = 1
    );

    public byte[] PrintImage(
        byte[] image,
        bool isHiDPI,
        bool isLegacy = false,
        int maxWidth = -1,
        int color = 1
    );

    public byte[] DefineStoredImage(
        byte[] image,
        string keyCode,
        bool isHiDPI,
        int maxWidth = -1,
        int color = 1
    );

    public byte[] DefineStoredImage(
        MonochromeBitmap bitmap,
        string keyCode,
        bool isHiDPI,
        int color = 1
    );

    public byte[] PrintStoredImage(
        string keyCode,
        ImageScale horizontal = ImageScale.Normal,
        ImageScale vertical = ImageScale.Normal
    );

    public byte[] DeleteStoredImage(string keyCode);

    public byte[] DeleteAllStoredImages();

    /* Status Commands */
    public byte[] EnableAutomaticStatusBack();

    public byte[] EnableAutomaticInkStatusBack();

    public byte[] DisableAutomaticStatusBack();

    public byte[] DisableAutomaticInkStatusBack();

    public byte[] RequestOnlineStatus();

    public byte[] RequestPaperStatus();

    public byte[] RequestDrawerStatus();

    public byte[] RequestInkStatus();

    /* Barcode Commands */
    public byte[] PrintBarcode(
        BarcodeType type,
        string barcode,
        BarcodeCode code = BarcodeCode.CODE_B
    );

    /* 2D-Code Commands */
    public byte[] PrintQRCode(
        string data,
        TwoDimensionCodeType type = TwoDimensionCodeType.QRCODE_MODEL2,
        Size2DCode size = Size2DCode.NORMAL,
        CorrectionLevel2DCode correction = CorrectionLevel2DCode.PERCENT_7
    );

    public byte[] Print2DCode(
        TwoDimensionCodeType type,
        string data,
        Size2DCode size = Size2DCode.NORMAL,
        CorrectionLevel2DCode correction = CorrectionLevel2DCode.PERCENT_7
    );

    public byte[] SetBarcodeHeightInDots(int height);

    public byte[] SetBarWidth(BarWidth width);

    public byte[] SetBarLabelPosition(BarLabelPrintPosition position);

    public byte[] SetBarLabelFontB(bool fontB);
}
