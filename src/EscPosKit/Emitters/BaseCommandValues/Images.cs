namespace ESCPOS_NET.Emitters.BaseCommandValues;

public static class Images
{
    public static readonly byte ImageCmdParen = 0x28;
    public static readonly byte ImageCmdLegacy = 0x76;
    public static readonly byte ImageCmd8 = 0x38;
    public static readonly byte ImageCmdL = 0x4C;

    // GS ( L argument m and the fn codes of the stored (NV) image functions.
    public static readonly byte StoredImageMode = 0x30;
    public static readonly byte DeleteAllStoredImages = 0x41;
    public static readonly byte DeleteStoredImage = 0x42;
    public static readonly byte DefineStoredImage = 0x43;
    public static readonly byte PrintStoredImage = 0x45;
    public static readonly byte StoredImageMonochrome = 0x01;
}
