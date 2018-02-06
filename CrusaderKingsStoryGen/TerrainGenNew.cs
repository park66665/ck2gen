using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrusaderKingsStoryGen.MapGen;
using LibNoise;

namespace CrusaderKingsStoryGen
{
    public partial class TerrainGenNew : Form
    {
        public TerrainMap noise = new TerrainMap();
        public TerrainGenNew()
        {
            InitializeComponent();
            noise.Init(new Random().Next(100000), 3072/6, 2048/6);
            //preview.Image = noise.ResultBitmap2;
            pictureBox1.Image = noise.ResultBitmap;

        }
    }
}
