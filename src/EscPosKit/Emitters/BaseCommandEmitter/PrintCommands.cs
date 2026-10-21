using System.Text;
using System.Text.RegularExpressions;
using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    public Encoding Encoding { get; set; }

    /* Printing Commands */
    public virtual byte[] Print(string data)
    {
        // Fix OSX or Windows-style newlines
        data = MyRegex().Replace(data, "\n");

        if (Encoding is null)
        {
            // TODO: Sanitize...
            return [.. data.ToCharArray().Select(x => (byte)x)];
        }

        return Encoding.GetBytes(data);
    }

    public virtual byte[] PrintLine(string line)
    {
        if (line == null)
        {
            return Print("\n");
        }

        return Print(line.Replace("\r", string.Empty).Replace("\n", string.Empty) + "\n");
    }

    public virtual byte[] FeedLines(int lineCount) =>
        [Cmd.ESC, Whitespace.FeedLines, (byte)lineCount];

    public virtual byte[] FeedLinesReverse(int lineCount) =>
        [Cmd.ESC, Whitespace.FeedLinesReverse, (byte)lineCount];

    public virtual byte[] FeedDots(int dotCount) => [Cmd.ESC, Whitespace.FeedDots, (byte)dotCount];

    public virtual byte[] ReverseMode(bool enable) =>
        [Cmd.GS, Chars.ReversePrintMode, enable ? (byte)0x01 : (byte)0x00];

    public virtual byte[] UpsideDownMode(bool enable) =>
        [Cmd.ESC, Chars.UpsideDownMode, enable ? (byte)0x01 : (byte)0x00];

    // ESC - n: 1-dot underline on or off.
    public virtual byte[] UnderlineMode(bool enable) =>
        [Cmd.ESC, Chars.UnderlineMode, enable ? (byte)0x01 : (byte)0x00];

    [GeneratedRegex(@"\r\n|\r")]
    private static partial Regex MyRegex();
}
