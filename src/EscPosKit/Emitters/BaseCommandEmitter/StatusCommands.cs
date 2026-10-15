using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    /* Status Commands */
    public virtual byte[] EnableAutomaticStatusBack() => [Cmd.GS, Status.AutomaticStatusBack, 0xFF];

    public virtual byte[] EnableAutomaticInkStatusBack() =>
        [Cmd.GS, Status.AutomaticInkStatusBack, 0xFF];

    public virtual byte[] DisableAutomaticStatusBack() =>
        [Cmd.GS, Status.AutomaticStatusBack, 0x00];

    public virtual byte[] DisableAutomaticInkStatusBack() =>
        [Cmd.GS, Status.AutomaticInkStatusBack, 0x00];

    public virtual byte[] RequestOnlineStatus() => [Cmd.DLE, Status.RealtimeStatus, 0x01];

    public virtual byte[] RequestPaperStatus() => [Cmd.GS, Status.RequestStatus, 0x01];

    public virtual byte[] RequestDrawerStatus() => [Cmd.GS, Status.RequestStatus, 0x02];

    public virtual byte[] RequestInkStatus() => [Cmd.GS, Status.RequestStatus, 0x04];
}
