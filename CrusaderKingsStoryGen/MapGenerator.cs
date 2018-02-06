using CrusaderKingsStoryGen.MapGen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibNoise.Modfiers;

namespace CrusaderKingsStoryGen
{
    public partial class MapGenerator : Form
    {
        GeneratedTerrainMap map;
        int usedSeed = 0;

        public MapGenerator()
        {
            InitializeComponent();
            if (randomize.Checked)
            {
                Rand.SetSeed(new Random().Next(1000000));
                seedBox.Value = Rand.Next(10000000);
            }
            climate.SelectedIndex = Globals.Climate = 0;
            land.Checked = true;
            mediumBrush.Checked = true;
            //Color.FromArgb(255, 69, 91, 186)
            randHigh.Checked = true;
            landDrawBitmap = new LockBitmap(new Bitmap(preview.Width, preview.Height));
            mountainDrawBitmap = new LockBitmap(new Bitmap(preview.Width, preview.Height));
            landProxyBitmap = new LockBitmap(new Bitmap(3072 / 2, 2048 / 2));
            mountainProxyBitmap = new LockBitmap(new Bitmap(3072 / 2, 2048 / 2));
            var a = new SolidBrush(Color.FromArgb(255, 130, 158, 75));
            var b = new SolidBrush(Color.FromArgb(255, 130 + 40, 158 + 40, 75 + 40));
            var c = new SolidBrush(Color.FromArgb(255, 65, 42, 17));
            var d = new SolidBrush(Color.FromArgb(255, 69, 91, 186));

            using (Graphics gg = Graphics.FromImage(landDrawBitmap.Source))
            {
                gg.Clear(Color.FromArgb(255, 69, 91, 186));
            }
            using (Graphics gg = Graphics.FromImage(mountainDrawBitmap.Source))
            {
                gg.Clear(Color.Transparent);
            }
        }

        private void generateLandmass_Click(object sender, EventArgs e)
        {
            map = new GeneratedTerrainMap();

            if (randomize.Checked)
                seedBox.Value = Rand.Next(10000000);

            int seed = (int) seedBox.Value;
            usedSeed = seed;
            int w = 3072;
            if (mapvlarge.Checked)
                w = 4096;

            if (maplarge.Checked)
                w = 3200;

            if (mapnorm.Checked)
                w = 3072;

            if (mapsmall.Checked)
                w = 2048;


            map.Init(w / 10, 2048 / 10, usedSeed);
            preview.Image = map.Map.Source;
            exportButton.Enabled = true;
        }

        private void randomize_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void generateDrawn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to generate?", "Are you sure?", MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
            {
                return;
            }
            if (Globals.GameDir.Trim().Length == 0)
            {
                MessageBox.Show(
                    "You need to set the CK2 game directory in the main interface Configuration tab before generating maps",
                    "Error");
                return;
            }
            Form1.instance.clear();
            if (Globals.MapOutputDir == null)
            {
                FolderBrowserDialog d = new FolderBrowserDialog();
                d.Description = "Choose root folder you would like to store your custom maps.";
                if (d.ShowDialog() == DialogResult.OK)
                {
                    Globals.MapOutputDir = d.SelectedPath;

                }
            }
            Globals.MapName = this.mapOutputDrawn.Text;

            if (!Directory.Exists(Globals.MapOutputTotalDir))
                Directory.CreateDirectory(Globals.MapOutputTotalDir);
            if (!Directory.Exists(Globals.MapOutputTotalDir + "map\\"))
                Directory.CreateDirectory(Globals.MapOutputTotalDir + "map\\");

            //   MapGenManager.instance.Width /= 4;
            //   MapGenManager.instance.Height /= 4;
            CopyDir(Directory.GetCurrentDirectory() + "\\data\\mapstuff", Globals.MapOutputTotalDir + "map");
            CopyDir(Directory.GetCurrentDirectory() + "\\data\\common", Globals.MapOutputTotalDir + "common");

            float delta = 1.0f;

            if (vhigh.Checked)
                delta = 2.5f;
            if (high.Checked)
                delta = 1.75f;
            if (normal.Checked)
                delta = 1.2f;
            if (low.Checked)
                delta = 0.85f;
            if (vLow.Checked)
                delta = 0.6f;

            if (mapvlarge.Checked)
                MapGenManager.instance.Width = 4096;

            if (maplarge.Checked)
                MapGenManager.instance.Width = 3200;

            if (mapnorm.Checked)
                MapGenManager.instance.Width = 3072;

            if (mapsmall.Checked)
                MapGenManager.instance.Width = 2048;

            //    MapGenManager.instance.Width /= 4;
            //   MapGenManager.instance.Height /= 4;

