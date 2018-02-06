using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise.Modfiers;

namespace CrusaderKingsStoryGen.MapGen
{
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public void Normalize()
        {
            Normalize(ref this, out this);
        }

        public static Vector3 Normalize(Vector3 vector)
        {
            Normalize(ref vector, out vector);
            return vector;
        }

        public static float Distance(Vector3 vector1, Vector3 vector2)
        {
            float result;
            DistanceSquared(ref vector1, ref vector2, out result);
            return (float)Math.Sqrt(result);
        }

        public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (float)Math.Sqrt(result);
        }

        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            float result;
            DistanceSquared(ref value1, ref value2, out result);
            return result;
        }

        public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            result = (value1.X - value2.X) * (value1.X - value2.X) +
                     (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                     (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }
        public static void Normalize(ref Vector3 value, out Vector3 result)
        {        
            result = new Vector3(value.X,value.Y,value.Z);    
            float factor;
            Distance(ref value, ref zero, out factor);
            factor = 1f / factor;
            result.X = value.X * factor;
            result.Y = value.Y * factor;
            result.Z = value.Z * factor;
        }
        private static Vector3 zero = new Vector3(0f, 0f, 0f);

    }
    public class NormalMapGenerator
    {
        public static NormalMapGenerator instance = new NormalMapGenerator();

        public LockBitmap Create(LockBitmap bmp)
        {
            LockBitmap outBmp = new LockBitmap(bmp.Source.Width, bmp.Source.Height);
            outBmp.LockBits();
            bmp.LockBits();
            int texwidth = bmp.Width;
            int texheight = bmp.Height;

            float texelSize = 1.0f / (float)texwidth;
            float normalStrength = (float)(Rand.Next(40) + 60) / 100.0f;
            normalStrength /= 10.0f;
            byte pixel;
            float tl, l, bl, b, tr, r, br, t;

            for (int y = 0; y < texheight; y++)
                for (int x = 0; x < texwidth; x++)
                {
                    //Vector2 uv = new Vector2(x, y);
                    pixel = bmp.GetPixel(x, y).R;
                    tl = bmp.GetPixel(x - 1, y - 1).R / 255.0f;
                    l = bmp.GetPixel(x - 1, y - 0).R / 255.0f;
                    bl = bmp.GetPixel(x - 1, y + 1).R / 255.0f;
                    t = bmp.GetPixel(x - 0, y - 1).R / 255.0f;
                    b = bmp.GetPixel(x - 0, y + 1).R / 255.0f;
                    tr = bmp.GetPixel(x + 1, y - 1).R / 255.0f;
                    r = bmp.GetPixel(x + 1, y - 0).R / 255.0f;
                    br = bmp.GetPixel(x + 1, y + 1).R / 255.0f;

                    // Compute dx using Sobel:
                    //           -1 0 1 
                    //           -2 0 2
                    //           -1 0 1
                    float dX = tr + 2 * r + br - tl - 2 * l - bl;

                    // Compute dy using Sobel:
                    //           -1 -2 -1 
                    //            0  0  0
                    //            1  2  1
                    float dY = bl + 2 * b + br - tl - 2 * t - tr;

                    // Build the normalized normal

                    Vector3 N;
                    if (pixel < 95)
                        N = new Vector3(dX, 4, dY);
                    else
                        N = new Vector3(dX, normalStrength, dY);
                    N = Vector3.Normalize(N);
                    N.X = -N.X;
                    N.X += 1.0f;
                    N.X /= 2.0f;
                    N.Y += 1.0f;
                    N.Y /= 2.0f;
                    N.Z += 1.0f;
                    N.Z /= 2.0f;

                    outBmp.SetPixel(x, y, Color.FromArgb(255, (int) (N.X*255), (int) (N.Z * 255), (int) (N.Y * 255)));;
                }
            outBmp.UnlockBits();
            return outBmp;
        }
    }
}
