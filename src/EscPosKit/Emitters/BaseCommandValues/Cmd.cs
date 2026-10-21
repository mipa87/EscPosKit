namespace ESCPOS_NET.Emitters.BaseCommandValues;

public static class Cmd
{
    public static readonly byte ESC = 0x1B;
    public static readonly byte GS = 0x1D;
    public static readonly byte FS = 0x1C;
    public static readonly byte DLE = 0x10;

    // "(" introduces the parameterised commands such as FS ( C, GS ( A and GS ( L.
    public static readonly byte Paren = 0x28;
}