            LockBitmap lBit = new LockBitmap(landBitmapOut);
            LockBitmap mBit = new LockBitmap(mountainBitmap);

            MapGenManager.instance.Create(false, DrawnSeed, 1500, delta, lBit, mBit);
            lBit.UnlockBits();
            mBit.UnlockBits();
            //  MapGenManager.instance.Create(usedSeed, 1300);
            // MapGenManager.instance.Create(usedSeed, 600);
            LockBitmap waterColorMap =
                new LockBitmap(
                    DevIL.DevIL.LoadBitmap(Directory.GetCurrentDirectory() +
                                           "\\data\\mapstuff\\terrain\\colormap_water.dds"));
            waterColorMap.ResizeImage(MapGenManager.instance.Width, MapGenManager.instance.Height);
            DevIL.DevIL.SaveBitmap(Globals.MapOutputTotalDir + "map\\terrain\\colormap_water.dds", waterColorMap.Source);
            //  LockBitmap normalMap = new LockBitmap(new Bitmap((Directory.GetCurrentDirectory() + "\\data\\mapstuff\\world_normal_height.bmp")));
            //  normalMap.ResizeImage(MapGenManager.instance.Width, MapGenManager.instance.Height);

            //    normalMap.Save24(Globals.MapOutputTotalDir + "map\\world_normal_height.bmp");

            //preview.Image = DevIL.DevIL.LoadBitmap(Globals.MapOutputTotalDir + "map\\terrain\\colormap.dds");
            landBitmap = DevIL.DevIL.LoadBitmap(Globals.MapOutputTotalDir + "map\\terrain\\colormap.dds");
            preview.Invalidate();
            //   map = null;
            exportButton.Enabled = false;

