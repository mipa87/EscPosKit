using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Utilities;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    /* Character Commands */
    // ESC ! carries font, bold and size. Underline goes through ESC - instead of bit 7 of ESC !,
    // because in UTF-8 mode that bit is read as part of a multibyte character. Both commands
    // control the same underline state, the last one received wins.
    public virtual byte[] SetStyles(PrintStyle style)
    {
        byte[] styleMode = [Cmd.ESC, Chars.StyleMode, (byte)(style & ~PrintStyle.Underline)];
        return style.HasFlag(PrintStyle.Underline)
            ? ByteSplicer.Combine(styleMode, UnderlineMode(true))
            : styleMode;
    }

    public virtual byte[] LeftAlign() => [Cmd.ESC, Chars.Alignment, (byte)Align.Left];

    public virtual byte[] CenterAlign() => [Cmd.ESC, Chars.Alignment, (byte)Align.Center];

    public virtual byte[] RightAlign() => [Cmd.ESC, Chars.Alignment, (byte)Align.Right];

    public virtual byte[] LeftAlignAlt() => [Cmd.ESC, Chars.Alignment, (byte)Align.LeftAlt];

    public virtual byte[] CenterAlignAlt() => [Cmd.ESC, Chars.Alignment, (byte)Align.CenterAlt];

    public virtual byte[] RightAlignAlt() => [Cmd.ESC, Chars.Alignment, (byte)Align.RightAlt];

    public virtual byte[] RightCharacterSpacing(int spaceCount) =>
        [Cmd.ESC, Chars.RightCharacterSpacing, (byte)spaceCount];

    public virtual byte[] CodePage(CodePage codePage) => [Cmd.ESC, Chars.CodePage, (byte)codePage];

    // FS ( C fn 48: choose between the code page system and UTF-8 for the text that follows.
    public virtual byte[] SelectCharacterEncoding(CharacterEncoding encoding) =>
        [
            Cmd.FS,
            Cmd.Paren,
            Chars.CharacterEncoding,
            0x02, // pL
            0x00, // pH
            Functions.SelectCharacterEncoding,
            (byte)encoding,
        ];

    public virtual byte[] Color(Color color) => [Cmd.ESC, Chars.Color, (byte)color];
}
