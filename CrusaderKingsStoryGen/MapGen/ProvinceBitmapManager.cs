using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using csDelaunay;
using FloodFill2;
using LibNoise.Modfiers;

namespace CrusaderKingsStoryGen.MapGen
{
    public class ProvinceBitmapManager
    {

        public static ProvinceBitmapManager instance = new ProvinceBitmapManager();

        public class Province
        {
            public List<Point> points = new List<Point>(); 
            public Color Color { get; set; }
            public Color AreaColor { get; set; }
            public bool isSea { get; set; }
            public List<Point> border = new List<Point>();
            public List<Point> coast = new List<Point>();
            public List<Province> adjacent = new List<Province>();
            public List<Point> barrier = new List<Point>();


            public Point min = new Point(10000000,1000000);
            public Point max = new Point(-10000000, -1000000);
            public MapGenManager.SeaZone seaZone;
            public int ID;
       }
        public List<Province> provinces = new List<Province>();
        public Dictionary<Color, Province> colorProvinceMap = new Dictionary<Color, Province>();
        public HashSet<Color> areaColors = new HashSet<Color>();
        Color last = Color.White;
        private float provincesDelta = 1.0f;
        public void Init(LockBitmap bmp, float sizeDelta, GeneratedTerrainMap generatedMap)
        {
            provincesDelta = sizeDelta;
            if (provincesDelta < 0.5f)
                provincesDelta *= 1.8f;

            LoadProvinceColors();
            Color col = Color.FromArgb(255, 1, 0, 0);
            
            int r = 1;
            int g = 0;
            int b = 0;
            float highmountainLevel = 0.7f;
            float DivNoise = 1200.0f * (generatedMap.Height / 2048.0f);

            NoiseGenerator noiseH = new NoiseGenerator();
            NoiseGenerator noiseM = new NoiseGenerator();

            generatedMap.Map.LockBits();
            generatedMap.MoistureMap.LockBits();
            generatedMap.HeatMap.LockBits();
            SolidBrush br = new SolidBrush(Color.Black);
            using(Graphics gg = Graphics.FromImage(bmp.Source))
                for (int x = 0; x < generatedMap.Map.Width; x++)
                {
                    for (int y = 0; y < generatedMap.Map.Height; y++)
                    {
                        float height = generatedMap.Map.GetHeight(x, y);
                    
                        if (height >= highmountainLevel * 255)
                        {
                            gg.FillEllipse(br, new Rectangle(x - 2, y - 2, 4, 4));
                        }


                    }
                }
            LockBitmap bmp2 = new LockBitmap(new Bitmap(bmp.Source));
            bmp2.LockBits();
            using (Graphics gg = Graphics.FromImage(bmp.Source))
                for (int x = 0; x < generatedMap.Map.Width; x++)
            {
                for (int y = 0; y < generatedMap.Map.Height; y++)
                {
                    float heat = generatedMap.HeatMap.GetHeight(x, y) / 255.0f;// + (Rand.Next(-30, 30) / 8000.0f);
                    float moisture = generatedMap.MoistureMap.GetHeight(x/4, y/4) / 255.0f;// + (Rand.Next(-30, 30) / 8000.0f);

                    float no = (float)((float)((noiseH.Noise(x / DivNoise / 8, y / DivNoise / 8)) - 0.5f) * 0.2);
                    heat += no * 1.4f;
                    moisture -= no * 0.5f;

                    float desertLevel = 0.48f;
                    float desertDry = 0.28f;
                    bool hot = heat > desertLevel;
                    bool dry = moisture <= desertDry;

                    if (hot && dry && bmp2.GetPixel(x, y)== Color.FromArgb(255, 130, 158, 75))
                    {
                            gg.FillEllipse(br, new Rectangle(x - 5, y - 5, 10, 10));
                     }


                }
            }
            bmp2.UnlockBits();
            generatedMap.Map.UnlockBits();
            generatedMap.MoistureMap.UnlockBits();
            generatedMap.HeatMap.UnlockBits();
//            bmp.Save24(Globals.MapOutputTotalDir + "testprovinces.bmp");
            bmp.LockBits();
            UnsafeQueueLinearFloodFiller filler = new UnsafeQueueLinearFloodFiller(null);
            filler.Bitmap = bmp;
            Bitmap = bmp;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
              
                    Color pix = bmp.GetPixel(x, y);
                    if (pix == Color.FromArgb(255, 69, 91, 186) || pix == Color.FromArgb(255, 130, 158, 75))
                    {
                        areaColors.Add(col);
                        filler.FillColor = col;
                        filler.FloodFill(new Point(x, y));

                        bool valid = false;
                        foreach (var point in filler.pts)
                        {
                            for (int yy = -1; yy <= 1; yy++)
                            {
                                for (int xx = -1; xx <= 1; xx++)
                                {
                                    if (xx == 0 && yy == 0)
                                        continue;
                                    if (point.X + xx < 0 || point.Y + yy < 0 || point.X + xx >= bmp.Width || point.Y + yy >= bmp.Height)
                                        continue;

                                    var ccc = bmp.GetPixel(xx + point.X, yy + point.Y);
                                    if (col.R == ccc.R && ccc.G == col.G && ccc.B == col.B)
                                        continue;
                                    if (ccc.R > 0 || ccc.G > 0 || ccc.B > 0)
                                    {
                                        valid = true;
                                    }
                                }
                            }
                        }

                        if (!valid)
                        {
                            filler.FillColor = Color.Black;
                            filler.FloodFill(new Point(x, y));
                            continue;
                        }

                        {
                            int numProvinces = (int) (filler.pts.Count / (3000));
                            if (pix == Color.FromArgb(255, 130, 158, 75))
                                numProvinces = (int) (numProvinces * sizeDelta);
                            if (numProvinces == 0)
                                numProvinces = 1;
                            if (pix != Color.FromArgb(255, 130, 158, 75))
                            {
                                numProvinces /= 6;
                                if (numProvinces < 1)
                                    numProvinces = 1;
                            }
                            CreateProvinces(numProvinces, col, filler.pts, bmp.Width, bmp.Height, bmp, pix != Color.FromArgb(255, 130, 158, 75));
                            FixupProvinces(col, filler, bmp);
                        }
                        r += 1;
                        if (r > 255)
                        {
                            r = 0;
                            g += 1;
                        }
                        if (g > 255)
                        {
                            g = 0;
                            b += 1;
                        }
                        col = Color.FromArgb(255, r, g, b);
                    }
                }
            }
            provinces = provinces.Distinct().ToList();
            foreach (var province in provinces)
            {
                province.points.Clear();
                colorProvinceMap[province.Color] = province;
            }
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pix = bmp.GetPixel(x, y);
                    if (colorProvinceMap.ContainsKey(pix))
                    {
                        colorProvinceMap[pix].points.Add(new Point(x, y));
                    }
                    else
                    {
                        
                    }
                }
            }

            CalculateDetails();
           
            bmp.UnlockDirect();
         

        //    bmp.Save24("C:\\Users\\LEMMY\\Documents\\terrainNew.bmp");

          
    
        }

        private void CalculateDetails()
        {


            int cc = 1;
            ProvinceSea.Clear();
            ProvinceLand.Clear();
            

            provinces = provinces.Distinct().OrderBy(p => p.isSea).ToList();
            int ii = 1;
            for (int index = 0; index < provinces.Count; index++)
            {
                var provinceDetailse = provinces[index];
                if (provinceDetailse.points.Count == 0)
                {
                    provinces.Remove(provinceDetailse);
                     index--;
                    continue;
                }
                if(provinceDetailse.isSea)
                    ProvinceSea.Add(provinceDetailse);
                else
                    ProvinceLand.Add(provinceDetailse);
                provinceDetailse.ID = ii;
             
                ii++;
            }

            foreach (var provinceDetails in provinces)
            {
                foreach (var point in provinceDetails.points)
                {
                    int x = point.X;
                    int y = point.Y;
                    if (x < provinceDetails.min.X)
                    {
                        provinceDetails.min.X = x;
                    }
                    if (y < provinceDetails.min.Y)
                    {
                        provinceDetails.min.Y = y;
                    }
                    if (x > provinceDetails.max.X)
                    {
                        provinceDetails.max.X = x;
                    }
                    if (y > provinceDetails.max.Y)
                    {
                        provinceDetails.max.Y = y;
                    }
                }
            }
            for (int y = 0; y < Bitmap.Height; y++)
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    var pixel = Bitmap.GetPixel(x, y);
                    if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                        continue;
                    var provinceDetails = colorProvinceMap[pixel];


                    for (int yy = -1; yy <= 1; yy++)
                    {
                        for (int xx = -1; xx <= 1; xx++)
                        {
                            if (xx == 0 && yy == 0)
                                continue;
                            if (x + xx < 0 || y + yy < 0 || x + xx >= Bitmap.Width || y + yy >= Bitmap.Height)
                                continue;

                            var pixel2 = Bitmap.GetPixel(x + xx, y + yy);
                            if (pixel2.R == 0 && pixel2.G == 0 && pixel2.B == 0)
                            {
                                provinceDetails.barrier.Add(new Point(x+xx, y+yy));

                            }
                            else
                            {
                                var provinceDetails2 = colorProvinceMap[pixel2];
                                if (provinceDetails2 != provinceDetails)
                                {
                                    provinceDetails.border.Add(new Point(x + xx, y + yy));
                                    if (!provinceDetails2.adjacent.Contains(provinceDetails))
                                    {
                                        provinceDetails2.adjacent.Add(provinceDetails);
                                        provinceDetails.adjacent.Add(provinceDetails2);

                                    }
                                }
                                if (provinceDetails2.isSea)
                                    provinceDetails.coast.Add(new Point(x + xx, y + yy));
                            }
                         

                        }
                    }

                }




            var seaProv2 = new List<Province>(ProvinceSea);

            List<MapGenManager.SeaZone> seaZones = new List<MapGenManager.SeaZone>();

            // get groups of sea provinces
            // number then sequentially in definitions
            // create ocean zones
            List<Province> dones = new List<Province>();
            while (seaProv2.Count > 0)
            {
                List<Province> chosen = new List<Province>();
                chosen.Add(seaProv2[0]);
                seaProv2.Remove(seaProv2[0]);
                int lastCount = chosen.Count + 1;
                while (chosen.Count != lastCount && chosen.Count < 30)
                {
                    lastCount = chosen.Count;
                    for (int index = 0; index < chosen.Count; index++)
                    {
                        var provinceDetailse = chosen[index];
                        var adj = provinceDetailse.adjacent.Where(a => a.isSea && !dones.Contains(a)).ToList();
                        chosen.AddRange(adj);
                        dones.AddRange(adj);

                        foreach (var detailse in adj)
                        {
                            seaProv2.Remove(detailse);
                        }

                        if (chosen.Count >= 30)
                            break;
                    }
                }

                var sz = new MapGenManager.SeaZone() { ID = seaZones.Count + 1, Provinces = chosen.Distinct().ToList() };
                seaZones.Add(sz);
                foreach (var provinceDetailse in chosen)
                {
                    provinceDetailse.seaZone = sz;
                }

            }

            int id = ProvinceLand.Count + 1;
            foreach (var seaZone in seaZones)
            {
                foreach (var provinceDetailse in seaZone.Provinces)
                {
                    provinceDetailse.ID = id++;
                }
            }

            List<MapGenManager.SeaZone> toDo = new List<MapGenManager.SeaZone>(seaZones);

            List<MapGenManager.OceanZone> oceanZones = new List<MapGenManager.OceanZone>();

            while (toDo.Count > 0)
            {
                List<MapGenManager.SeaZone> chosenZones = new List<MapGenManager.SeaZone>();
                chosenZones.Add(toDo[0]);
                int lastChosen = chosenZones.Count + 1;
                toDo.Remove(toDo[0]);
                while (chosenZones.Count != lastChosen)
                {
                    lastChosen = chosenZones.Count;
                    for (int index = 0; index < chosenZones.Count; index++)
                    {
                        var seaZone = chosenZones[index];
                        foreach (var provinceDetailse in seaZone.Provinces)
                        {
                            var adj = provinceDetailse.adjacent.Where(a => a.isSea && a.seaZone != seaZone);
                            foreach (var detailse in adj)
                            {
                                if (toDo.Contains(detailse.seaZone))
                                {
                                    chosenZones.Add(detailse.seaZone);
                                    toDo.Remove(detailse.seaZone);
                                }
                            }
                        }
                    }
                }

                MapGenManager.OceanZone z = new MapGenManager.OceanZone() { seaZones = chosenZones };

                oceanZones.Add(z);


            }
            this.provinces = this.provinces.OrderBy(p => p.ID).ToList();

            CalculatePositions();
           // CalculateClimate();
            this.OceanZones = oceanZones;
            this.SeaZones = seaZones;
            SaveDefinitions();
            SaveDefaultMap();
        }

        private LockBitmap heat;
        public void CalculateClimate(LockBitmap heatMap)
        {
            var provinces = new List<Province>(this.ProvinceLand);

            this.heat = heatMap;
            heatMap.LockBits();
            using (System.IO.StreamWriter filein =
                new System.IO.StreamWriter(Globals.MapOutputTotalDir + "map\\climate.txt", false,
                    Encoding.GetEncoding(1252)))
            {
                provinces.Sort(SortByHeat);

                int numHeavy = provinces.Count/7;
               int numNormal = provinces.Count/6;
                int numLight = provinces.Count/5;
                if (Globals.Climate == 3)
                {
                    numHeavy /= 2;
                    numNormal /= 2;
                    numHeavy /= 2;
                }
                if (Globals.Climate == 4)
                {
                    numHeavy = 0;
                    numNormal = 0;
                    numHeavy = 0;
                }

                numNormal += numHeavy;
                numLight += numNormal;
                List<Province> heavy = new List<Province>();
                List<Province> normal = new List<Province>();
                List<Province> light = new List<Province>();
                int c = 1;
                String h = "";
                String n = "";
                String l = "";
                for (int x = 0; x < provinces.Count; x++)
                {
                    var p = provinces[x];

                    if (c < numHeavy)
                    {
                        heavy.Add(p);
                        h = h + " " + p.ID;
                    }
                    else if (c < numNormal)
                    {
                        normal.Add(p);
                        n = n + " " + p.ID;
                    }
                    else if (c < numLight)
                    {
                        light.Add(p);
                        l = l + " " + p.ID;
                    }
                    c++;

                }

                filein.WriteLine("mild_winter = {");
                filein.WriteLine(l);
                filein.WriteLine("}");
                filein.WriteLine("normal_winter = {");
                filein.WriteLine(n);
                filein.WriteLine("}");
                filein.WriteLine("severe_winter = {");
                filein.WriteLine(h);
                filein.WriteLine("}");

            }

            heatMap.UnlockBits();
            foreach (var value in colorProvinceMap.Values)
            {
                value.points.Clear();
                value.border.Clear();
                value.coast.Clear();
                value.adjacent.Clear();

            }
            this.provinces.Clear();
            ProvinceSea.Clear();
            ProvinceLand.Clear();
            OceanZones.Clear();
            SeaZones.Clear();
        
        }

        public int SortByHeat(Province x, Province y)
        {
            var p1 = x.points[0];
            var p2 = y.points[0];

            float h = heat.GetPixel(p1.X, p1.Y).R / 255.0f;
            float h2 = heat.GetPixel(p2.X, p2.Y).R / 255.0f;

            if (h > h2)
                return 1;
            if (h < h2)
                return -1;
            return 0;
        }

        private void SaveDefinitions()
        {
            int n = 0;
            //  File.Mutate(filename, filename);
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(Globals.MapOutputTotalDir + "map\\definition.csv", false, Encoding.GetEncoding(1252)))
            {
                file.Write("province;red;green;blue;x;x" + Environment.NewLine);
                foreach (var def in provinces)
                {
                    file.Write(def.ID + ";" + def.Color.R + ";" + def.Color.G + ";" + def.Color.B + ";x;x" + Environment.NewLine);
                }

                file.Close();
            }
        }

        public List<Province> ProvinceLand = new List<Province>();

        public List<MapGenManager.SeaZone> SeaZones = new List<MapGenManager.SeaZone>();

        public List<MapGenManager.OceanZone> OceanZones = new List<MapGenManager.OceanZone>();

        public List<Province> ProvinceSea = new List<Province>();


        private void CalculatePositions()
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
                    var centre = provinceDetailse.points.Where(p => rect.Contains(p)).ToList();
                    if(centre.Count==0)
                        centre.AddRange(provinceDetailse.points);
                    var town = centre.OrderBy(p => Rand.Next(10000000)).First();
                    var army = centre.OrderBy(p => Rand.Next(10000000)).First();
                    var councillers = centre.OrderBy(p => Rand.Next(10000000)).First();
                    var ports = provinceDetailse.coast.OrderBy(p => Rand.Next(10000000));
                    var port = town;
                    if (ports.Count() > 0)
                        port = ports.OrderBy(p => Rand.Next(10000000)).First();
                    var text = centre.OrderBy(p => Rand.Next(10000000)).First();
                    //provinceDetailse.points.Remove(provinceDetailse.border);

                    town.Y = Bitmap.Height - town.Y - 1;
                    army.Y = Bitmap.Height - army.Y - 1;
                    councillers.Y = Bitmap.Height - councillers.Y - 1;
                    port.Y = Bitmap.Height - port.Y - 1;
                    text.Y = Bitmap.Height - text.Y - 1;

                    filein.WriteLine(provinceDetailse.ID + "=");
                    filein.WriteLine("{");
                    filein.WriteLine("position=");
                    filein.WriteLine("{");
                    filein.WriteLine(town.X + ".000 " + town.Y + ".000 " + army.X + ".000 " + army.Y + ".000 " + councillers.X + ".000 " + councillers.Y + ".000 " + text.X + ".000 " + text.Y + ".000 " + port.X + ".000 " + port.Y + ".000");
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


        public LockBitmap Bitmap { get; set; }

        private void FixupProvinces(Color col, UnsafeQueueLinearFloodFiller filler, LockBitmap bmp)
        {
            HashSet<Color> done = new HashSet<Color>();
            var pts = new List<Point>(filler.pts);

            foreach (var point in pts)
            {
                Color test = bmp.GetPixel(point.X, point.Y);
                if (done.Contains(test))
                    continue;
                if (test.R == 0 && test.B == 0 && test.G == 0)
                    continue;

                Province p = colorProvinceMap[test];
             
                filler.FillColor = Color.White;
                filler.FloodFill(point);

                Color t = bmp.GetPixel(676, 1935);

                if (t != last)
                {

                }
                last = t;
                bool trimmed = false;

                int minX = 1000000;
                int minY = 1000000;
                int maxX = -1000000;
                int maxY = -1000000;

                foreach (var pt in filler.pts)
                {
                    if (pt.X < minX)
                        minX = pt.X;
                    if (pt.Y < minY)
                        minY = pt.Y;

                    if (pt.X > maxX)
                        maxX = pt.X;
                    if (pt.Y > maxY)
                        maxY = pt.Y;

                }

                bool tooSmall = false;

                if (maxX - minX < 16 || maxY - minY < 16)
                    tooSmall = true;

                if (filler.pts.Count < 100 || tooSmall)
                {
                    if (filler.pts.Count == p.points.Count)
                    {
                        done.Add(test);
                    }
                    Color newCol = FindBestNeighbour(filler.pts, test, true);
                    if (newCol == Color.White)
                    {
                        newCol = FindBestNeighbour(filler.pts, test, false);
                    }

                    if (newCol != Color.White)
                    {
                        filler.FillColor = newCol;
                        filler.FloodFill(point);
                        trimmed = true;
                        if (newCol.R == 0 && newCol.B == 0 && newCol.G == 0)
                            continue;

                        colorProvinceMap[newCol].points.AddRange(filler.pts);
                    }
                    else
                    {
                    //    filler.FillColor = Color.Black;
                     //   filler.FloodFill(point);
                     //   trimmed = true;

                    }
                    if (!trimmed)
                    {
                        
                    }
                }

                if (!trimmed)
                {
                    if (filler.pts.Count == p.points.Count)
                    {

                        filler.FillColor = test;
                        filler.FloodFill(point);
                        done.Add(test);

                        t = bmp.GetPixel(676, 1935);

                        if (t != last)
                        {

                        }
                        last = t;
                    }
                    else
                    {
                        Province pNew = new Province();
                        pNew.AreaColor = col;
                        do
                        {
                            pNew.Color = colorMap[totalGeneratedProvinces++];

                        } while ((pNew.Color.G == 0 && pNew.Color.B == 0) || pNew.Color == Color.FromArgb(255, 69, 91, 186) || pNew.Color == Color.FromArgb(255, 130, 158, 75) || colorProvinceMap.ContainsKey(pNew.Color));

                        filler.FillColor = pNew.Color;
                        filler.FloodFill(point);
                        t = bmp.GetPixel(676, 1935);

                        if (t != last)
                        {

                        }
                        last = t;

                        provinces.Add(pNew);
                        colorProvinceMap[pNew.Color] = pNew;
                        done.Add(pNew.Color);
                        pNew.points.AddRange(filler.pts);
                        pNew.points.ForEach(o => p.points.Remove(o));
                        pNew.isSea = p.isSea;
                    }
                }
           
            }
        }

        private Color FindBestNeighbour(List<Point> pts, Color col, bool careAboutSea)
        {
            Dictionary<Color, int> count = new Dictionary<Color, int>();
            bool isSea = colorProvinceMap[col].isSea;
            foreach (var point in pts)
            {
                int x = point.X;
                int y = point.Y;
                for (int yy = -1; yy <= 1; yy++)
                {
                    for (int xx = -1; xx <= 1; xx++)
                    {
                        if (xx == 0 && yy == 0)
                            continue;
                        if (x + xx < 0 || y + yy < 0 || x + xx >= Bitmap.Width || y + yy >= Bitmap.Height)
                            continue;

                        var pixel2 = Bitmap.GetPixel(x + xx, y + yy);
                        if (pixel2.R == 0 && pixel2.G == 0 && pixel2.B == 0)
                        {
                            if (count.ContainsKey(pixel2))
                                count[pixel2]++;
                            else
                            {
                                count[pixel2] = 1;
                            }
                            continue;
                        }
                        if (pixel2 != col && colorProvinceMap.ContainsKey(pixel2) && (!careAboutSea || (colorProvinceMap[pixel2].isSea == isSea)))
                        {
                            if (count.ContainsKey(pixel2))
                                count[pixel2]++;
                            else
                            {
                                count[pixel2] = 1;
                            }
                        }
                    }
                }
            }
            Color biggest = Color.White;
            int biggestCount = 0;

            foreach (var i in count)
            {
                if (i.Value > biggestCount)
                {
                    biggest = i.Key;
                    biggestCount = i.Value;
                }
            }

            return biggest;
        }

        public Dictionary<Point, List<TerritoryPoint>> Regions = new Dictionary<Point, List<TerritoryPoint>>();

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
            public TerritoryPoint()
            {
                Owner = -1;
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
        public int RegionGridSize { get; set; } = 128;

        private void AddPointToRegion(TerritoryPoint newPoint)
        {
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


            list.Add(newPoint);
        }

        private void SaveDefaultMap()
        {
         
            String str = @"
max_provinces = " + (provinces.Count + 1) + @"
definitions = ""definition.csv""
provinces = ""provinces.bmp""
positions = ""positions.txt""
terrain = ""terrain.bmp""
rivers = ""rivers.bmp""
terrain_definition = ""terrain.txt""
heightmap = ""topology.bmp""
tree_definition = ""trees.bmp""
continent = ""continent.txt""
adjacencies = ""adjacencies.csv""
climate = ""climate.txt""
region = ""island_region.txt""
geographical_region = ""geographical_region.txt""
static = ""statics""
seasons = ""seasons.txt""

";

            String filename = Globals.MapOutputTotalDir + "map\\default.map";
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {

                file.Write(str);
                foreach (var seaZone in SeaZones)
                {
                    seaZone.Provinces = seaZone.Provinces.OrderBy(a => a.ID).ToList();
                    file.WriteLine("sea_zones = { " + seaZone.Provinces[0].ID + " " + seaZone.Provinces[seaZone.Provinces.Count - 1].ID + "}");


                }
                foreach (var oceanZone in OceanZones)
                {
                    String strr = "";

                    foreach (var seaZone in oceanZone.seaZones)
                    {
                        strr += seaZone.ID + " ";
                    }
                    file.WriteLine(@"ocean_region = { 
     sea_zones = { " + strr + @"}
}");


                }

            }



        }


        const int maxPointsPerTerritory = 25;
        const int minPointsPerTerritory = 8;
        public Dictionary<int, Color> colorMap = new Dictionary<int, Color>();
        Dictionary<Color, int> invColorMap = new Dictionary<Color, int>();
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
                        int g = Convert.ToInt32(sp[2]);
                        int b = Convert.ToInt32(sp[3]);

                        Color col = Color.FromArgb(255, r, g, b);
                        colorMap[id] = col;
                        invColorMap[col] = id;
                    }
                }

                filein.Close();
            }
        }
        int[] provinceSize = new int[20000];

        private int totalGeneratedProvinces = 0;
        private void CreateProvinces(int numProvinces, Color testColor, List<Point> pts, int w, int h, LockBitmap provinces, bool bSea)
        {
            Regions.Clear();
            int numPoints = numProvinces * 50;
            if (bSea)
                numPoints = numProvinces*50;

            #region GenerateRandomPoints

            var points = new List<TerritoryPoint>();
            var usePts = new HashSet<Point>(pts);
            List<Vector2f> vpoints = new List<Vector2f>();
            HashSet<Point> done = new HashSet<Point>();
            // int numSeaPoints = numPoints / 140;
            int numSeaPoints = numPoints / 340;
            for (int n = 0; n < numPoints; n++)
            {
                Point newPoint = Point.Empty;
                bool duplicate = true;


             
                {
                  

                    int nn = Rand.Next(pts.Count);
                    newPoint = pts[nn];

                 //   pts.RemoveAt(nn);

                    /*
                                        foreach (TerritoryPoint p in points)
                                            if (p.Position.X == newPoint.Position.X && p.Position.Y == newPoint.Position.Y)
                                            {
                                                duplicate = true;
                                                break;
                                            }*/
                }

             
                {
                    vpoints.Add(new Vector2f(newPoint.X, newPoint.Y));
                
                }
            }
            points.Clear();
            int vorStrength = 0;
            if (pts.Count > 150)
                vorStrength += 2;
            Voronoi v = new Voronoi(vpoints, new Rectf(0, 0, w, h), 0);
            var coords = v.SiteCoords();
            coords = coords.Where(c => usePts.Contains(new Point((int) c.x, (int) c.y))).ToList();
            for (int n = 0; n < coords.Count; n++)
            {
                TerritoryPoint newPoint = null;
                bool duplicate = true;

                newPoint = new TerritoryPoint((int)coords[n].x, (int)coords[n].y);

                if (newPoint != null)
                {
                    points.Add(newPoint);

                }
            }

            #endregion

            if (points.Count == 0)
            {
                points.Add(new TerritoryPoint() {Position = usePts.OrderBy(o=>Rand.Next(1000000)).First()});
            }

            #region GenerateTerritories

            var colours = new List<Color>();
            var origPoints = new List<TerritoryPoint>(points);
            for (int n = 0; n < numProvinces; n++)
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
           
                for (c = 0; c < Math.Min(maxPointsPerTerritory, points.Count); c++)
                    if (points[c].Owner != -1) break;

                int owner = n + totalGeneratedProvinces;
                while ((colorMap[owner + 1].G==0 && colorMap[owner + 1].B==0) || colorMap[owner + 1] == Color.FromArgb(255, 69, 91, 186) || colorMap[owner + 1] == Color.FromArgb(255, 130, 158, 75) || colorProvinceMap.ContainsKey(colorMap[owner+1])) 
                {
                    owner = n + (++totalGeneratedProvinces);

                } 


                // if (c >= minPointsPerTerritory)// || i >= seaStart)
                {
                    for (int c2 = 0; c2 < c; c2++)
                    {

                        if (points[c2].Owner == -1)
                        {

                            points[c2].Owner = owner;
                            origPoints.Remove(points[c2]);
                            AddPointToRegion(points[c2]);
                        }

                    }
                }

                totalGeneratedProvinces++;

            }

            #endregion

            #region GenerateBitmap
            foreach (var point in usePts)
            {
                int x = point.X;
                int y = point.Y;
          
                    float minDist = 1000000000;
                    TerritoryPoint closest = null;

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
                   
                        var col = colorMap[closest.Owner + 1];
                        if (!colorProvinceMap.ContainsKey(col))
                        {
                            var p = new Province();
                            p.Color = col;
                            p.isSea = bSea;
                            colorProvinceMap[col] = p;
                            this.provinces.Add(p);
                            
                        }
                        colorProvinceMap[col].points.Add(new Point(x, y));
                        provinces.SetPixel(x, y, col);

                    }
                }

            if (!bSea)
            {
                List<Point> choices = new List<Point>();
                var pt = usePts.OrderBy(o => Rand.Next(10000000));
                for (int nn = 0; nn < 2; nn++)
                {
                    foreach (var point in pt)
                    {
                        int x = point.X;
                        int y = point.Y;

                        Color use = provinces.GetPixel(x, y);
                        choices.Clear();
                        for (int xx = -1; xx <= 1; xx++)
                        {
                            for (int yy = -1; yy <= 1; yy++)
                            {
                                if (xx == 0 && yy == 0)
                                    continue;

                                Color test = provinces.GetPixel(x + xx, y + yy);
                                var p = new Point(x + xx, y + yy);
                                if (test != use)
                                    if (usePts.Contains(p))
                                        choices.Add(p);
                            }
                        }

                        if (choices.Count == 1)
                        {
                            Point p = choices[Rand.Next(choices.Count)];
                            colorProvinceMap[provinces.GetPixel(p.X, p.Y)].points.Remove(p);
                            provinces.SetPixel(p.X, p.Y, use);
                            colorProvinceMap[use].points.Add(p);

                        }

                    }
                }

                List<Color> choicesc = new List<Color>();
                foreach (var point in pt)
                {
                    int x = point.X;
                    int y = point.Y;

                    Color use = provinces.GetPixel(x, y);
                    choicesc.Clear();
                    for (int xx = -1; xx <= 1; xx++)
                    {
                        for (int yy = -1; yy <= 1; yy++)
                        {
                            if (xx == 0 && yy == 0)
                                continue;

                            Color test = provinces.GetPixel(x + xx, y + yy);
                            var p = new Point(x + xx, y + yy);
                            if (test != use)
                                if (usePts.Contains(p))
                                    choicesc.Add(test);
                        }
                    }

                    if (choices.Count >= 7)
                    {
                        Color p = choicesc[Rand.Next(choicesc.Count)];
                        colorProvinceMap[provinces.GetPixel(x, y)].points.Remove(new Point(x, y));
                        provinces.SetPixel(x, y, p);
                        colorProvinceMap[p].points.Add(new Point(x, y));
                    }

                }
            }
           
            #endregion
        }
    }
}