            if (
                MessageBox.Show("Would you like to load this map to generate a world history on?", "Load Map?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Form1.instance.SetMap(Globals.MapOutputTotalDir);
                map = null;
                ProvinceBitmapManager.instance = new ProvinceBitmapManager();
                MapGenManager.instance = new MapGenManager();
                Close();
            }

            ProvinceBitmapManager.instance = new ProvinceBitmapManager();
            MapGenManager.instance = new MapGenManager();
            map = null;
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (Globals.GameDir.Trim().Length == 0)
            {
                MessageBox.Show(
                    "You need to set the CK2 game directory in the main interface Configuration tab before generating maps",
                    "Error");
                return;
            }
            Form1.instance.clear();
            if (Globals.MapOutputDir == null)
            {
                FolderBrowserDialog d = new FolderBrowserDialog();
                d.Description = "Choose root folder you would like to store your custom maps.";
                if (d.ShowDialog() == DialogResult.OK)
                {
                    Globals.MapOutputDir = d.SelectedPath;

                }
            }
            Globals.MapName = this.mapName.Text;

            if (!Directory.Exists(Globals.MapOutputTotalDir))
                Directory.CreateDirectory(Globals.MapOutputTotalDir);
            if (!Directory.Exists(Globals.MapOutputTotalDir + "map\\"))
                Directory.CreateDirectory(Globals.MapOutputTotalDir + "map\\");

            CopyDir(Directory.GetCurrentDirectory() + "\\data\\mapstuff", Globals.MapOutputTotalDir + "map");
            CopyDir(Directory.GetCurrentDirectory() + "\\data\\common", Globals.MapOutputTotalDir + "common");

            float delta = 1.0f;

            if (vhigh.Checked)
                delta = 2.5f;
            if (high.Checked)
                delta = 1.75f;
            if (normal.Checked)
                delta = 1.2f;
            if (low.Checked)
                delta = 0.85f;
            if (vLow.Checked)
                delta = 0.6f;

            if (mapvlarge.Checked)
                MapGenManager.instance.Width = 4096;

            if (maplarge.Checked)
                MapGenManager.instance.Width = 3200;

            if (mapnorm.Checked)
                MapGenManager.instance.Width = 3072;

            if (mapsmall.Checked)
                MapGenManager.instance.Width = 2048;

             
            var h = adjustedHeight;
            adjustedHeight = null;
            
            if (bFromHeightMap)
                MapGenManager.instance.Create(bFromHeightMap, usedSeed, 1500, delta, h);
            else
                MapGenManager.instance.Create(false, usedSeed, 1500, delta);
            //  MapGenManager.instance.Create(usedSeed, 1300);
            // MapGenManager.instance.Create(usedSeed, 600);
         
            LockBitmap waterColorMap =
                new LockBitmap(
                    DevIL.DevIL.LoadBitmap(Directory.GetCurrentDirectory() +
                                           "\\data\\mapstuff\\terrain\\colormap_water.dds"));
            waterColorMap.ResizeImage(MapGenManager.instance.Width, MapGenManager.instance.Height);
            DevIL.DevIL.SaveBitmap(Globals.MapOutputTotalDir + "map\\terrain\\colormap_water.dds", waterColorMap.Source);
            // LockBitmap normalMap = new LockBitmap(new Bitmap((Directory.GetCurrentDirectory() + "\\data\\mapstuff\\world_normal_height.bmp")));
            //   normalMap.ResizeImage(MapGenManager.instance.Width, MapGenManager.instance.Height);

            //     normalMap.Save24(Globals.MapOutputTotalDir + "map\\world_normal_height.bmp");

            preview.Image = DevIL.DevIL.LoadBitmap(Globals.MapOutputTotalDir + "map\\terrain\\colormap.dds");

            preview.Invalidate();
            //   map = null;
            exportButton.Enabled = false;

            if (
                MessageBox.Show("Would you like to load this map to generate a world history on?", "Load Map?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Form1.instance.SetMap(Globals.MapOutputTotalDir);
                map = null;
                ProvinceBitmapManager.instance = new ProvinceBitmapManager();
                MapGenManager.instance = new MapGenManager();
                Close();
            }

            ProvinceBitmapManager.instance = new ProvinceBitmapManager();
            MapGenManager.instance = new MapGenManager();
            map = null;
        }

        public void CopyDir(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            var files = Directory.GetFiles(to);
            for (int index = 0; index < files.Length; index++)
            {
                var file = files[index];
                File.Delete(file);
            }
            if (Directory.Exists(from))
            {
                files = Directory.GetFiles(from);
                foreach (var file in files)
                {
                    File.Copy(file, to + file.Substring(file.LastIndexOf('\\')));
                }

                var dirs = Directory.GetDirectories(from);

                foreach (var dir in dirs)
                {
                    CopyDir(dir, to + dir.Substring(dir.LastIndexOf('\\')));
                }
            }


        }

        private void loadHeight_Click(object sender, EventArgs e)
        {

        }

        private void vhigh_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void high_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void normal_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void low_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void vLow_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void newMap_Click(object sender, EventArgs e)
        {
            SeaCommands.Clear();
            LandCommands.Clear();
            MountainCommands.Clear();
            landProxyBitmap = new LockBitmap(new Bitmap(3072 / 2, 2048 / 2));
            mountainProxyBitmap = new LockBitmap(new Bitmap(3072 / 2, 2048 / 2));
            var a = new SolidBrush(Color.FromArgb(255, 130, 158, 75));
            var b = new SolidBrush(Color.FromArgb(255, 130 + 40, 158 + 40, 75 + 40));
            var c = new SolidBrush(Color.FromArgb(255, 65, 42, 17));
            var d = new SolidBrush(Color.FromArgb(255, 69, 91, 186));

            using (Graphics gg = Graphics.FromImage(landProxyBitmap.Source))
            {
                gg.Clear(Color.FromArgb(255, 69, 91, 186));
            }
            using (Graphics gg = Graphics.FromImage(mountainProxyBitmap.Source))
            {
                gg.Clear(Color.Transparent);
            }

            preview.Invalidate();
        }

        private void sea_Click(object sender, EventArgs e)
        {
            //     sea.Checked = true;
            land.Checked = false;
            //    hill.Checked = false;
            mountain.Checked = false;
        }

        private void water_Click(object sender, EventArgs e)
        {
            land.Checked = false;
            water.Checked = true;
            //  hill.Checked = false;
            mountain.Checked = false;

        }

        private void land_Click(object sender, EventArgs e)
        {
            //     sea.Checked = false;
            water.Checked = false;
            land.Checked = true;
            //  hill.Checked = false;
            mountain.Checked = false;
        }

        private void hill_Click(object sender, EventArgs e)
        {
            //     sea.Checked = false;
            land.Checked = false;
            //  hill.Checked = true;
            mountain.Checked = false;
        }

        private void mountain_Click(object sender, EventArgs e)
        {
            water.Checked = false;
            //      sea.Checked = false;
            land.Checked = false;
            //   hill.Checked = false;
            mountain.Checked = true;
        }

        public struct DrawCommand
        {
            public Point Point { get; set; }
            public int Radius { get; set; }

            public DrawCommand(Point p, int radius)
            {
                Radius = radius;
                Point = p;
            }
        }

        public List<DrawCommand> LandCommands = new List<DrawCommand>();
        public List<DrawCommand> HillCommands = new List<DrawCommand>();
        public List<DrawCommand> SeaCommands = new List<DrawCommand>();
        public List<DrawCommand> MountainCommands = new List<DrawCommand>();

        private int drawRadius = 32;
        Point lastP = new Point();

        public void Draw(int x, int y)
        {
            if (new Point(x, y) == lastP)
                return;

            lastP = new Point(x, y);
            if (land.Checked)
            {

                LandCommands.Add(new DrawCommand(new Point(x, y), drawRadius));
                var r = new Rectangle(x - drawRadius, y - drawRadius, x + drawRadius, y + drawRadius);

                float delStartX = r.X / (float) preview.Width;
                float delStartY = r.Y / (float) preview.Height;
                float delEndX = r.Right / (float) preview.Width;
                float delEndY = r.Bottom / (float) preview.Height;


                Rectangle src = new Rectangle((int) (mountainProxyBitmap.Source.Width * delStartX),
                    (int) (mountainProxyBitmap.Source.Height * delStartY),
                    (int)
                    ((mountainProxyBitmap.Source.Width * delEndX) - (mountainProxyBitmap.Source.Width * delStartX)),
                    (int)
                    ((mountainProxyBitmap.Source.Height * delEndY) - (mountainProxyBitmap.Source.Height * delStartY)));

                delStartX = src.X / (float) mountainProxyBitmap.Source.Width;
                delStartY = src.Y / (float) mountainProxyBitmap.Source.Height;
                delEndX = src.Right / (float) mountainProxyBitmap.Source.Width;
                delEndY = src.Bottom / (float) mountainProxyBitmap.Source.Height;


                src = new Rectangle((int) (preview.Width * delStartX), (int) (preview.Height * delStartY),
                    (int) ((preview.Width * delEndX) - (preview.Width * delStartX)),
                    (int) ((preview.Height * delEndY) - (preview.Height * delStartY)));

                preview.Invalidate(src);

            }
            if (water.Checked)
            {
                SeaCommands.Add(new DrawCommand(new Point(x, y), (int) (drawRadius / 2.5)));
                preview.Invalidate(new Rectangle(x - drawRadius, y - drawRadius, x + drawRadius, y + drawRadius));

            }
            if (mountain.Checked)
            {
                MountainCommands.Add(new DrawCommand(new Point(x, y), (int) (drawRadius / 2.5)));
                preview.Invalidate(new Rectangle(x - drawRadius, y - drawRadius, x + drawRadius, y + drawRadius));

            }
        }

        private bool drawing = false;

        private void preview_MouseDown(object sender, MouseEventArgs e)
        {
            last = new Point(e.X, e.Y);
            preview.Capture = true;
            drawing = true;
            if (drawing)
            {
                Draw(e.X, e.Y);
            }
            landBitmap = null;
            preview.Invalidate();
        }

        private void preview_MouseUp(object sender, MouseEventArgs e)
        {

            preview.Capture = false;
            drawing = false;
            preview.Invalidate();
            var a = new SolidBrush(Color.FromArgb(255, 130, 158, 75));
            var b = new SolidBrush(Color.FromArgb(255, 130 + 40, 158 + 40, 75 + 40));
            var c = new SolidBrush(Color.FromArgb(255, 65, 42, 17));
            var d = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            if (land.Checked || water.Checked)
            {
                using (Graphics gg = Graphics.FromImage(landProxyBitmap.Source))
                {
                    using (Graphics gg2 = Graphics.FromImage(mountainProxyBitmap.Source))
                    {
                        foreach (var drawCommand in LandCommands)
                        {
                            var rect = new Rectangle(drawCommand.Point.X - drawCommand.Radius,
                                drawCommand.Point.Y - drawCommand.Radius, drawCommand.Radius * 2, drawCommand.Radius * 2);

                            float deltaX = mountainProxyBitmap.Source.Width / (float) preview.Width;
                            float deltaY = mountainProxyBitmap.Source.Height / (float) preview.Height;

                            Point ap = new Point((int) (rect.X * deltaX), (int) (rect.Y * deltaY));
                            Point bp = new Point((int) (rect.Right * deltaX), (int) (rect.Bottom * deltaY));
                            rect = new Rectangle(ap.X, ap.Y, bp.X - ap.X, bp.Y - ap.Y);
                            gg.FillEllipse(a, rect);
                            gg2.FillEllipse(d, rect);
                        }
                        foreach (var drawCommand in SeaCommands)
                        {
                            var rect = new Rectangle(drawCommand.Point.X - drawCommand.Radius,
                                drawCommand.Point.Y - drawCommand.Radius, drawCommand.Radius * 2, drawCommand.Radius * 2);

                            float deltaX = mountainProxyBitmap.Source.Width / (float) preview.Width;
                            float deltaY = mountainProxyBitmap.Source.Height / (float) preview.Height;

                            Point ap = new Point((int) (rect.X * deltaX), (int) (rect.Y * deltaY));
                            Point bp = new Point((int) (rect.Right * deltaX), (int) (rect.Bottom * deltaY));
                            rect = new Rectangle(ap.X, ap.Y, bp.X - ap.X, bp.Y - ap.Y);

                            gg.FillEllipse(d, rect);
                            gg2.FillEllipse(d, rect);
                        }
                    }
                    landProxyBitmap.Source.MakeTransparent(Color.Black);
                    mountainProxyBitmap.Source.MakeTransparent(Color.Black);
                    LandCommands.Clear();
                    SeaCommands.Clear();
                }
            }
            if (mountain.Checked)
            {
                using (Graphics gg = Graphics.FromImage(mountainProxyBitmap.Source))
                {

                    foreach (var drawCommand in MountainCommands)
                    {
                        var rect = new Rectangle(drawCommand.Point.X - drawCommand.Radius,
                            drawCommand.Point.Y - drawCommand.Radius, drawCommand.Radius * 2, drawCommand.Radius * 2);

                        float deltaX = mountainProxyBitmap.Source.Width / (float) preview.Width;
                        float deltaY = mountainProxyBitmap.Source.Height / (float) preview.Height;

                        Point ap = new Point((int) (rect.X * deltaX), (int) (rect.Y * deltaY));
                        Point bp = new Point((int) (rect.Right * deltaX), (int) (rect.Bottom * deltaY));
                        rect = new Rectangle(ap.X, ap.Y, bp.X - ap.X, bp.Y - ap.Y);

                        gg.FillEllipse(c, rect);
                    }

                    MountainCommands.Clear();

                }
            }

            MapGenerator_ResizeEnd(null, new EventArgs());
        }

        private Point last;

        private void preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                float difx = (e.X - last.X);
                float dify = (e.Y - last.Y);
                if (difx != 0)
                    difx = 1 / difx;
                if (dify != 0)
                    dify = 1 / dify;
                float xx = last.X;
                float yy = last.Y;
                while (Math.Abs(Math.Round(xx) - e.X) > 0.001f && Math.Abs(Math.Round(yy) - e.Y) > 0.001f)
                {
                    Draw((int) Math.Round(xx), (int) Math.Round(yy));
                    xx += difx;
                    yy += dify;
                }

                last.X = e.X;
                last.Y = e.Y;

                Draw(e.X, e.Y);
            }


        }

