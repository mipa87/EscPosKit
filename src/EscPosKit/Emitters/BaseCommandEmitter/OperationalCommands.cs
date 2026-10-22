using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    /* Operational Commands */
    public virtual byte[] Initialize() => [Cmd.ESC, Ops.Initialize];

    public virtual byte[] Enable() => [Cmd.ESC, Ops.EnableDisable, 1];

    public virtual byte[] Disable() => [Cmd.ESC, Ops.EnableDisable, 0];

    // GS ( A: print one of the printer's built-in test patterns on roll paper.
    public virtual byte[] ExecuteTestPrint(TestPrintPattern pattern)
    {
        if (!Enum.IsDefined(pattern))
        {
            throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null);
        }

        return
        [
            Cmd.GS,
            Cmd.Paren,
            Ops.TestPrint,
            0x02, // pL
            0x00, // pH
            Ops.TestPrintRollPaper,
            (byte)pattern,
        ];
    }

    public virtual byte[] StandardMode() => [Cmd.ESC, Ops.StandardMode];
}
