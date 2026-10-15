using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    /* Operational Commands */
    public virtual byte[] Initialize() => [Cmd.ESC, Ops.Initialize];

    public virtual byte[] Enable() => [Cmd.ESC, Ops.EnableDisable, 1];

    public virtual byte[] Disable() => [Cmd.ESC, Ops.EnableDisable, 0];

    public virtual byte[] StandardMode() => [Cmd.ESC, Ops.StandardMode];
}
