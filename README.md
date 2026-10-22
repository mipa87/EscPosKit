# EscPosKit

ESC/POS command generation for Epson-compatible thermal receipt printers, plus optional printer transports. Runs on .NET 10.

EscPosKit continues [lukevp/ESC-POS-.NET](https://github.com/lukevp/ESC-POS-.NET) (NuGet `ESCPOS_NET` 3.0.0), which stopped receiving updates in 2024. The namespaces are still `ESCPOS_NET.*`, so switching is a package swap plus the few changes listed under [Migrating from ESCPOS_NET](#migrating-from-escpos_net-300).

[![EscPosKit on NuGet](https://img.shields.io/nuget/v/EscPosKit?label=EscPosKit)](https://www.nuget.org/packages/EscPosKit) [![EscPosKit.Printers on NuGet](https://img.shields.io/nuget/v/EscPosKit.Printers?label=EscPosKit.Printers)](https://www.nuget.org/packages/EscPosKit.Printers) [![CI](https://github.com/mipa87/EscPosKit/actions/workflows/ci.yml/badge.svg)](https://github.com/mipa87/EscPosKit/actions/workflows/ci.yml)

## Packages

| Package              | Contents                                                                                                                                                                                                            | Dependencies                                                                          |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| `EscPosKit`          | The `EPSON` command emitter: text styles, alignment, code pages and UTF-8 mode, barcodes, QR codes, images, stored (NV) images, cash drawer, paper cut, status requests. Image decoding and dithering are built in. | StbImageSharp                                                                         |
| `EscPosKit.Printers` | Transports that send the bytes: `ImmediateNetworkPrinter`, `NetworkPrinter` (with status events), `SerialPrinter`, `FilePrinter`, `SambaPrinter`, `MemoryPrinter`, `DirectPrinter`.                                 | EscPosKit, SuperSimpleTcp, System.IO.Ports, Microsoft.Extensions.Logging.Abstractions |

You only need `EscPosKit` if you already have a way to get bytes to the printer (a raw print queue, IPP, your own socket).

```shell
dotnet add package EscPosKit
dotnet add package EscPosKit.Printers
```

## Quick start

```csharp
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

var e = new EPSON();
var receipt = ByteSplicer.Combine(
    e.Initialize(),
    e.CenterAlign(),
    e.PrintImage(File.ReadAllBytes("logo.png"), isHiDPI: true, maxWidth: 384),
    e.SetStyles(PrintStyle.Bold | PrintStyle.DoubleHeight),
    e.PrintLine("EXAMPLE STORE"),
    e.SetStyles(PrintStyle.None),
    e.PrintLine("123 Main Street"),
    e.LeftAlign(),
    e.PrintLine(""),
    e.SetStyles(PrintStyle.FontB),
    e.PrintLine("1 x Coffee                    3.50"),
    e.PrintLine("1 x Bagel                     2.20"),
    e.SetStyles(PrintStyle.Bold),
    e.PrintLine("TOTAL                         5.70"),
    e.SetStyles(PrintStyle.None),
    e.PrintLine(""),
    e.PrintQRCode("https://example.com/receipt/123"),
    e.PartialCutAfterFeed(4)
);

// EscPosKit.Printers: network printer, one-shot write
var printer = new ImmediateNetworkPrinter(
    new ImmediateNetworkPrinterSettings { ConnectionString = "192.168.1.50:9100", PrinterName = "Front desk" }
);
await printer.WriteAsync(receipt);
```

`ICommandEmitter` methods return `byte[]`; `ByteSplicer.Combine` concatenates them. Everything the emitter produces is plain ESC/POS, so any transport works.

## Images

`PrintImage(byte[] image, ...)` accepts PNG, JPEG, BMP, GIF, TGA or PSD bytes. The image is fitted into `maxWidth` (keeping the aspect ratio), composited over white, converted to BT.709 luminance and dithered with Stucki error diffusion before it is sent as a raster image. `isHiDPI` selects the printer's high density mode; `isLegacy: true` uses `GS v 0` for printers without `GS ( L` support.

If you already have pixels, skip the decoder:

```csharp
using ESCPOS_NET.Imaging;

MonochromeBitmap bitmap = ImageRasterizer.FromRgba(rgbaBytes, width, height, maxWidth: 384);
var bytes = e.PrintImage(bitmap, isHiDPI: true);
```

`MonochromeBitmap` is 1 bit per pixel, MSB first, rows padded to whole bytes, set bit = black dot. You can also build one yourself and hand it to `PrintImage`, `BufferImage` or `DefineStoredImage`.

### Stored images

Logos can live in the printer's non-volatile memory and be printed by key code, which saves transfer time on every receipt:

```csharp
// once, during setup
await printer.WriteAsync(e.DefineStoredImage(File.ReadAllBytes("logo.png"), "LG", isHiDPI: true, maxWidth: 384));

// on every receipt
await printer.WriteAsync(e.PrintStoredImage("LG"));

// housekeeping
await printer.WriteAsync(e.DeleteStoredImage("LG"));
await printer.WriteAsync(e.DeleteAllStoredImages());
```

Key codes are two printable ASCII characters. `PrintStoredImage` takes an `ImageScale` per axis (`Normal` or `Double`).

## Text and encodings

Strings are encoded with the emitter's `Encoding` property. Thermal printers do not understand Unicode by default: either select a code page that contains your characters and set `Encoding` to match, or switch UTF-8 capable models (TM-T88VI, TM-m30 and newer) to UTF-8 mode:

```csharp
using System.Text;

// Code page mode
await printer.WriteAsync(e.CodePage(CodePage.PC858_EURO));
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // once at startup
e.Encoding = Encoding.GetEncoding(858);

// UTF-8 mode
await printer.WriteAsync(e.SelectCharacterEncoding(CharacterEncoding.Utf8));
e.Encoding = Encoding.UTF8;
```

`SetStyles` sends underline through `ESC -` instead of bit 7 of `ESC !`, so underline works in UTF-8 mode too. `UnderlineMode(bool)` toggles it on its own, like `ReverseMode` and `UpsideDownMode`.

## Implemented commands

Every method of `ICommandEmitter` and the ESC/POS command it emits. Commands not listed here are not implemented; see Epson's ESC/POS command reference for the full protocol.

| Group        | Method                                                                                | Command                                                        |
| ------------ | ------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| Print        | `Print`, `PrintLine`                                                                  | text in the current encoding, `LF`                             |
| Print        | `FeedLines`, `FeedLinesReverse`                                                       | `ESC d`, `ESC e`                                               |
| Print        | `FeedDots`                                                                            | `ESC J`                                                        |
| Line spacing | `ResetLineSpacing`, `SetLineSpacingInDots`                                            | `ESC 2`, `ESC 3`                                               |
| Character    | `SetStyles`                                                                           | `ESC !` (font, bold, size), plus `ESC -` when underline is set |
| Character    | `UnderlineMode`, `ReverseMode`, `UpsideDownMode`                                      | `ESC -`, `GS B`, `ESC {`                                       |
| Character    | `LeftAlign`, `CenterAlign`, `RightAlign`                                              | `ESC a`                                                        |
| Character    | `RightCharacterSpacing`                                                               | `ESC SP`                                                       |
| Character    | `CodePage`, `SelectCharacterEncoding`                                                 | `ESC t`, `FS ( C`                                              |
| Character    | `Color`                                                                               | `ESC r`                                                        |
| Operational  | `Initialize`, `Enable`, `Disable`                                                     | `ESC @`, `ESC =`                                               |
| Operational  | `ExecuteTestPrint`                                                                    | `GS ( A`                                                       |
| Cash drawer  | `CashDrawerOpenPin2`, `CashDrawerOpenPin5`                                            | `ESC p`                                                        |
| Paper cut    | `FullCut`, `PartialCut`, `FullCutAfterFeed`, `PartialCutAfterFeed`                    | `GS V`                                                         |
| Image        | `SetImageDensity`                                                                     | `GS ( L` fn 49                                                 |
| Image        | `BufferImage`, `WriteImageFromBuffer`                                                 | `GS ( L` / `GS 8 L` fn 112 and fn 50; `GS v 0` with `isLegacy` |
| Image        | `PrintImage`                                                                          | density, buffer and print in one call                          |
| Image        | `DefineStoredImage`, `PrintStoredImage`, `DeleteStoredImage`, `DeleteAllStoredImages` | `GS ( L` fn 67, 69, 66, 65                                     |
| Status       | `EnableAutomaticStatusBack`, `DisableAutomaticStatusBack`                             | `GS a`                                                         |
| Status       | `EnableAutomaticInkStatusBack`, `DisableAutomaticInkStatusBack`                       | `GS j`                                                         |
| Status       | `RequestOnlineStatus`                                                                 | `DLE EOT`                                                      |
| Status       | `RequestPaperStatus`, `RequestDrawerStatus`, `RequestInkStatus`                       | `GS r`                                                         |
| Barcode      | `PrintBarcode`                                                                        | `GS k`                                                         |
| Barcode      | `SetBarcodeHeightInDots`, `SetBarWidth`, `SetBarLabelPosition`, `SetBarLabelFontB`    | `GS h`, `GS w`, `GS H`, `GS f`                                 |
| 2D code      | `PrintQRCode`, `Print2DCode`                                                          | `GS ( k` (QR Code model 1, model 2, Micro QR; PDF417)          |

## Transports (EscPosKit.Printers)

| Printer                          | Use for                                                                                                        |
| -------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `ImmediateNetworkPrinter`        | TCP port 9100, connect-write-disconnect per call. The simplest option; recommended for mobile and server apps. |
| `NetworkPrinter`                 | Persistent TCP connection with `StatusChanged` events (paper, cover, drawer) via automatic status back.        |
| `SerialPrinter`                  | Serial and USB printers exposed as COM ports or `/dev/tty*`.                                                   |
| `FilePrinter`                    | Device files such as `/dev/usb/lp0` on Linux.                                                                  |
| `SambaPrinter`                   | Windows printer shares.                                                                                        |
| `MemoryPrinter` | Collects the bytes in memory, for tests or for your own transport. |
| `DirectPrinter` | Fluent wrapper over an emitter and a printer: chain commands and send them with one call. |

The transports are carried over from the original project. Network, serial and Samba transports are verified by compilation and unit tests here, not on hardware; the command emitter is verified on Epson TM-T88 and TM-m30 printers.

`NetworkPrinter` keeps the connection open and raises `StatusChanged` once automatic status back is enabled:

```csharp
var printer = new NetworkPrinter(new NetworkPrinterSettings { ConnectionString = "192.168.1.50:9100" });
printer.StatusChanged += (sender, args) =>
{
    var status = (PrinterStatusEventArgs)args;
    Console.WriteLine($"Online: {status.IsPrinterOnline}, paper out: {status.IsPaperOut}, cover open: {status.IsCoverOpen}");
};
printer.Write(ByteSplicer.Combine(e.Initialize(), e.Enable(), e.EnableAutomaticStatusBack()));
```

USB printers are used through a virtual COM port. On Linux and macOS the OS exposes them as a serial device; on Windows install the vendor's virtual COM port driver or use a USB-to-serial cable. The .NET serial port implementation only supports COM ports 1 to 8 on Windows.

## Migrating from ESCPOS_NET 3.0.0

- Replace the `ESCPOS_NET` package with `EscPosKit`; add `EscPosKit.Printers` if you used the printer classes. Namespaces are unchanged.
- Target framework is `net10.0` only.
- `SixLabors.ImageSharp` is no longer referenced. The `ToSingleBitPixelByteArray` extension in the `SixLabors.ImageSharp` namespace is gone; use `ImageRasterizer`.
- `SetStyles(PrintStyle.Underline)` now emits `ESC ! n` without bit 7 followed by `ESC - 1`. The printed result is the same.
- `Logging` and `BaseLogger` moved to `EscPosKit.Printers`; the emitter package does not log.
- `TCPConnection.Connected` and `Disconnected` use `SuperSimpleTcp.ConnectionEventArgs` (SuperSimpleTcp 3).
- New: `SelectCharacterEncoding`, `UnderlineMode`, `ExecuteTestPrint`, `DefineStoredImage`, `PrintStoredImage`, `DeleteStoredImage`, `DeleteAllStoredImages`, and `MonochromeBitmap` overloads of `BufferImage` and `PrintImage`.

## Building

Requires the .NET SDK pinned in `global.json`. `dotnet build EscPosKit.slnx`, `dotnet test EscPosKit.slnx`. Formatting is enforced by CSharpier and `dotnet format whitespace` (`dotnet tool restore` first). The console sample in `samples/ConsoleTest` prints test pages to a real printer.

Releases are cut by pushing a tag `vX.Y.Z` (or `vX.Y.Z-rc.N` for a prerelease); the version comes from the tag through MinVer and the workflow publishes both packages to nuget.org.

## Credits and license

Originally written by Luke Paireepinart and the contributors of [ESC-POS-.NET](https://github.com/lukevp/ESC-POS-.NET). MIT License, see [LICENSE](https://github.com/mipa87/EscPosKit/blob/main/LICENSE).
