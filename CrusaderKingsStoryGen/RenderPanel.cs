using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrusaderKingsStoryGen
{
    public partial class RenderPanel : PictureBox
    {
        public RenderPanel()
        {
            InitializeComponent();
            DoubleBuffered = true;

            
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
        }
    }
}
