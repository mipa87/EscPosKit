namespace ESCPOS_NET.Emitters;

public enum TwoDimensionCodeType
{
    PDF417 = 0,
    QRCODE_MODEL1 = 49,
    QRCODE_MODEL2 = 50,
    QRCODE_MICRO = 51,
}

public enum Size2DCode
{
    TINY = 2,
    SMALL = 3,
    NORMAL = 4,
    LARGE = 5,
    EXTRA = 6,
}

public enum CorrectionLevel2DCode
{
    PERCENT_7 = 48,
    PERCENT_15 = 49,
    PERCENT_25 = 50,
    PERCENT_30 = 51,
}
