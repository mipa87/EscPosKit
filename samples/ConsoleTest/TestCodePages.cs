using ESCPOS_NET.Emitters;

namespace ESCPOS_NET.ConsoleTest;

public static partial class Tests
{
    public static byte[][] CodePages(ICommandEmitter e, CodePage codePage)
    {
        var test = new List<byte[]>
        {
            e.LeftAlign(),
            e.PrintLine("Empty space = space or non-printable character."),
            e.PrintLine("Each row represents the first hex digit."),
            e.PrintLine("Each column represents the second hex digit."),
            e.PrintLine("For example, row 7, column A, represents"),
            e.PrintLine("the character with hex 0x7A."),
            e.PrintLine(),
            e.PrintLine("Character Table for Code Page:"),
            e.PrintLine(codePage + "  (Page " + ((int)codePage) + ")"),
            e.PrintLine("=========================="),
            e.PrintLine(),
            e.PrintLine("   0 1 2 3 4 5 6 7 8 9 A B C D E F "),
            // Set CodePage to test codepage.
            e.CodePage(codePage),
        };
        for (var d1 = 0; d1 < 0x10; d1++)
        {
            var upperDigit = d1.ToString("x1").ToUpperInvariant();
            test.Add(e.Print(upperDigit + "  "));
            for (var d2 = 0; d2 < 0x10; d2++)
            {
                var digit = d1 * 0x10 + d2;
                if (digit <= 0x20)
                {
                    digit = 0x20;
                }

                test.Add(e.Print((char)digit + " "));
            }
            test.Add(e.PrintLine());
        }
        // Set CodePage back to default
        test.Add(e.CodePage(CodePage.PC437_USA_STANDARD_EUROPE_DEFAULT));
        test.Add(e.PrintLine());
        return [.. test];
    }
}
