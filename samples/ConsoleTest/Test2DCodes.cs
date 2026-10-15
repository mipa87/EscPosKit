using ESCPOS_NET.Emitters;

namespace ESCPOS_NET.ConsoleTest;

public static partial class Tests
{
    private const string WebsiteString = "https://github.com/lukevp/ESC-POS-.NET/";

    public static byte[][] TwoDimensionCodes(ICommandEmitter e) =>
        [
            e.PrintLine("PDF417:"),
            e.Print2DCode(TwoDimensionCodeType.PDF417, WebsiteString),
            e.PrintLine(),
            e.PrintLine("PDF417 (TINY):"),
            e.Print2DCode(TwoDimensionCodeType.PDF417, WebsiteString, Size2DCode.TINY),
            e.PrintLine(),
            e.PrintLine("PDF417 (LARGE):"),
            e.Print2DCode(TwoDimensionCodeType.PDF417, WebsiteString, Size2DCode.LARGE),
            e.PrintLine(),
            e.PrintLine("QRCODE MODEL 1:"),
            e.Print2DCode(TwoDimensionCodeType.QRCODE_MODEL1, WebsiteString),
            e.PrintLine(),
            e.PrintLine("QRCODE MODEL 2:"),
            e.PrintQRCode(WebsiteString),
            e.PrintLine(),
            e.PrintLine("QRCODE MICRO:"),
            e.Print2DCode(TwoDimensionCodeType.QRCODE_MICRO, "github.com/lukevp"),
            e.PrintLine(),
            e.PrintLine("QRCODE MODEL 1 (TINY):"),
            e.Print2DCode(TwoDimensionCodeType.QRCODE_MODEL1, WebsiteString, Size2DCode.TINY),
            e.PrintLine(),
            e.PrintLine("QRCODE MODEL 1 (LARGE):"),
            e.Print2DCode(TwoDimensionCodeType.QRCODE_MODEL1, WebsiteString, Size2DCode.LARGE),
            e.PrintLine(),
        ];
}