        private void preview_Paint(object sender, PaintEventArgs e)
        {
            if (tabPage1.Visible)
                return;
            if (tabPage3.Visible)
                return;
            e.Graphics.PageUnit = GraphicsUnit.Pixel;

            var a = new SolidBrush(Color.FromArgb(255, 130, 158, 75));
            var b = new SolidBrush(Color.FromArgb(255, 130 + 40, 158 + 40, 75 + 40));
            var c = new SolidBrush(Color.FromArgb(255, 65, 42, 17));
            var d = new SolidBrush(Color.FromArgb(255, 69, 91, 186));
            e.Graphics.Clear(Color.FromArgb(255, 69, 91, 186));

            ColorMatrix cm = new ColorMatrix();
            ImageAttributes ia = new ImageAttributes();


            cm.Matrix00 = (130 / 255.0f); // * 0.7f;
            cm.Matrix11 = (158 / 255.0f); // * 0.7f;
            cm.Matrix22 = (75 / 255.0f); // * 0.7f;
            cm.Matrix33 = 1.0f;
            cm.Matrix44 = 1.0f;
            //   cm.Matrix33 = 0.4f;

            ia.SetColorMatrix(cm);

            var totRect = e.ClipRectangle;


            e.Graphics.DrawImage(landDrawBitmap.Source, totRect, totRect, e.Graphics.PageUnit);
            e.Graphics.DrawImage(mountainDrawBitmap.Source, totRect, totRect, e.Graphics.PageUnit);
            foreach (var drawCommand in LandCommands)
            {
                e.Graphics.FillEllipse(a,
                    new Rectangle(drawCommand.Point.X - drawCommand.Radius, drawCommand.Point.Y - drawCommand.Radius,
                        drawCommand.Radius * 2, drawCommand.Radius * 2));
            }
            foreach (var drawCommand in SeaCommands)
            {
                e.Graphics.FillEllipse(d,
                    new Rectangle(drawCommand.Point.X - drawCommand.Radius, drawCommand.Point.Y - drawCommand.Radius,
                        drawCommand.Radius * 2, drawCommand.Radius * 2));
            }
            foreach (var drawCommand in HillCommands)
            {
                e.Graphics.FillEllipse(b,
                    new Rectangle(drawCommand.Point.X - drawCommand.Radius, drawCommand.Point.Y - drawCommand.Radius,
                        drawCommand.Radius * 2, drawCommand.Radius * 2));
            }
            foreach (var drawCommand in MountainCommands)
            {
                e.Graphics.FillEllipse(c,
                    new Rectangle(drawCommand.Point.X - drawCommand.Radius, drawCommand.Point.Y - drawCommand.Radius,
                        drawCommand.Radius * 2, drawCommand.Radius * 2));
            }

            if (landBitmap != null)
            {
                e.Graphics.DrawImage(landBitmap, new Rectangle(0, 0, preview.Width, preview.Height));
            }
        }

