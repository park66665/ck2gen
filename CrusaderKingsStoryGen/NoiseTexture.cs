using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;
using LibNoise.Modfiers;

namespace CrusaderKingsStoryGen
{
    public class NoiseTexture : LockBitmap
    {
        private double[,] heights;
        public double minRange = 100000;
        public double maxRange = -100000;
        public float DefaultFrequencyDiv = 150.0f;
        private float capMin = -1;
        private float capMax = -1;

        private NoiseTexture(Bitmap source) : base(source)
        {
        }
      

        public static implicit operator Bitmap(NoiseTexture tex)
        {
            return tex.Source;
        }

        public NoiseTexture(int width, int height, IModule source, float delta = 1.0f, float capMax=-1, float capMin=-1)
        {
            Source = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = Source.Palette;

            for (int i = 0; i < 256; i++)
            {
                pal.Entries[i] = Color.FromArgb(255, i, i, i);
            }

            Source.Palette = pal;

            this.capMax = capMax;
            this.capMin = capMin;
            DefaultFrequencyDiv = DefaultFrequencyDiv*delta;
            heights = new double[width,height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SetHeight(x, y, (float)((source.GetValue(x/ DefaultFrequencyDiv, 0, y/ DefaultFrequencyDiv))));
                }
            }

            ApplyNoise();
        }

        public float GetHeightDelta(int x, int y)
        {
            return (float) ((heights[x, y] - minRange) / Range);
        }

        private void ApplyNoise()
        {
            LockBits();
            if (capMax != -1)
            {
                maxRange = capMax;
                minRange = capMin;
            }
            double range = Range;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    double h = heights[x, y];


                    double adj = h - minRange;
                    
                    adj = adj/range;
                    if (range == 0 || Double.IsNaN(range) || Double.IsInfinity(range))
                        adj = minRange;
                    SetPixel(x, y, (float)adj);

                }
            }
            UnlockBits();

            heights = null;
        }

        public double Range
        {
            get { return maxRange - minRange; }
        }
        private void SetHeight(int x, int y, double h)
        {
            if (h > maxRange)
                maxRange = h;
            if (h < minRange)
                minRange = h;

            heights[x, y] = h;
        }
    }
}
