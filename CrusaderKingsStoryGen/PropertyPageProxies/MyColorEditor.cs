using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Cyotek.Windows.Forms;

namespace CrusaderKingsStoryGen.PropertyPageProxies
{
    public class MyColorConverter : ColorConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
    public class MyColorEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
          
            IWindowsFormsEditorService svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (svc != null)
            {
                using (ColorPickerDialog form = new ColorPickerDialog())
                {
                    if(value !=null)
                        form.Color =(Color) value ;
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        return form.Color;
                    }
                }
            }

            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value == null)
                return;

            using (SolidBrush brush = new SolidBrush((Color)e.Value))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            e.Graphics.DrawRectangle(Pens.Black, e.Bounds);
        }
    }
}