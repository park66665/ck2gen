using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using LibNoise.Modifiers;

namespace LibNoise.Modfiers
{
    public class BitmapSelect : Select
    {
        public static float Randomness = 1.0f;
        public BitmapSelect(IModule control, IModule source1, IModule source2, float div, LockBitmap bmp) : base(control, source1, source2)
        {
            this.Bitmap = bmp;
            this.Div = div;
            Bitmap.LockBits();
        }

        public float Div { get; set; }

        public LockBitmap Bitmap { get; set; }

        public override double GetValue(double x, double y, double z)
        {
         //   var result = base.GetValue(x, y, z);


            double del = Div * 150.0f;
            double xx = (x * del);
            double yy = (z * del);
            var col = Bitmap.GetPixel((int)xx, (int)(yy));

            float height = col.R / 255.0f;
            if (col.A == 0)
                return 0;
            if (height > 0.7f)
            {

            }
            height -= 0.5f;
            height *= 2.0f;

            double controlValue = ControlModule.GetValue(x, y, z);
            double alpha;
            controlValue = height + (controlValue * 1.2f * Randomness);

            if (EdgeFalloff > 0.0)
            {
                if (controlValue < (LowerBound - EdgeFalloff))
                {
                    // The output value from the control module is below the selector
                    // threshold; return the output value from the first source module.
                    return SourceModule1.GetValue(x, y, z);
                }
                else if (controlValue < (LowerBound + EdgeFalloff))
                {
                    // The output value from the control module is near the lower end of the
                    // selector threshold and within the smooth curve. Interpolate between
                    // the output values from the first and second source modules.
                    double lowerCurve = (LowerBound - EdgeFalloff);
                    double upperCurve = (LowerBound + EdgeFalloff);
                    alpha = SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));
                    return LinearInterpolate(SourceModule1.GetValue(x, y, z),
                        SourceModule2.GetValue(x, y, z), alpha);
                }
                else if (controlValue < (UpperBound - EdgeFalloff))
                {
                    // The output value from the control module is within the selector
                    // threshold; return the output value from the second source module.
                    return SourceModule2.GetValue(x, y, z);
                }
                else if (controlValue < (UpperBound + EdgeFalloff))
                {
                    // The output value from the control module is near the upper end of the
                    // selector threshold and within the smooth curve. Interpolate between
                    // the output values from the first and second source modules.
                    double lowerCurve = (UpperBound - EdgeFalloff);
                    double upperCurve = (UpperBound + EdgeFalloff);
                    alpha = SCurve3(
                      (controlValue - lowerCurve) / (upperCurve - lowerCurve));
                    return LinearInterpolate(SourceModule2.GetValue(x, y, z),
                      SourceModule1.GetValue(x, y, z),
                      alpha);
                }
                else
                {
                    // Output value from the control module is above the selector threshold;
                    // return the output value from the first source module.
                    return SourceModule1.GetValue(x, y, z);
                }
            }
            else
            {
                if (controlValue < LowerBound || controlValue > UpperBound)
                {
                    return SourceModule1.GetValue(x, y, z);
                }
                else
                {
                    return SourceModule2.GetValue(x, y, z);
                }
            }

        }
    }
}
