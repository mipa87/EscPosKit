using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Imaging;
using ESCPOS_NET.Utilities;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    private static byte[] GetImageHeader(int commandLength)
    {
        var lengths = new byte[4];
        var i = 0;
        while (commandLength > 0)
        {
            lengths[i] = (byte)(commandLength & 0xFF);
            commandLength >>= 8;
            i++;
        }

        if (i >= 3)
        {
            return
            [
                Cmd.GS,
                Images.ImageCmd8,
                Images.ImageCmdL,
                lengths[0],
                lengths[1],
                lengths[2],
                lengths[3],
            ];
        }

        return [Cmd.GS, Images.ImageCmdParen, Images.ImageCmdL, lengths[0], lengths[1]];
    }

    /* Image Commands */
    public virtual byte[] SetImageDensity(bool isHiDPI)
    {
        var builder = new ByteArrayBuilder();
        var dpiSetting = isHiDPI ? (byte)0x33 : (byte)0x32; // TODO: is this right??
        byte[] baseCommand = [0x30, 0x31, dpiSetting, dpiSetting];
        builder.Append(GetImageHeader(baseCommand.Length));
        builder.Append(baseCommand);
        return builder.ToArray();
    }

    public virtual byte[] BufferImage(
        byte[] image,
        int maxWidth = -1,
        bool isLegacy = false,
        int color = 1
    )
    {
        var bitmap = ImageRasterizer.FromEncoded(image, maxWidth: maxWidth == -1 ? null : maxWidth);
        return BufferImage(bitmap, isLegacy, color);
    }

    public virtual byte[] BufferImage(MonochromeBitmap bitmap, bool isLegacy = false, int color = 1)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        // The ESC/POS headers carry 16-bit sizes.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmap.Width, 65535, nameof(bitmap));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmap.Height, 65535, nameof(bitmap));

        var colorByte = ColorByte(color);

        var width = bitmap.Width;
        var height = bitmap.Height;
        var heightL = (byte)height;
        var heightH = (byte)(height >> 8);

        var imageCommand = new ByteArrayBuilder();
        if (isLegacy)
        {
            var byteWidth = bitmap.BytesPerRow;
            var widthL = (byte)byteWidth;
            var widthH = (byte)(byteWidth >> 8);
            imageCommand.Append([
                Cmd.GS,
                Images.ImageCmdLegacy,
                0x30,
                0x00,
                widthL,
                widthH,
                heightL,
                heightH,
            ]);
        }
        else
        {
            var widthL = (byte)width;
            var widthH = (byte)(width >> 8);
            imageCommand.Append([
                0x30,
                0x70,
                0x30,
                0x01,
                0x01,
                colorByte,
                widthL,
                widthH,
                heightL,
                heightH,
            ]);
        }

        imageCommand.Append(bitmap.Data);

        // Load image to print buffer
        var response = new ByteArrayBuilder();
        var imageCommandBytes = imageCommand.ToArray();
        if (!isLegacy)
        {
            response.Append(GetImageHeader(imageCommandBytes.Length));
        }

        response.Append(imageCommandBytes);
        return response.ToArray();
    }

    public virtual byte[] WriteImageFromBuffer()
    {
        // Print image that's already buffered.
        var response = new ByteArrayBuilder();
        byte[] printCommandBytes = [0x30, 0x32];
        response.Append(GetImageHeader(printCommandBytes.Length));
        response.Append(printCommandBytes);
        return response.ToArray();
    }

    public virtual byte[] PrintImage(
        byte[] image,
        bool isHiDPI,
        bool isLegacy = false,
        int maxWidth = -1,
        int color = 1
    )
    {
        var bitmap = ImageRasterizer.FromEncoded(image, maxWidth: maxWidth == -1 ? null : maxWidth);
        return PrintImage(bitmap, isHiDPI, isLegacy, color);
    }

    public virtual byte[] PrintImage(
        MonochromeBitmap bitmap,
        bool isHiDPI,
        bool isLegacy = false,
        int color = 1
    )
    {
        if (isLegacy)
        {
            return BufferImage(bitmap, isLegacy, color);
        }

        return ByteSplicer.Combine(
            SetImageDensity(isHiDPI),
            BufferImage(bitmap, isLegacy, color),
            WriteImageFromBuffer()
        );
    }

    // Color 1..3 of a raster image as the ESC/POS argument; anything else is color 1.
    private static byte ColorByte(int color) =>
        color switch
        {
            2 => 0x32,
            3 => 0x33,
            _ => 0x31,
        };
}
