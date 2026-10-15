using System;

namespace ESCPOS_NET.Emitters
{
    [Flags]
    public enum PrintStyle
    {
        None = 0,
        FontB = 1,
        Proportional = 1 << 1,
        Condensed = 1 << 2,
        Bold = 1 << 3,
        DoubleHeight = 1 << 4,
        DoubleWidth = 1 << 5,
        Italic = 1 << 6,
        Underline = 1 << 7,
    }
}