        private Bitmap landBitmap = null;
        private Bitmap hillBitmap = null;
        private Bitmap mountainBitmap = null;
        private Bitmap landBitmapOut;

        private LockBitmap landDrawBitmap = null;
        private LockBitmap mountainDrawBitmap = null;
        private LockBitmap landProxyBitmap = null;
        private LockBitmap mountainProxyBitmap = null;

        private void generateFromDraw_Click(object sender, EventArgs e)
        {
            generateDrawn.Enabled = true;
            int ow = 1;
            int oh = 1;
            int w = 3072;
            int h = 2048;
            if (mapvlarge.Checked)
                w = 4096;

            if (maplarge.Checked)
                w = 3200;

            if (mapnorm.Checked)
                w = 3072;

            if (mapsmall.Checked)
                w = 2048;

            {
                Bitmap bmp = new Bitmap(w / 2, h / 2, PixelFormat.Format24bppRgb);
                Bitmap bmp2 = new Bitmap(w / 2, h / 2, PixelFormat.Format24bppRgb);
                LockBitmap lbmp2 = new LockBitmap(bmp2);

                LockBitmap lbmp = new LockBitmap(bmp);

                float deltaX = w / (float) preview.Width;
                float deltaY = h / (float) preview.Height;

                lbmp.LockBits();
                lbmp2.LockBits();
                landProxyBitmap.LockBits();
                mountainProxyBitmap.LockBits();
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        float dx = mountainProxyBitmap.Width / (float) bmp.Width;
                        float dy = mountainProxyBitmap.Height / (float) bmp.Height;

                        var col = mountainProxyBitmap.GetPixel((int) (x * dx), (int) (y * dy));

                        var col2 = landProxyBitmap.GetPixel((int) (x * dx), (int) (y * dy));

                        int xx = x;
                        int yy = y;

                        if (col.R > 0 && col.A > 0)
                        {
                            lbmp.SetPixel(xx, yy, Color.White);
                        }
                        else
                        {
                            lbmp.SetPixel(xx, yy, Color.Black);
                        }

                        if (col2.R != 69 && col2.A > 0)
                        {
                            lbmp2.SetPixel(xx, yy, Color.White);
                        }
                        else
                        {
                            lbmp2.SetPixel(xx, yy, Color.Black);
                        }
                    }

                }
                lbmp2.UnlockBits();
                lbmp.UnlockBits();
                landProxyBitmap.UnlockBits();
                mountainProxyBitmap.UnlockBits();
                /*   Graphics g = Graphics.FromImage(bmp);
   
                   foreach (var drawCommand in MountainCommands)
                   {
                       var r = new Rectangle(drawCommand.Point.X - drawCommand.Radius, drawCommand.Point.Y - drawCommand.Radius,
                           drawCommand.Radius * 2, drawCommand.Radius * 2);
   
                       Rectangle rect = new Rectangle((int)(r.X * deltaX), (int)(r.Y * deltaY), (int)((r.Right * deltaX) - (r.Left * deltaX)), (int)((r.Bottom * deltaY) - r.Top * deltaY));
   
                       g.FillEllipse(a, rect);
                   }
                   */
                int rand = 32;
                if (randMed.Checked)
                    rand = 32;
                if (randHigh.Checked)
                    rand = 32;

