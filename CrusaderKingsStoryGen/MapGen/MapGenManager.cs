using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using csDelaunay;
using LibNoise.Modfiers;
using Encoder = System.Drawing.Imaging.Encoder;

namespace CrusaderKingsStoryGen.MapGen
{
    public class MapGenManager
    {
        public static MapGenManager instance = new MapGenManager();

        public class ProvincePoint
        {
            public bool IsSea { get; set; }
            public Color Color { get; set; }
            public Point Position { get; set; }
        }

        private static int Div = 4;
        public int RegionGridSize { get; set; } = 128 / 3;

        public float DivNoise { get; set; } = 1500.0f;

        public Dictionary<Point, List<TerritoryPoint>> Regions = new Dictionary<Point, List<TerritoryPoint>>();
        GeneratedTerrainMap generatedMap = new GeneratedTerrainMap();
        private byte[] heightData;
        private byte[] provinceData;
        private BitmapData BMPData;
        public int Width { get; set; } = 3072;
        public int Height { get; set; } = 2048;
        public void Create(bool fromHeightMap = false, int seed = 0, int numProvinces=1500, float delta = 1.0f, LockBitmap landBitmap = null, LockBitmap mountainBitmap = null)
        {
            DivNoise = 1500.0f * (Width / 3072.0f);
           // LoadProvinceColors();
            
            //   Bitmap map = new Bitmap(Width, Height);
            //  Bitmap provinceMap = new Bitmap(Width, Height);
            //   Bitmap terrainMap = new Bitmap(Width, Height);

            //  var bmpData = map.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //  var bmpData2 = provinceMap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //  var bmpData3 = terrainMap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //  BMPData = bmpData;
            //  NoiseGenerator noise = new NoiseGenerator();
            //  NoiseGenerator noiseDes = new NoiseGenerator();
            //  NoiseGenerator noiseForest = new NoiseGenerator();
            // this.Noise = noise;
            // IntPtr ptr = bmpData.Scan0;
            //  IntPtr ptr2 = bmpData2.Scan0;
            //  IntPtr ptr3 = bmpData3.Scan0;
            //  int numBytes = bmpData.Stride * map.Height;

            //   byte[] hValues = new byte[numBytes];
            //  byte[] pValues = new byte[numBytes];
            //    byte[] tValues = new byte[numBytes];
            //   provinceData = pValues;
            //  heightData = hValues;

            //Color waterHeight = Color.FromArgb(255, 50,50,50);
            //SolidBrush b = new SolidBrush(waterHeight);
            if(fromHeightMap)
                generatedMap.InitFromHeightMap(Width, Height, landBitmap, false, seed);
            else
                generatedMap.Init(Width, Height, landBitmap, landBitmap, mountainBitmap, seed);
            ProvinceMap = new LockBitmap(new Bitmap(generatedMap.TerrainMap.Source));
            ProvinceBitmapManager.instance = new ProvinceBitmapManager();
            generatedMap.CreateHeatMap();
            generatedMap.CreateMoistureMap();
            generatedMap.AdjustMoistureMap();

            ProvinceBitmapManager.instance.Init(ProvinceMap, delta, generatedMap);
            ProvinceBitmapManager.instance.CalculateClimate(generatedMap.HeatMap);
            //Generate(numProvinces);

            generatedMap.UpdateFromProvinces();
            if (!Directory.Exists(Globals.MapOutputTotalDir + "map\\statics\\"))
                Directory.CreateDirectory(Globals.MapOutputTotalDir + "map\\statics\\");
            if (!Directory.Exists(Globals.MapOutputTotalDir + "gfx\\FX\\"))
                Directory.CreateDirectory(Globals.MapOutputTotalDir + "gfx\\FX\\");

            if (File.Exists(Globals.MapOutputTotalDir + "map\\statics\\00_static.txt"))
                File.Delete(Globals.MapOutputTotalDir + "map\\statics\\00_static.txt");
            if (File.Exists(Globals.MapOutputTotalDir + "gfx\\FX\\pdxmap.lua"))
                File.Delete(Globals.MapOutputTotalDir + "gfx\\FX\\pdxmap.lua");
            if (File.Exists(Globals.MapOutputTotalDir + "map\\terrain\\fractal_noise.dds"))
                File.Delete(Globals.MapOutputTotalDir + "map\\terrain\\fractal_noise.dds");

          //  if (File.Exists(Globals.MapOutputTotalDir + "map\\terrain\\atlas0.dds"))
         //       File.Delete(Globals.MapOutputTotalDir + "map\\terrain\\atlas0.dds");
         //   if (File.Exists(Globals.MapOutputTotalDir + "map\\terrain\\atlas_normal0.dds"))
         //       File.Delete(Globals.MapOutputTotalDir + "map\\terrain\\atlas_normal0.dds");
        //    File.Copy(Globals.GameDir + "map\\terrain\\atlas0.dds", Globals.MapOutputTotalDir + "map\\terrain\\atlas0.dds");
          //  File.Copy(Globals.GameDir + "map\\terrain\\atlas_normal0.dds", Globals.MapOutputTotalDir + "map\\terrain\\atlas_normal0.dds");

            File.Copy(Directory.GetCurrentDirectory() + "\\data\\pdxmap.lua", Globals.MapOutputTotalDir + "gfx\\FX\\pdxmap.lua");
            File.Copy(Directory.GetCurrentDirectory() + "\\data\\mapstuff\\terrain\\fractal_noise.dds", Globals.MapOutputTotalDir + "map\\terrain\\fractal_noise.dds");
            using (System.IO.StreamWriter file =
                   new System.IO.StreamWriter(Globals.MapOutputTotalDir + "map\\statics\\00_static.txt", false, Encoding.GetEncoding(1252)))
            {
                switch (Width)
                {
                    case 2048:
                        file.WriteLine(@"
	object = {
		type = ""frame""
		position = { 5.000 -8.000 5.000 }
		rotation = { 0.000 0.000 0.000 }
		scale = 99.90
	}
");
                        break;
                    case 3072:
                        file.WriteLine(@"
	object = {
		type = ""frame3072""

        position = { 3.000 - 8.000 3.000 }
                        rotation = { 0.000 0.000 0.000 }
                        scale = 100.00

    }
                ");
                        break;
                    case 3200:
                        file.WriteLine(@"
	object = {
		type = ""frame3200""
		position = { 3.000 -8.000 3.000 }
		rotation = { 0.000 0.000 0.000 }
		scale = 100.00
	}
");
                        break;
                    case 4096:
                        file.WriteLine(@"
    object = {
		type = ""frame4096""
		position = { 3.000 -8.000 3.000 }
		rotation = { 0.000 0.000 0.000 }
		scale = 100.00
	}
");
                        break;
                }

                file.Close();
            }


            //  MapManager.instance.ProvinceBitmap = map;
            // MapManager.instance.colorMap = terrainMap;

        }
        public void CopyDir(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            var files = Directory.GetFiles(to);
            foreach (var file in files)
            {
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

        private void DelDir(string from, string to)
        {
            if (Directory.Exists(to))
            {
                var files = Directory.GetFiles(to);
                foreach (var file in files)
                {
                    File.Delete(file);
                }

                var dirs = Directory.GetDirectories(from);

                foreach (var dir in dirs)
                {
                    DelDir(dir, to + dir.Substring(dir.LastIndexOf('\\')));
                }
            }


        }


        public LockBitmap ProvinceMap { get; set; }

        public static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, image.PixelFormat);

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

            return destImage;
        }
        Dictionary<int, Color> colorMap = new Dictionary<int, Color>();
        Dictionary<Color, int>  invColorMap = new Dictionary<Color, int>();


        private void CalculatePositions(List<ProvinceDetails> provinces)
        {
       
            using (System.IO.StreamWriter filein =
                new System.IO.StreamWriter(Globals.MapOutputTotalDir + "map\\positions.txt", false, Encoding.GetEncoding(1252)))
            {
                foreach (var provinceDetailse in provinces)
                {
                    Point min = provinceDetailse.min;
                    Point max = provinceDetailse.max;
                    Point c = new Point(max.X - min.X, max.Y - min.Y);
                    min.X += c.X / 3;
                    min.Y += c.Y / 3;
                    max.X -= c.X / 3;
                    max.Y -= c.Y / 3;
                    Rectangle rect = new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
                    var centre = provinceDetailse.points.Where(p => rect.Contains(p));
                    var town = centre.OrderBy(p => Rand.Next(10000000)).First();
                    var army = centre.OrderBy(p => Rand.Next(10000000)).First();
                    var councillers = centre.OrderBy(p => Rand.Next(10000000)).First();
                    var ports = provinceDetailse.coast.OrderBy(p => Rand.Next(10000000));
                    var port = town;
                    if (ports.Count() > 0)
                        port = ports.OrderBy(p => Rand.Next(10000000)).First();
                    var text = centre.OrderBy(p => Rand.Next(10000000)).First();
                    //provinceDetailse.points.Remove(provinceDetailse.border);

                    town.Y = Height - town.Y - 1;
                    army.Y = Height - army.Y - 1;
                    councillers.Y = Height - councillers.Y - 1;
                    port.Y = Height - port.Y - 1;
                    text.Y = Height - text.Y - 1;

                    filein.WriteLine(provinceDetailse.ID + "=");
                    filein.WriteLine("{");
                    filein.WriteLine("position=");
                    filein.WriteLine("{");
                    filein.WriteLine(town.X + ".000 " + town.Y + ".000 " + army.X + ".000 " + army.Y + ".000 " + councillers.X + ".000 " + councillers.Y + ".000 " + text.X + ".000 " + text.Y + ".000 " + port.X + ".000 " + port.Y + ".000" );
                    filein.WriteLine("}");
                    filein.WriteLine("rotation=");
                    filein.WriteLine("{");
                    filein.WriteLine("0.000 0.000 0.000 0.000 0.000");
                    filein.WriteLine("}");
                    filein.WriteLine("height=");
                    filein.WriteLine("{");
                    filein.WriteLine("0.000 0.000 0.000 20.000 0.000");
                    filein.WriteLine("}");
                    filein.WriteLine("}");
                }
            }
        }

        private void LoadProvinceColors()
        {
            using (System.IO.StreamReader filein =
               new System.IO.StreamReader(Globals.GameDir + "map\\definition.csv", Encoding.GetEncoding(1252)))
            {
                int idd = 1;
                string line = "";
                int count = 0;
                while ((line = filein.ReadLine()) != null)
                {
                    //  if (count > 0)
                    {
                        count++;
                        if (count == 1)
                            continue;
                        if (line.Length == 0)
                            continue;
                        var sp = line.Split(';');

                        int id = idd++;//Convert.ToInt32(sp[0]);
                        int r = Convert.ToInt32(sp[1]);
                        int g= Convert.ToInt32(sp[2]);
                        int b = Convert.ToInt32(sp[3]);

                        Color col = Color.FromArgb(255, r, g, b);
                        colorMap[id] = col;
                        invColorMap[col] = id;
                    }
                }

                filein.Close();
            }
        }

        public NoiseGenerator Noise { get; set; }

        public class TerritoryPoint
        {
            public Point Position;
            public float Distance;
            public int Owner;

            public TerritoryPoint(int x, int y)
            {
                Owner = -1;
                Position = new Point(x, y);
            }

            public bool Sea { get; set; }
        }
        private int SortByDistance(TerritoryPoint x, TerritoryPoint y)
        {
            float dist = x.Distance;
            float dist2 = y.Distance;

            if (dist < dist2)
                return -1;
            if (dist > dist2)
                return 1;

            return 0;
        }

        List<Color> colours;
        List<TerritoryPoint> points;
        int numTerritories = 1500;
        const int maxPointsPerTerritory = 25;
        const int minPointsPerTerritory = 8;
        private int numPoints = 0;// numTerritories * 20;

        private int seaStart = 0;

        public bool isCoast(int x, int y)
        {
            if (generatedMap.TerrainMap.GetPixel(x, y) ==
                Color.FromArgb(255, 69, 91, 186))
                return false;

            for (int xx = -1; xx <= 1; xx++)
            {
                for (int yy = -1; yy <= 1; yy++)
                {
                    if (xx == 0 && yy == 0)
                        continue;

                    if (generatedMap.TerrainMap.GetPixel(x, y) ==
                        Color.FromArgb(255, 69, 91, 186))
                        return true;
                }
            }

            return false;
        }

        public void Generate(int numTerritories = 1500)
        {
            this.numTerritories = numTerritories;
            numPoints = numTerritories * 50;

            #region GenerateRandomPoints
            this.generatedMap.TerrainMap.LockBits();
            ProvinceMap.LockBits();
            points = new List<TerritoryPoint>();

            int tot = (int)(((Width / RegionGridSize) * 0.4190996) * ((Height / RegionGridSize) * 0.4190996));
            List<Vector2f> vpoints = new List<Vector2f>();
            HashSet<Point> done = new HashSet<Point>();
            // int numSeaPoints = numPoints / 140;
            int numSeaPoints = numPoints / 340;
            int seaStart = numPoints - numSeaPoints;
            for (int n = 0; n < numPoints - numSeaPoints; n++)
            {
                TerritoryPoint newPoint = null;
                bool duplicate = true;


                while (duplicate)
                {
                    duplicate = false;

                    do
                    {
                        newPoint = new TerritoryPoint(Rand.Next(Width), Rand.Next(Height));

                    } while (done.Contains(newPoint.Position));


                    if (generatedMap.TerrainMap.GetPixel(newPoint.Position.X, newPoint.Position.Y) ==
                        Color.FromArgb(255, 69, 91, 186))
                    {
                        duplicate = true;
                        continue;
                    }

                    done.Add(newPoint.Position);
                    /*
                                        foreach (TerritoryPoint p in points)
                                            if (p.Position.X == newPoint.Position.X && p.Position.Y == newPoint.Position.Y)
                                            {
                                                duplicate = true;
                                                break;
                                            }*/
                }

                if (newPoint != null)
                {
                    vpoints.Add(new Vector2f(newPoint.Position.X, newPoint.Position.Y));
                    points.Add(newPoint);
                }
            }
            points.Clear();
            Voronoi v = new Voronoi(vpoints, new Rectf(0, 0, Width, Height), 32);
            var coords = v.SiteCoords();
            coords = coords.Where(c => generatedMap.TerrainMap.GetPixel((int) c.x, (int) c.y) != Color.FromArgb(255, 69, 91, 186)).ToList();
            for (int n = 0; n < coords.Count; n++)
            {
                TerritoryPoint newPoint = null;
                bool duplicate = true;

                newPoint = new TerritoryPoint((int)coords[n].x, (int)coords[n].y);

                if (newPoint != null)
                {
                    points.Add(newPoint);

                    int xx = newPoint.Position.X / RegionGridSize;
                    int yy = newPoint.Position.Y / RegionGridSize;
                    List<TerritoryPoint> list = null;
                    if (!Regions.ContainsKey(new Point(xx, yy)))
                    {
                        list = new List<TerritoryPoint>();
                        Regions[new Point(xx, yy)] = list;
                    }
                    else
                    {
                        list = Regions[new Point(xx, yy)];
                    }
                   
                    newPoint.Sea = false;
                  
                    list.Add(newPoint);
                }
            }

            #endregion


            #region GenerateTerritories
            List<TerritoryPoint> assigned = new List<TerritoryPoint>();
            List<TerritoryPoint> unassigned = new List<TerritoryPoint>();

            colours = new List<Color>();
            var origPoints = new List<TerritoryPoint>(points);
            for (int n = 0; n < numTerritories; n++)
            {
                TerritoryPoint initialPoint = null;
                int i = 0;//rand.Next(points.Count());
                while (initialPoint == null)
                {
                    i = Rand.Next(origPoints.Count());
                    initialPoint = origPoints[i];
                    if (initialPoint.Owner != -1)
                        initialPoint = null;
                    else
                    {
                        i = points.IndexOf(initialPoint);
                    }

                }
                bool requireSea = false;
                if (initialPoint.Sea)
                {
                    requireSea = true;
                }
                foreach (TerritoryPoint p in points)
                {
                    int dx = p.Position.X - initialPoint.Position.X;
                    int dy = p.Position.Y - initialPoint.Position.Y;
                    p.Distance = (float)Math.Sqrt(dx * dx + dy * dy);
                }

                points.Sort(SortByDistance);

                int c = 0;
                if (i >= seaStart)
                    c = 1;
                else
                    for (c = 0; c < maxPointsPerTerritory; c++)
                        if (points[c].Owner != -1) break;

                if (c >= minPointsPerTerritory || i >= seaStart)
                {
                    for (int c2 = 0; c2 < c; c2++)
                    {

                        if (points[c2].Owner == -1)
                        {
                            points[c2].Owner = n;
                            origPoints.Remove(points[c2]);
                        }

                    }
                }
                else
                {
                    n--;
                }
            }

            #endregion

            #region GenerateBitmap
            int[] provinceSize = new int[numTerritories + 100];
            Color[] provinceColor = new Color[numTerritories + 100];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    float minDist = 1000000000;
                    TerritoryPoint closest = null;

                    if (generatedMap.TerrainMap.GetPixel(x, y) ==
                        Color.FromArgb(255, 69, 91, 186))
                    {
                        ProvinceMap.SetPixel(x, y, Color.White);

                        continue;
                    }
                    List<TerritoryPoint> list = new List<TerritoryPoint>();

                    bool found = false;

                    int range = 1;
                    while (!found)
                    {
                        for (int xx = -range; xx <= range; xx++)
                        {
                            for (int yy = -range; yy <= range; yy++)
                            {
                                int gx = x / RegionGridSize;
                                int gy = y / RegionGridSize;

                                int tx = xx + gx;
                                int ty = yy + gy;

                                if (Regions.ContainsKey(new Point(tx, ty)))
                                {
                                    var l = Regions[new Point(tx, ty)];

                                    list.AddRange(l.Where(p => p.Owner != -1));
                                }


                            }
                        }

                        if (list.Count > 1)
                            break;

                        range++;
                    }


                    foreach (TerritoryPoint p in list)
                    {
                        int dx = p.Position.X - x;
                        int dy = p.Position.Y - y;
                        p.Distance = (float)Math.Sqrt(dx * dx + dy * dy);

                        if (p.Owner != -1 && p.Distance < minDist)
                        {
                            closest = p;
                            minDist = p.Distance;
                        }
                    }

                    if (closest.Owner != -1)
                    {
                        provinceSize[closest.Owner]++;

                        var col = colorMap[closest.Owner + 1];
                        ProvinceMap.SetPixel(x, y, col);

                    }
                }
            ProvinceMap.UnlockBits();

            ProvinceMap.Save24(Globals.MapOutputTotalDir + "map\\provincestest.bmp");
            #endregion
        }

        public void GenerateOld(int numTerritories = 1500)
        {
      

        
        }

     

        public class OceanZone
        {
            public List<SeaZone> seaZones = new List<SeaZone>();
        }
        public class SeaZone
        {
            public int ID;
            public int From { get; set; }
            public int To { get; set; }
            public List<ProvinceBitmapManager.Province> Provinces = new List<ProvinceBitmapManager.Province>();

        }

        public List<ProvinceDetails> ProvinceLand { get; set; }

        public void SaveDefinitions()
        {
            Provinces = Provinces.OrderBy(p => p.ID).ToList();
                        int n = 0;
            //  File.Mutate(filename, filename);
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(Globals.MapOutputTotalDir + "map\\definition.csv", false, Encoding.GetEncoding(1252)))
            {
                file.Write("province;red;green;blue;x;x" + Environment.NewLine);
                foreach (var def in Provinces)
                {
                    file.Write( def.ID + ";" + def.Color.R + ";" + def.Color.G + ";" + def.Color.B + ";x;x" + Environment.NewLine);
                }

                file.Close();
            }

        }
        public List<ProvinceDetails> Provinces { get; set; }
        public List<ProvinceDetails> ProvinceSea { get; set; }

        public Dictionary<Color, ProvinceDetails> ProvinceKey { get; set; }

        public class ProvinceDetails
        {
            public List<Point> points = new List<Point>();
            public List<Point> coast = new List<Point>();
            public List<Point> border = new List<Point>();
            public Color Color { get; set; }
            public int ID { get; set; }
            public bool isSea { get; set; }
            public int Size { get; set; }
            public SeaZone seaZone { get; set; }

            public Point min = new Point(100000,100000);
            public Point max = new Point(-100000, -100000);
            public List<ProvinceDetails> adjacent = new List<ProvinceDetails>();
        }
        private void DoProvinceData()
        {
          
        }


    }
}
