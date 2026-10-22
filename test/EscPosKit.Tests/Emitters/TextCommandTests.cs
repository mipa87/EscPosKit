using ESCPOS_NET.Emitters;

namespace ESCPOS_NET.Tests.Emitters;

public class TextCommandTests
{
    private readonly ICommandEmitter _e = new EPSON();

    [Theory]
    [InlineData(CharacterEncoding.CodePage, 0x31)]
    [InlineData(CharacterEncoding.Utf8, 0x32)]
    public void SelectCharacterEncoding_IsFsParenC(CharacterEncoding encoding, byte m) =>
        Assert.Equal([0x1C, 0x28, 0x43, 0x02, 0x00, 0x30, m], _e.SelectCharacterEncoding(encoding));

    [Theory]
    [InlineData(true, 0x01)]
    [InlineData(false, 0x00)]
    public void UnderlineMode_IsEscMinus(bool enable, byte n) =>
        Assert.Equal([0x1B, 0x2D, n], _e.UnderlineMode(enable));

    [Fact]
    public void SetStyles_WithoutUnderline_IsEscBangOnly() =>
        Assert.Equal([0x1B, 0x21, 0x08], _e.SetStyles(PrintStyle.Bold));

    [Fact]
    public void SetStyles_None_ClearsEverything() =>
        Assert.Equal([0x1B, 0x21, 0x00], _e.SetStyles(PrintStyle.None));

    [Fact]
    public void SetStyles_WithUnderline_KeepsBit7OutOfEscBang_AndAppendsEscMinus() =>
        Assert.Equal(
            [0x1B, 0x21, 0x08, 0x1B, 0x2D, 0x01],
            _e.SetStyles(PrintStyle.Bold | PrintStyle.Underline)
        );

    [Fact]
    public void SetStyles_AllFlags_NeverCarriesBit7InEscBang()
    {
        var bytes = _e.SetStyles((PrintStyle)0xFF);

        Assert.Equal([0x1B, 0x21, 0x7F, 0x1B, 0x2D, 0x01], bytes);
    }

    [Theory]
    [InlineData(TestPrintPattern.HexDump, 0x31)]
    [InlineData(TestPrintPattern.StatusSheet, 0x32)]
    [InlineData(TestPrintPattern.RollingPattern, 0x33)]
    public void ExecuteTestPrint_IsGsParenA_OnRollPaper(TestPrintPattern pattern, byte m) =>
        Assert.Equal([0x1D, 0x28, 0x41, 0x02, 0x00, 0x32, m], _e.ExecuteTestPrint(pattern));

    [Fact]
    public void StandardMode_IsEscS() => Assert.Equal([0x1B, 0x53], new EPSON().StandardMode());

    [Fact]
    public void ExecuteTestPrint_RejectsUndefinedPattern() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _e.ExecuteTestPrint((TestPrintPattern)0x34)
        );
}