                ow = bmp.Width;
                oh = bmp.Height;
                lbmp.ResizeImage(ow / rand, oh / rand);
                lbmp.ResizeImage(ow * 2, oh * 2);
                mountainBitmap = lbmp.Source;

                rand = 8;
                BitmapSelect.Randomness = 0.75f;

                if (randMed.Checked)
                {
                    rand = 16;
                    BitmapSelect.Randomness = 1f;
                }
                if (randHigh.Checked)
                {
                    rand = 64;
                    BitmapSelect.Randomness = 1f;
                }
                if (randMin.Checked)
                {
                    rand = 2;
                    BitmapSelect.Randomness = 0.5f;
                }

                ow = bmp2.Width;
                oh = bmp2.Height;
                lbmp2.ResizeImage(ow / rand, oh / rand);
                lbmp2.ResizeImage(ow * 2, oh * 2);
                landBitmap = lbmp2.Source;

            }

            map = new GeneratedTerrainMap();

            int seed = Rand.Next(1000000);
            LockBitmap lBit = new LockBitmap(landBitmap);
            LockBitmap mBit = new LockBitmap(mountainBitmap);
            map.Init(ow / 4, oh / 4, lBit, lBit, mBit, seed);
            lBit.UnlockBits();
            mBit.UnlockBits();
            DrawnSeed = seed;
            landBitmap = map.Map.Source;
            landBitmapOut = lBit.Source;
            mountainBitmap = mBit.Source;
            preview.Invalidate();
        }

        public int DrawnSeed { get; set; }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            preview.Invalidate();
        }

        private void smallBrush_Click(object sender, EventArgs e)
        {
            smallBrush.Checked = true;
            mediumBrush.Checked = false;
            largeBrush.Checked = false;
            drawRadius = 16;
        }

        private void mediumBrush_Click(object sender, EventArgs e)
        {
            smallBrush.Checked = false;
            mediumBrush.Checked = true;
            largeBrush.Checked = false;
            drawRadius = 32;

        }

        private void largeBrush_Click(object sender, EventArgs e)
        {
            smallBrush.Checked = false;
            mediumBrush.Checked = false;
            largeBrush.Checked = true;
            drawRadius = 64;

        }

        private void randLow_Click(object sender, EventArgs e)
        {
            randLow.Checked = true;
            randMed.Checked = false;
            randHigh.Checked = false;
            randMin.Checked = false;
        }

        private void randMed_Click(object sender, EventArgs e)
        {
            randLow.Checked = false;
            randMed.Checked = true;
            randHigh.Checked = false;
            randMin.Checked = false;

        }

        private void randHigh_Click(object sender, EventArgs e)
        {
            randLow.Checked = false;
            randMed.Checked = false;
            randHigh.Checked = true;
            randMin.Checked = false;
        }

        private void randMin_Click(object sender, EventArgs e)
        {
            randLow.Checked = false;
            randMed.Checked = false;
            randHigh.Checked = false;
            randMin.Checked = true;

        }

        private void MapGenerator_ResizeEnd(object sender, EventArgs e)
        {
            if (landDrawBitmap == null)
                return;

            var oldLand = landProxyBitmap;
            var oldMount = mountainProxyBitmap;
            if (landDrawBitmap.Source.Width != preview.Width || landDrawBitmap.Source.Height != preview.Height)
            {
                landDrawBitmap = new LockBitmap(new Bitmap(preview.Width, preview.Height));
                mountainDrawBitmap = new LockBitmap(new Bitmap(preview.Width, preview.Height));

            }

            using (Graphics gg = Graphics.FromImage(landDrawBitmap.Source))
            {
                gg.Clear(Color.Transparent);
                gg.SmoothingMode = SmoothingMode.Default;
                gg.DrawImage(oldLand.Source, new Rectangle(0, 0, preview.Width, preview.Height));
            }
            using (Graphics gg = Graphics.FromImage(mountainDrawBitmap.Source))
            {
                gg.Clear(Color.Transparent);
                gg.SmoothingMode = SmoothingMode.Default;
                gg.DrawImage(oldMount.Source, new Rectangle(0, 0, preview.Width, preview.Height));
            }

        }

        private void climate_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.Climate = climate.SelectedIndex;
        }

        private void loadHeightMap_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "All Graphics Types|*.bmp;*.png;*.jpg";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap i = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);

                this.HeightMapBitmap = new LockBitmap(i);
                this.HeightMapBitmapPreview = new LockBitmap(new Bitmap(i));
                {
                    HeightMapBitmapPreview.LockBits();

                    if ((HeightMapBitmapPreview.Width * HeightMapBitmapPreview.Height) > 4096 * 2048)
                    {
                        MessageBox.Show(this, "Error: Height map too large. Max size 4096x2048");
                        HeightMapBitmapPreview.UnlockBits();
                        return;
                    }
                    for (int x = 0; x < HeightMapBitmapPreview.Width; x++)
                    {
                        for (int y = 0; y < HeightMapBitmapPreview.Height; y++)
                        {
                            byte h = HeightMapBitmapPreview.GetHeight(x, y);
                            if (h < seaLevel.Value )
                                HeightMapBitmapPreview.SetPixel(x, y, Color.FromArgb(255, 69, 91, 186));
                        }
                    }
                    HeightMapBitmapPreview.UnlockBits();
                    preview.Image = HeightMapBitmapPreview.Source;
                    preview.Invalidate();
                    //using (Graphics gg = Graphics.FromImage(landDrawBitmap.Source))
                    {
                      //  gg.Clear(Color.Transparent);
                    //    gg.SmoothingMode = SmoothingMode.Default;
                  //      gg.DrawImage(i, new Rectangle(0, 0, preview.Width, preview.Height));
                    }
                }
            }
        }

        public LockBitmap HeightMapBitmapPreview { get; set; }

        public LockBitmap HeightMapBitmap { get; set; }

        private void seaLevel_ValueChanged(object sender, EventArgs e)
        {
            this.HeightMapBitmapPreview = new LockBitmap(new Bitmap(HeightMapBitmap.Source));
            {
                HeightMapBitmapPreview.LockBits();
                for (int x = 0; x < HeightMapBitmapPreview.Width; x++)
                {
                    for (int y = 0; y < HeightMapBitmapPreview.Height; y++)
                    {
                        byte h = HeightMapBitmapPreview.GetHeight(x, y);
                        if (h < seaLevel.Value)
                            HeightMapBitmapPreview.SetPixel(x, y, Color.FromArgb(255, 69, 91, 186));
                    }
                }
                HeightMapBitmapPreview.UnlockBits();
                preview.Image = HeightMapBitmapPreview.Source;
                preview.Invalidate();
            }
        }

        private LockBitmap adjustedHeight;
        private bool bFromHeightMap;

        private void generateFromHeightMap_Click(object sender, EventArgs e)
        {
            if (mapvlarge.Checked)
                MapGenManager.instance.Width = 4096;

            if (maplarge.Checked)
                MapGenManager.instance.Width = 3200;

            if (mapnorm.Checked)
                MapGenManager.instance.Width = 3072;

            if (mapsmall.Checked)
                MapGenManager.instance.Width = 2048;

            adjustedHeight = new LockBitmap(new Bitmap(MapGenManager.instance.Width, 2048, HeightMapBitmap.Source.PixelFormat));
            HeightMapBitmap.LockBits();
            bFromHeightMap = true;
            adjustedHeight.LockBits();
            float seaLevel = (float) this.seaLevel.Value;

            for (int x = 0; x < adjustedHeight.Width; x++)
            {
                for (int y = 0; y < adjustedHeight.Height; y++)
                {
                    adjustedHeight.SetPixel(x, y, 89 / 255.0f);

                }
            }
            for (int x = 0; x < HeightMapBitmap.Width; x++)
            {
                for (int y = 0; y < HeightMapBitmap.Height; y++)
                {
                    byte h = HeightMapBitmap.GetHeight(x, y);

                    int xa = x;
                    int ya = y;

                    int halfo = HeightMapBitmap.Width / 2;
                    int halfa = adjustedHeight.Width / 2;
                    int startX = halfa - halfo;
                    xa += startX;

                    if (HeightMapBitmap.Height < adjustedHeight.Height)
                    {
                        halfo = HeightMapBitmap.Height / 2;
                        halfa = adjustedHeight.Height / 2;
                        ya += halfa - halfo;
                    }

                    if (h < seaLevel)
                    {
                    
                        float below = 89;

                        adjustedHeight.SetPixel(xa, ya, below / 255.0f);
                    }
                    else
                    {
                        h -= (byte)seaLevel;
                        float delta = h / (255.0f - seaLevel);

                        delta *= (float)(reliefScale.Value) / 100.0f;
                        float below = delta * (255.0f - 98.0f);
                        below += 98;

                        adjustedHeight.SetPixel(xa, ya, below / 255.0f);
                    }
                }
            }
            HeightMapBitmap.UnlockBits();
            adjustedHeight.UnlockBits();

         //   adjustedHeight.ResizeImage(adjustedHeight.Width / 2, adjustedHeight.Height / 2);
        //    adjustedHeight.ResizeImage(adjustedHeight.Width, adjustedHeight.Height);
            HeightMapBitmap.Source.Dispose();
            HeightMapBitmap = null;
            GC.Collect();
            exportButton_Click(null, new EventArgs());
            /*

                        {
                            map = new GeneratedTerrainMap();

                            int ow = 1;
                            int oh = 1;
                            int w = 3072;
                            int h = 2048;
                            if (mapvlarge.Checked)
                                w = 4096;

                            if (maplarge.Checked)
                                w = 3200;

                            if (mapnorm.Checked)
                                w = 3072;

                            if (mapsmall.Checked)
                                w = 2048;


                            int seed = Rand.Next(1000000);

                            map.InitFromHeightMap(w / 4, h / 4, adjustedHeight);

                            DrawnSeed = seed;
                            landBitmap = map.Map.Source;

                            preview.Image = landBitmap;
                            preview.Invalidate();
                        }
                        */
        }

    }
}

