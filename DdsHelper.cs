using System.Drawing.Imaging;
using DdsFileTypePlus;
using PaintDotNet;

namespace ERMapGenerator;

public static class DdsHelper
{
    public static byte[] ConvertBitmapToDDS(Bitmap bitmap, BC7CompressionSpeed compressionSpeed = BC7CompressionSpeed.Slow)
    {
        Surface surface = ConvertBitmapToSurface(bitmap);
        using MemoryStream ddsStream = new();
        DdsFile.Save(
            ddsStream,
            DdsFileFormat.BC7,
            DdsErrorMetric.Perceptual,
            compressionSpeed,
            cubeMap: false,
            generateMipmaps: false,
            ResamplingAlgorithm.Bicubic,
            surface,
            progressCallback: null
        );
        return ddsStream.ToArray();
    }

    private static Surface ConvertBitmapToSurface(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        Surface surface = new(width, height);
        BitmapData bmpData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );
        try
        {
            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + (y * stride);
                    for (int x = 0; x < width; x++)
                    {
                        int offset = x * 4;
                        byte b = row[offset];
                        byte g = row[offset + 1];
                        byte r = row[offset + 2];
                        byte a = row[offset + 3];
                        surface[x, y] = ColorBgra.FromBgra(b, g, r, a);
                    }
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }
        return surface;
    }
}