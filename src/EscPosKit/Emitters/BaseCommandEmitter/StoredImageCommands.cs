using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Imaging;
using ESCPOS_NET.Utilities;

namespace ESCPOS_NET.Emitters;

public abstract partial class BaseCommandEmitter : ICommandEmitter
{
    /* Stored Image Commands (GS ( L fn 65, 66, 67, 69): images kept in the printer's NV memory */

    public virtual byte[] DefineStoredImage(
        byte[] image,
        string keyCode,
        bool isHiDPI,
        int maxWidth = -1,
        int color = 1
    )
    {
        ParseKeyCode(keyCode);
        var bitmap = ImageRasterizer.FromEncoded(image, maxWidth: maxWidth == -1 ? null : maxWidth);
        return DefineStoredImage(bitmap, keyCode, isHiDPI, color);
    }

    // fn 67: store a monochrome image under the key code.
    public virtual byte[] DefineStoredImage(
        MonochromeBitmap bitmap,
        string keyCode,
        bool isHiDPI,
        int color = 1
    )
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        var (kc1, kc2) = ParseKeyCode(keyCode);
        // The record carries 16-bit sizes.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmap.Width, 65535, nameof(bitmap));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitmap.Height, 65535, nameof(bitmap));

        var record = new ByteArrayBuilder();
        record.Append([
            Images.StoredImageMode,
            Images.DefineStoredImage,
            Images.StoredImageRaster,
            kc1,
            kc2,
            Images.StoredImageMonochrome,
            (byte)bitmap.Width,
            (byte)(bitmap.Width >> 8),
            (byte)bitmap.Height,
            (byte)(bitmap.Height >> 8),
            ColorByte(color),
        ]);
        record.Append(bitmap.Data);

        return ByteSplicer.Combine(SetImageDensity(isHiDPI), WithImageHeader(record.ToArray()));
    }

    // fn 69: print the stored image, each axis at normal or double size.
    public virtual byte[] PrintStoredImage(
        string keyCode,
        ImageScale horizontal = ImageScale.Normal,
        ImageScale vertical = ImageScale.Normal
    )
    {
        var (kc1, kc2) = ParseKeyCode(keyCode);
        if (!Enum.IsDefined(horizontal))
        {
            throw new ArgumentOutOfRangeException(nameof(horizontal));
        }

        if (!Enum.IsDefined(vertical))
        {
            throw new ArgumentOutOfRangeException(nameof(vertical));
        }

        return WithImageHeader([
            Images.StoredImageMode,
            Images.PrintStoredImage,
            kc1,
            kc2,
            (byte)horizontal,
            (byte)vertical,
        ]);
    }

    // fn 66: delete the stored image with the key code.
    public virtual byte[] DeleteStoredImage(string keyCode)
    {
        var (kc1, kc2) = ParseKeyCode(keyCode);
        return WithImageHeader([Images.StoredImageMode, Images.DeleteStoredImage, kc1, kc2]);
    }

    // fn 65: delete every stored image; the function takes the literal "CLR" as its argument.
    public virtual byte[] DeleteAllStoredImages() =>
        WithImageHeader([Images.StoredImageMode, Images.DeleteAllStoredImages, .. "CLR"u8]);

    private static byte[] WithImageHeader(byte[] payload) =>
        ByteSplicer.Combine(GetImageHeader(payload.Length), payload);

    // Key codes are two characters in 32..126 (kc1, kc2).
    private static (byte, byte) ParseKeyCode(string keyCode)
    {
        ArgumentNullException.ThrowIfNull(keyCode);
        if (keyCode.Length != 2 || keyCode.Any(c => c < 0x20 || c > 0x7E))
        {
            throw new ArgumentException(
                "A key code is exactly two printable ASCII characters.",
                nameof(keyCode)
            );
        }

        return ((byte)keyCode[0], (byte)keyCode[1]);
    }
}
