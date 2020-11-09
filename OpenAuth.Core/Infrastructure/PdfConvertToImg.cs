using Docnet.Core;
using Docnet.Core.Readers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Infrastructure
{
    /// <summary>
    /// PDF转图片
    /// </summary>
    public class PdfConvertToImg
    {
        public static Stream Do(byte[] pdfData)
        {
            using (var library = DocLib.Instance)
            {
                using (var docReader = library.GetDocReader(pdfData, new Docnet.Core.Models.PageDimensions(1080, 1920)))
                {
                    //var count = docReader.GetPageCount();
                    using (var pageReader = docReader.GetPageReader(0))
                    {
                        // Convert PDF page to PNG image
                        var pdfPageStream = ConvertPdfPageToToImage(pageReader);
                        // Save PNG image to storage
                        //using (var fileStream = System.IO.File.Create("../../../FileSource/1.png"))
                        //{
                        //    pdfPageStream.WriteTo(fileStream);
                        //}
                        return pdfPageStream;
                    }
                }
            }
        }

        private static MemoryStream ConvertPdfPageToToImage(IPageReader pageReader)
        {
            var rawBytes = pageReader.GetImage();
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                AddBytes(bmp, rawBytes);
                watch.Stop();
                Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
                var pageStream = new MemoryStream();
                bmp.Save(pageStream, ImageFormat.Png);
                return pageStream;
            }
        }
        private static void AddBytes(Bitmap bitmap, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var pNative = bmpData.Scan0;
            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bitmap.UnlockBits(bmpData);

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color gotColor = bitmap.GetPixel(x, y);
                    if (gotColor.A == 0)
                    {
                        gotColor = Color.FromArgb(255, 255, 255);
                        bitmap.SetPixel(x, y, gotColor);
                    }
                }
            }
        }
    }
}
