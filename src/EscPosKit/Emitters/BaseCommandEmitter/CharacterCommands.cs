using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    /* Character Commands */
    public virtual byte[] SetStyles(PrintStyle style) => [Cmd.ESC, Chars.StyleMode, (byte)style];

    public virtual byte[] LeftAlign() => [Cmd.ESC, Chars.Alignment, (byte)Align.Left];

    public virtual byte[] CenterAlign() => [Cmd.ESC, Chars.Alignment, (byte)Align.Center];

    public virtual byte[] RightAlign() => [Cmd.ESC, Chars.Alignment, (byte)Align.Right];

    public virtual byte[] LeftAlignAlt() => [Cmd.ESC, Chars.Alignment, (byte)Align.LeftAlt];

    public virtual byte[] CenterAlignAlt() => [Cmd.ESC, Chars.Alignment, (byte)Align.CenterAlt];

    public virtual byte[] RightAlignAlt() => [Cmd.ESC, Chars.Alignment, (byte)Align.RightAlt];

    public virtual byte[] RightCharacterSpacing(int spaceCount) =>
        [Cmd.ESC, Chars.RightCharacterSpacing, (byte)spaceCount];

    public virtual byte[] CodePage(CodePage codePage) => [Cmd.ESC, Chars.CodePage, (byte)codePage];

    public virtual byte[] Color(Color color) => [Cmd.ESC, Chars.Color, (byte)color];
}
