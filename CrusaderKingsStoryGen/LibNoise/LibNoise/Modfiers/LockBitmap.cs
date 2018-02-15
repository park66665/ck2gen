using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace LibNoise.Modfiers
{
    public class LockBitmap
    {
        internal Bitmap source = null;
        public IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;
           public Bitmap Source { get { return source; } set { source = value; } }
        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public LockBitmap(Bitmap source)
        {
            this.source = source;
        }
        public LockBitmap()
        {
         
        }
        public LockBitmap(int width, int height)
        {
            this.source = new Bitmap(width, height);
        }
        public LockBitmap(int width, int height, PixelFormat format)
        {
            this.source = new Bitmap(width, height, format);
        }

        public void Blur(Int32 blurSize, Rectangle rectangle)
        {
            var image = this;

            // look at every pixel in the blur rectangle
            for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    Int32 avgR = 0, avgG = 0, avgB = 0;
                    Int32 blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (Int32 x = xx; (x < xx + blurSize && x < image.Width); x++)
                    {
                        for (Int32 y = yy; (y < yy + blurSize && y < image.Height); y++)
                        {
                            if (x < 0 || y < 0 || x >= image.Width || y >= image.Height)
                                continue;

                            Color pixel = GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }
                    if (blurPixelCount == 0)
                        continue;

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (Int32 x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                        for (Int32 y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                            SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

        }

        public void Blur(Int32 blurSize)
        {
            Rectangle rectangle = new Rectangle(0, 0, Width, Height);
           var image = this;

            // look at every pixel in the blur rectangle
            for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    Int32 avgR = 0, avgG = 0, avgB = 0;
                    Int32 blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (Int32 x = xx; (x < xx + blurSize && x < image.Width); x++)
                    {
                        for (Int32 y = yy; (y < yy + blurSize && y < image.Height); y++)
                        {
                            Color pixel = GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (Int32 x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                        for (Int32 y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                            SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }
         
            
        }

        public void ResizeImage(int width, int height, bool bDispose = true)
        {
            var image = source;
            var destRect = new Rectangle(0, 0, width, height);
            PixelFormat pf = source.PixelFormat;

            if (pf == PixelFormat.Format8bppIndexed)
                pf = PixelFormat.Format24bppRgb;
            if (pf == PixelFormat.DontCare)
                pf = PixelFormat.Format24bppRgb;
            var destImage = new Bitmap(width, height, pf);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            if(bDispose)
                source.Dispose();
            source = destImage;
        }
        public bool locked = false;
        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            if (locked)
                return;
            locked = true;
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                Stride = (PixelCount/Height) * step;
                 Iptr = bitmapData.Scan0;
             
                // Mutate data from pointer to array
            //    Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Stride { get; set; }

        public void SetPixel(int x, int y, float h)
        {

            if (h > 1)
                h = 1;
            if (h < 0)
                h = 0;
            SetPixel(x, y, (int)(255.0f * h));
        }

        internal void SetPixel(int x, int y, int h)
        {

            if (h > 255)
                h = 255;
            if (h < 0)
                h = 0;
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;

            SetPixel(x, y, Color.FromArgb(255, (int)(h), (int)(h), (int)(h)));
            return;
            unsafe
            {
                var scan0 = (byte*) Iptr;
                int bitmapStride = Stride;
                int bitmapPixelFormatSize = Depth/8;

                int index = (bitmapStride*y) + (x*bitmapPixelFormatSize);
                if (bitmapPixelFormatSize == 1)
                {

                    scan0[index] = (byte) h;

                }


            }
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockDirect()
        {
            if (!locked)
                return;
            locked = false;
            try
            {
                // Mutate data from byte array to pointer
      
                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                // Mutate data from byte array to pointer
                //  Marshal.Mutate(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                //   source.UnlockBits(bitmapData); 
                //   throw ex;
            }
        }
        public void UnlockBits()
        {
            if (!locked)
                return;
            locked = false;
            try
            {
                // Mutate data from byte array to pointer
          //      Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                // Mutate data from byte array to pointer
                //  Marshal.Mutate(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                //   source.UnlockBits(bitmapData); 
                //   throw ex;
            }
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixelOld(int x, int y)
        {
            Color clr = Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;

            unsafe
            {
                var scan0 = (byte*)Iptr;
                int bitmapStride = Stride;
                int bitmapPixelFormatSize = Depth / 8;

                int index = (bitmapStride * y) + (x * bitmapPixelFormatSize);
                if (bitmapPixelFormatSize == 4)
                {
                    scan0[index + 3] = color.A;
                    scan0[index + 2] = color.R;
                    scan0[index + 1] = color.G;
                    scan0[index] = color.B;

                }
                else if (bitmapPixelFormatSize == 1)
                {
                    scan0[index] = color.R;

                }
                else
                {
                    scan0[index + 2] = color.R;
                    scan0[index + 1] = color.G;
                    scan0[index] = color.B;

                }
                return;
//                return Color.FromArgb(255, scan0[index + 2], scan0[index + 1], scan0[index]);
            }

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }
        public void SetPixelOld(int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }

        public void Save24(string s)
        {
            if(File.Exists(s))
                File.Delete(s);
            ConvertTo24bpp(source).Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        public void SetSmoothHeight(int x, int y, float height, int range = 1)
        {
            SetPixel(x, y, Color.FromArgb(255, (int)(height * 255), (int)(height * 255), (int)(height * 255)));

            Blur(2, new Rectangle(x - (range), y - (range), range * 2, range * 2));


            //    Blur(range*2, new Rectangle(x-(range*2), y-(range*2), range*4, range*4));
        }

        public Color GetPixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return Color.Transparent;

            unsafe
            {
                var scan0 = (byte*) Iptr;
                int bitmapStride = Stride;
                int bitmapPixelFormatSize = Depth / 8;

                int index = (bitmapStride * y) + (x * bitmapPixelFormatSize);
                if (bitmapPixelFormatSize == 4)
                    return Color.FromArgb(scan0[index + 3], scan0[index + 2], scan0[index + 1], scan0[index]);
                else if(bitmapPixelFormatSize == 1)
                    return Color.FromArgb(scan0[index], scan0[index], scan0[index], scan0[index]);
                else
                    return Color.FromArgb(255, scan0[index + 2], scan0[index + 1], scan0[index]);
            }

            return Color.Black;
        }


        public byte GetHeight(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return 0;

            return GetPixel(x, y).R;
            unsafe
            {
                var scan0 = (byte*)Iptr;
                int bitmapStride = Stride;
                int bitmapPixelFormatSize = Depth / 8;

                int index = (bitmapStride * y) + (x * bitmapPixelFormatSize);

                return scan0[index];
            }

        }
    }
}
