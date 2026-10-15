using ESCPOS_NET.Emitters;
using ESCPOS_NET.Printers;

namespace ESCPOS_NET.Tests.GlobalTests;

public class DirectPrinterTests
{
    [Fact]
    public void VerifyInternalBufferIsUpdated()
    {
        ICommandEmitter e = new EPSON();
        var directPrinter = new DirectPrinter(e, null);

        directPrinter.Print("ABC").Print("DEF");

        Assert.Equal(new byte[] { 65, 66, 67, 68, 69, 70 }, directPrinter.Buffer);
    }

    [Fact]
    public void VerifyInternalBufferIsCleared()
    {
        ICommandEmitter e = new EPSON();
        var directPrinter = new DirectPrinter(e, null);
        directPrinter.Print("ABC").Print("DEF");

        directPrinter.ClearAll();
        directPrinter.Print("GHI").Print("JKL");

        Assert.Equal(new byte[] { 71, 72, 73, 74, 75, 76 }, directPrinter.Buffer);
    }

    [Fact]
    public void VerifyQRCodeIsSameAsCallingFromEmitter()
    {
        ICommandEmitter e = new EPSON();
        var directPrinter = new DirectPrinter(e, null);

        directPrinter.PrintQRCode("ABC");

        Assert.Equal(e.PrintQRCode("ABC"), directPrinter.Buffer);
    }
}
