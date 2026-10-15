using System.Runtime.CompilerServices;
using System.Text;

namespace ESCPOS_NET.Tests.Emitters;

public static class SnapshotHelpers
{
    // Hex, 32 bytes per line, so snapshot diffs stay readable.
    public static string Hex(byte[] bytes)
    {
        var hex = Convert.ToHexString(bytes);
        var lines = new StringBuilder();
        for (var i = 0; i < hex.Length; i += 64)
        {
            lines.AppendLine(hex.Substring(i, Math.Min(64, hex.Length - i)));
        }

        return lines.ToString();
    }

    // sourceFile is forwarded so Verify resolves the snapshot directory relative to the calling
    // test file, not to this helper; the file name comes from the running xunit test.
    public static SettingsTask VerifyBytes(byte[] bytes, [CallerFilePath] string sourceFile = "") =>
        Verify(Hex(bytes), sourceFile: sourceFile).UseDirectory("../Snapshots");
}
