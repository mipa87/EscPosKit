using ESCPOS_NET.Emitters;

namespace ESCPOS_NET.Tests.GlobalTests;

public class Barcode
{
    private const string WebsiteString = "https://github.com/lukevp/ESC-POS-.NET/";
    private const string ShorterWebsiteString = "https://github.com/";

    [Theory]
    [InlineData(TwoDimensionCodeType.QRCODE_MODEL1)]
    [InlineData(TwoDimensionCodeType.QRCODE_MODEL2)]
    [InlineData(TwoDimensionCodeType.QRCODE_MICRO)]
    [InlineData(null)]
    public void PrintQRCode_Success(TwoDimensionCodeType? codeType)
    {
        ICommandEmitter e = new EPSON();

        byte[] bytes;
        if (codeType is TwoDimensionCodeType codeTypeValue)
        {
            var data =
                codeTypeValue == TwoDimensionCodeType.QRCODE_MICRO
                    ? ShorterWebsiteString
                    : WebsiteString;
            bytes = e.PrintQRCode(data, codeTypeValue);
        }
        else
        {
            bytes = e.PrintQRCode(WebsiteString);
        }

        Assert.True(bytes.Length > 0);
    }

    [Theory]
    [InlineData(TwoDimensionCodeType.PDF417)]
    public void PrintQRCode_Failure(TwoDimensionCodeType codeType)
    {
        ICommandEmitter e = new EPSON();
        Assert.Throws<ArgumentException>(() => e.PrintQRCode(WebsiteString, codeType));
    }
}
