using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    public virtual byte[] ResetLineSpacing() => [Cmd.ESC, Whitespace.ResetLineSpacing];

    public virtual byte[] SetLineSpacingInDots(int dots) =>
        [Cmd.ESC, Whitespace.LineSpacingInDots, (byte)dots];
}
