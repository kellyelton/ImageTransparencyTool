using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageTransparencyTool
{
    public static class ExtensionMethods
    {
        public static void ChromoKey(this Bitmap image, Color color, float percent) {

            if (Bitmap.GetPixelFormatSize(image.PixelFormat) != 32) throw new InvalidOperationException();

            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                image.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * image.Height;
            if (Math.Abs(bmpData.Stride) * image.Height > int.MaxValue) throw new InvalidOperationException();

            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            int r = 0;
            int g = 1;
            int b = 2;
            int a = 3;

            for(var i = 0; i < rgbValues.Length; i+=4) {
                Chrono(color, percent, ref rgbValues[r], ref rgbValues[g], ref rgbValues[b], ref rgbValues[a]);
                r += 4;
                g += 4;
                b += 4;
                a += 4;
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            image.UnlockBits(bmpData);
        }

        private static void Chrono(Color color, float percent, ref byte red, ref byte blue, ref byte green, ref byte alpha) {
            if (percent < 0) throw new ArgumentOutOfRangeException(nameof(percent), percent, "Must be between 0 and 100");
            if (percent > 100) throw new ArgumentOutOfRangeException(nameof(percent), percent, "Must be between 0 and 100");
            percent = percent / 100;

            var rdist = Byte.MaxValue - Math.Abs(color.R - red);
            var gdist = Byte.MaxValue - Math.Abs(color.G - green);
            var bdist = Byte.MaxValue - Math.Abs(color.B - blue);
            var total = (rdist + gdist + bdist) * percent;
            total = (Byte.MaxValue * 3) - total;

            var avg = (total / 3);

            alpha = (byte)avg;
        }

        public static BitmapImage ToBitmapImage(this Bitmap image) {
            Application.Current.Dispatcher.VerifyAccess();
            using (MemoryStream strmImg = new MemoryStream()) {
                image.Save(strmImg, ImageFormat.Png);
                strmImg.Flush();
                strmImg.Position = 0;

                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.StreamSource = strmImg;
                myBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                myBitmapImage.DecodePixelWidth = image.Width;
                myBitmapImage.DecodePixelHeight = image.Height;
                myBitmapImage.EndInit();

                return myBitmapImage;
            }
        }
    }
}
