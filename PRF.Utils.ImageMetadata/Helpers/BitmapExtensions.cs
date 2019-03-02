using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;


namespace PRF.Utils.ImageMetadata.Helpers
{
    /// <summary>
    /// Méthode de conversion des bitmap en BitmapSource
    /// </summary>
    internal static class BitmapExtensions
    {
        private static readonly Dictionary<PixelFormat, System.Windows.Media.PixelFormat> _pixelFormatsConverter = new Dictionary<PixelFormat, System.Windows.Media.PixelFormat>
        {
            {PixelFormat.Format24bppRgb, PixelFormats.Bgr24},
            {PixelFormat.Format32bppRgb, PixelFormats.Bgr32},
            {PixelFormat.Format32bppArgb, PixelFormats.Bgra32}
        };

        /// <summary>
        /// Créer un BitmapSource à partir d'un Bitmap
        /// </summary>
        public static BitmapSource ToBitmapSource(this Bitmap bmp)
        {
            if (!_pixelFormatsConverter.TryGetValue(bmp.PixelFormat, out var pxf))
            {
                throw new NotSupportedException($"PixelFormat {bmp.PixelFormat} is not supported");
            }

            var bitmapData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, bmp.PixelFormat);

            try
            {
                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width,
                    bitmapData.Height,
                    bmp.HorizontalResolution,
                    bmp.VerticalResolution,
                    pxf,
                    null,
                    bitmapData.Scan0,
                    bitmapData.Stride * bmp.Height,
                    bitmapData.Stride);
                return bitmapSource;
            }
            finally
            {

                bmp.UnlockBits(bitmapData);
            }
        }
    }
}
