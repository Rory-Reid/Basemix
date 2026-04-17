using SkiaSharp;

namespace Basemix.Lib.Media;

public static class ImageProcessor
{
    public record ProcessedImage(
        byte[] Data,
        string Format,
        int OriginalWidth,
        int OriginalHeight,
        int StoredWidth,
        int StoredHeight);

    public static ProcessedImage ResizeImage(Stream input, int maxLongEdge)
    {
        var format = DetectFormat(input) ?? SKEncodedImageFormat.Jpeg;

        using var original = SKBitmap.Decode(input);
        if (original == null)
        {
            throw new ArgumentException("Unable to decode image");
        }

        var originalWidth = original.Width;
        var originalHeight = original.Height;
        var longestEdge = Math.Max(originalWidth, originalHeight);

        SKBitmap toEncode;
        if (longestEdge > maxLongEdge)
        {
            var scale = (double)maxLongEdge / longestEdge;
            var newWidth = (int)Math.Round(originalWidth * scale);
            var newHeight = (int)Math.Round(originalHeight * scale);
            toEncode = original.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default);
            if (toEncode == null)
            {
                throw new InvalidOperationException("Failed to resize image");
            }
        }
        else
        {
            toEncode = original;
        }

        try
        {
            var quality = format == SKEncodedImageFormat.Png ? 100 : 85;

            using var image = SKImage.FromBitmap(toEncode);
            using var data = image.Encode(format, quality);

            return new ProcessedImage(
                Data: data.ToArray(),
                Format: FormatToString(format),
                OriginalWidth: originalWidth,
                OriginalHeight: originalHeight,
                StoredWidth: toEncode.Width,
                StoredHeight: toEncode.Height);
        }
        finally
        {
            if (toEncode != original)
            {
                toEncode.Dispose();
            }
        }
    }

    private static SKEncodedImageFormat? DetectFormat(Stream input)
    {
        if (!input.CanSeek)
        {
            return null;
        }

        var position = input.Position;
        input.Position = 0;
        var header = new byte[12];
        var read = input.Read(header, 0, header.Length);
        input.Position = position;

        if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            return SKEncodedImageFormat.Png;

        if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return SKEncodedImageFormat.Jpeg;

        if (read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            return SKEncodedImageFormat.Webp;

        if (read >= 4 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
            return SKEncodedImageFormat.Gif;

        return null;
    }

    private static string FormatToString(SKEncodedImageFormat format) => format switch
    {
        SKEncodedImageFormat.Png => "png",
        SKEncodedImageFormat.Gif => "gif",
        SKEncodedImageFormat.Webp => "webp",
        _ => "jpeg"
    };
}
