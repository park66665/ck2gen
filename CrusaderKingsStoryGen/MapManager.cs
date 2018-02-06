using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrusaderKingsStoryGen.MapGen;
using CrusaderKingsStoryGen.Simulation;
using LibNoise.Modfiers;

namespace CrusaderKingsStoryGen
{
    class MapManager
    {
        public class ProvinceMapBitmap
        {
            public Bitmap Bitmap { get; set; }
            public Bitmap Outline { get; set; }
            public Point MapPoint { get; set; }
        }
        public Dictionary<int, ProvinceMapBitmap> ProvinceBitmaps = new Dictionary<int, ProvinceMapBitmap>();
        private ProvinceParser[,] ProvincePixelMap;
        public Bitmap ProvinceRenderBitmap;
        public float SizeMod = 1.0f;
        public float RenderMod = 1.0f;
        public static MapManager instance = new MapManager();
        public List<ProvinceParser> Provinces = new List<ProvinceParser>();
        public Dictionary<String, ProvinceParser> ProvinceMap = new Dictionary<string, ProvinceParser>();
        public Dictionary<int, ProvinceParser> ProvinceIDMap = new Dictionary<int, ProvinceParser>();
        public Dictionary<Color, ProvinceParser> ProvinceColorMap = new Dictionary<Color, ProvinceParser>();
        public List<ProvinceParser> SelectedProvinces = new List<ProvinceParser>();

        public float Zoom = 0.3f;
        public PointF Centre = new PointF(-1, 0);
        public void Save()
        {
            foreach (var provinceScript in provinceScripts)
            {
                provinceScript.Save();
            }
        }

        public void FindAdjacent(List<ProvinceParser> provinces, int c)
        {
            if (c == 0)
                c++;
            List<ProvinceParser> choices = new List<ProvinceParser>();
            foreach (var provinceParser in provinces)
            {
                foreach (var parser in provinceParser.Adjacent)
                {
                    if (!provinces.Contains(parser))
                        choices.Add(parser);

                }
            }
            for (int i = 0; i < c; i++)
            {
                if (choices.Count > 0)
                {
                    int cc = Rand.Next(choices.Count);

                    provinces.Add(choices[cc]);
                    choices.RemoveAt(cc);
                }
            }
        }

        public void FindAdjacent(List<ProvinceParser> provinces, int c, CharacterParser head)
        {
            List<ProvinceParser> choices = new List<ProvinceParser>();
            foreach (var provinceParser in provinces)
            {
                foreach (var parser in provinceParser.Adjacent)
                {
                    var t = parser.Title;
                    if (t == null)
                        continue;
                    if (!provinces.Contains(parser) && (head == null || (head == t.Holder || (head.PrimaryTitle.Rank == 2 && parser.Title.TopmostTitle == head.PrimaryTitle.TopmostTitle)) && (parser.Title.Liege == null || parser.Title.Rank == 2)))
                        choices.Add(parser);

                }
            }
            for (int i = 0; i < c; i++)
            {
                if (choices.Count > 0)
                {
                    int cc = Rand.Next(choices.Count);

                    provinces.Add(choices[cc]);
                    choices.RemoveAt(cc);
                }
            }
        }
        public List<Point> Ranges = new List<Point>();
        public void LoadDefaultMap()
        {
            String filename = Globals.MapDir + "map\\default.map";
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(filename, Encoding.GetEncoding(1252)))
            {
                string line = "";
                bool found = false;
                int lowest = 999999;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("max_provinces"))
                    {
                        MaxProvinces = Convert.ToInt32(line.Split('=')[1].Trim());
                    }
                    if (line.StartsWith("sea_zones"))
                    {
                        string[] spl = line.Split(new char[] { '{', '}' });
                        string[] spl2 = spl[1].Trim().Split(' ');

                        Ranges.Add(new Point(Convert.ToInt32(spl2[0]), Convert.ToInt32(spl2[1])));
                    }
                }
            }
        }

        public int MaxProvinces { get; set; }

        public int SeaStart { get; set; }

        public void FindAdjacentSameRealm(List<ProvinceParser> provinces, int c, CharacterParser head)
        {
            List<ProvinceParser> choices = new List<ProvinceParser>();
            foreach (var provinceParser in provinces)
            {
                foreach (var parser in provinceParser.Adjacent)
                {
                    if (!parser.land)
                        continue;
                    if (parser.title == null)
                        continue;

                    var t = parser.Title;
                    if (!provinces.Contains(parser) && (head == parser.Title.Holder || parser.Title.Holder == null || (parser.Title.Liege != null && head == parser.TotalLeader)))
                        choices.Add(parser);

                }
            }
            for (int i = 0; i < c; i++)
            {
                if (choices.Count > 0)
                {
                    int cc = Rand.Next(choices.Count);

                    provinces.Add(choices[cc]);
                    choices.RemoveAt(cc);
                }
            }
        }
        public List<ProvinceParser> ToFill = new List<ProvinceParser>();
        public List<ProvinceParser> Filled = new List<ProvinceParser>();
        List<Script> provinceScripts = new List<Script>();
        public void Load(BackgroundWorker worker)
        {

            colorMap = null;
            String provincesDir = Globals.GameDir + "history\\provinces\\";
            foreach (var file in Directory.GetFiles(provincesDir))
            {
                String name = file.Substring(file.LastIndexOf('\\') + 1);
                int id = Convert.ToInt32(name.Split('-')[0].Trim());


            }
            TitleManager.instance.Load();

            Provinces.Clear();
            ProvinceIDMap.Clear();
            ProvinceBitmaps.Clear();
            LoadedTerrain.Clear();

            if (Form1.instance.GenerateMap)
                ProvinceBitmap = new Bitmap(Globals.ModDir + "map\\provinces.bmp");
            else
                ProvinceBitmap = new Bitmap(Globals.MapDir + "map\\provinces.bmp");
            ProvinceBitmap = ResizeBitmap(ProvinceBitmap, (int)(ProvinceBitmap.Width * SizeMod), (int)(ProvinceBitmap.Height * SizeMod));
            ProvinceRenderBitmap = new Bitmap(ProvinceBitmap.Width, ProvinceBitmap.Height, PixelFormat.Format24bppRgb);
            LoadDefaultMap();

            ProvincePixelMap = new ProvinceParser[ProvinceBitmap.Width, ProvinceBitmap.Height];
            int progress = 0;

            for (int n = 0; n < MaxProvinces; n++)
            {
                ProvinceParser parser = new ProvinceParser(new ScriptScope());
                parser.Name = "c_unnamed" + n;
                parser.id = n + 1;
                ProvinceIDMap[parser.id] = parser;
                parser.land = true;

                foreach (var range in Ranges)
                {
                    if (n + 1 >= range.X && n + 1 <= range.Y)
                    {
                        parser.Range = range;
                        parser.land = false;
                    }
                }

            }
            LoadDefinitions();

            LockBitmap lockBitmap = new LockBitmap(ProvinceBitmap);

            int maxProgress = (ProvinceBitmap.Width * ProvinceBitmap.Height) * 2;
            maxProgress += Provinces.Count;

            lockBitmap.LockBits();

            for (int x = 0; x < lockBitmap.Width; x++)
                for (int y = 0; y < lockBitmap.Height; y++)
                {
                    Color col = lockBitmap.GetPixel(x, y);
                    if (ProvinceColorMap.ContainsKey(col))
                    {
                        ProvincePixelMap[x, y] = ProvinceColorMap[col];
                        ProvinceColorMap[col].Points.Add(new Point(x, y));

                    }

                    if (progress % 10 == 0)
                    {
                        DoProgress(worker, progress, maxProgress);
                    }
                    progress++;
                    int minX = x - 1;
                    int minY = y - 1;
                    if (minX < 0)
                        minX = 0;
                    if (minY < 0)
                        minY = 0;

                    for (int xx = minX; xx <= x; xx++)
                    {
                        for (int yy = minY; yy <= y; yy++)
                        {
                            if (xx == x && yy == y)
                                continue;

                            Color col2 = lockBitmap.GetPixel(xx, yy);
                            if (col2 != col && ProvinceColorMap.ContainsKey(col2) && ProvinceColorMap.ContainsKey(col))
                            {
                                ProvinceColorMap[col].AddAdjacent(ProvinceColorMap[col2]);
                            }
                        }
                    }




                }


            {
                foreach (var provinceParser in ProvinceIDMap)
                {
                    if (provinceParser.Value != null)
                    {

                        if (provinceParser.Value.land)
                        {
                            ToFill.Add(provinceParser.Value);
                        }
                        if (provinceParser.Value.Points.Count > 0)
                        {
                            Provinces.Add(provinceParser.Value);

                        }
                        else
                        {
                            provinceParser.Value.land = false;
                            ToFill.Remove(provinceParser.Value);
                        }
                    }
                }

            }

            while (!ProvinceIDMap.ContainsKey(Globals.StartProvinceID) || !ProvinceIDMap[Globals.StartProvinceID].land)
            {
                List<int> choices = new List<int>();
                foreach (var provinceParser in ProvinceIDMap)
                {
                    if (provinceParser.Value.land)
                        choices.Add(provinceParser.Key);
                }

                Globals.StartProvinceID = choices[Rand.Next(choices.Count)];
            }

            for (int y = lockBitmap.Height - 1; y >= 0; y--)
                for (int x = lockBitmap.Width - 1; x >= 0; x--)
                {
                    Color col = lockBitmap.GetPixel(x, y);
                    // col2 = Color.FromArgb(255, col.R, col.G, col.B);

                    int maxX = x + 1;
                    int maxY = y + 1;

                    maxX = Math.Min(lockBitmap.Width - 1, maxX);
                    maxY = Math.Min(lockBitmap.Height - 1, maxY);

                    if (progress % 10 == 0)
                    {
                        DoProgress(worker, progress, maxProgress);

                    }
                    progress++;
                    for (int xx = x; xx <= maxX; xx++)
                    {
                        for (int yy = y; yy <= maxY; yy++)
                        {
                            if (xx == x && yy == y)
                                continue;

                            Color col2 = lockBitmap.GetPixel(xx, yy);
                            if (col2 != col && ProvinceColorMap.ContainsKey(col2) && ProvinceColorMap.ContainsKey(col))
                            {
                                ProvinceColorMap[col].AddAdjacent(ProvinceColorMap[col2]);
                            }
                        }
                    }



                }
            lockBitmap.UnlockBits();

            foreach (var provinceParser in Provinces)
            {
                int maxX = -1000000;
                int maxY = -1000000;
                int minX = 1000000;
                int minY = 1000000;
                if (provinceParser.Points.Count == 0)
                {
                    provinceParser.land = false;
                    progress++;
                    continue;
                }

                if (progress % 10 == 0)
                {
                    DoProgress(worker, progress, maxProgress);

                }
                progress++;

                foreach (var point in provinceParser.Points)
                {
                    if (point.X > maxX)
                        maxX = point.X;
                    if (point.Y > maxY)
                        maxY = point.Y;
                    if (point.X < minX)
                        minX = point.X;
                    if (point.Y < minY)
                        minY = point.Y;


                }

                Bitmap bmp = new Bitmap(maxX - minX + 1, maxY - minY + 1);
                Bitmap outline = new Bitmap(maxX - minX + 1, maxY - minY + 1);
                ProvinceMapBitmap b = new ProvinceMapBitmap() { Bitmap = bmp, Outline = outline, MapPoint = new Point(minX, minY) };
                LockBitmap lb = new LockBitmap(b.Bitmap);
                LockBitmap lbo = new LockBitmap(b.Outline);
                provinceParser.Bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                lb.LockBits();
                for (int x = 0; x < lb.Width; x++)
                {
                    for (int y = 0; y < lb.Height; y++)
                    {
                        lb.SetPixel(x, y, Color.Transparent);
                    }
                }
                provinceParser.max_settlements = (provinceParser.Points.Count / 700);
                if (provinceParser.max_settlements <= 1)
                    provinceParser.max_settlements = 2;
                if (provinceParser.max_settlements > 7)
                    provinceParser.max_settlements = 7;
                foreach (var point in provinceParser.Points)
                {
                    lb.SetPixel(point.X - minX, point.Y - minY, Color.White);
                }

                lbo.LockBits();
                for (int x = 0; x < lbo.Width; x++)
                {
                    for (int y = 0; y < lbo.Height; y++)
                    {
                        lbo.SetPixel(x, y, Color.Transparent);
                        bool isEdge = false;
                        Color cc = lb.GetPixel(x, y);

                        if (cc.A != 0)
                        {
                            for (int xx = -1; xx <= 1; xx++)
                            {
                                if (isEdge)
                                    break;
                                for (int yy = -1; yy <= 1; yy++)
                                {
                                    int xxx = x + xx;
                                    int yyy = y + yy;
                                    if (xxx < 0 || yyy < 0 || xxx >= lbo.Width || yyy >= lbo.Height)
                                    {
                                        isEdge = true;
                                        break;
                                    }

                                    Color c = lb.GetPixel(xxx, yyy);

                                    if (c.A == 0)
                                    {
                                        isEdge = true;
                                    }

                                }
                            }

                            if (isEdge)
                            {
                                lbo.SetPixel(x, y, Color.White);

                            }
                        }
                     
                    }
                }
                lbo.UnlockBits();


                lb.UnlockBits();


                ProvinceBitmaps[provinceParser.id] = b;
            }


            if (!Form1.instance.GenerateMap)
                LoadAdjacencies();

            Simulation.SimulationManager.instance.Init();
        }

        private static void DoProgress(BackgroundWorker worker, int progress, int maxProgress)
        {
#if DEBUG
            return;
#endif
            int p = (int)((progress / (float)maxProgress) * 100.0f);
            if (p > 100)
                p = 100;
            worker.ReportProgress(p);
        }

        private void LoadAdjacencies()
        {
            String filename = Globals.MapDir + "map\\adjacencies.csv";
            if (!File.Exists(filename))
                return;
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(filename, Encoding.GetEncoding(1252)))
            {
                string line = "";
                int count = 0;
                while ((line = file.ReadLine()) != null)
                {
                    if (count > 0)
                    {
                        if (line.Trim().Length == 0)
                            continue;
                        String[] s = line.Split(';');
                        if (s[0].Trim().Length == 0)
                            continue;

                        int id = Convert.ToInt32(s[0]);
                        int id2 = Convert.ToInt32(s[1]);
                        if (s[2] == "major_river")
                        {
                            int id3 = Convert.ToInt32(s[3]);
                            if (ProvinceIDMap.ContainsKey(id3))
                            {
                                ProvinceIDMap[id3].river = true;
                            }
                        }

                        if (ProvinceIDMap.ContainsKey(id) && ProvinceIDMap.ContainsKey(id2))
                        {
                            ProvinceIDMap[id].AddAdjacent(ProvinceIDMap[id2]);

                        }
                    }
                    count++;
                }

                file.Close();
            }

        }


        private Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
            }
            return result;
        }
        public void SetColour(int province, Color col)
        {
            ProvinceIDMap[province].Color = col;
        }


        private void LoadDefinitions()
        {
            String filename = Globals.MapDir + "map\\definition.csv";
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(filename, Encoding.GetEncoding(1252)))
            {
                string line = "";
                int count = 0;
                while ((line = file.ReadLine()) != null)
                {
                    if (count > 0)
                    {
                        if (line.Trim().Length == 0)
                            continue;
                        String[] s = line.Split(';');
                        if (s[0].Trim().Length == 0)
                            continue;

                        int id = Convert.ToInt32(s[0]);
                        int r = Convert.ToInt32(s[1]);
                        int g = Convert.ToInt32(s[2]);
                        int b = Convert.ToInt32(s[3]);
                        String name = s[4];
                        if (ProvinceIDMap.ContainsKey(id))
                        {
                            ProvinceIDMap[id].provinceRCode = r;
                            ProvinceIDMap[id].provinceGCode = g;
                            ProvinceIDMap[id].provinceBCode = b;
                            ProvinceColorMap[Color.FromArgb(255, r, g, b)] = ProvinceIDMap[id];

                            ProvinceIDMap[id].EditorName = name.Trim();
                        }

                        
                    }
                    count++;
                }
                file.Close();
            }


        }

        public void SaveDefinitions()
        {

            if (!Form1.instance.GenerateMap)
            {
                if (Globals.MapDir != Globals.GameDir)
                {
                    // copy entire map...

                    CopyDir(Globals.MapDir + "map", Globals.ModDir + "map");
                    if (ModManager.instance.IsVanilla("gfx\\FX\\pdxmap.lua"))
                        CopyDir(Globals.MapDir + "gfx\\FX", Globals.ModDir + "gfx\\FX");
                    else
                        DelDir(Globals.MapDir + "gfx\\FX", Globals.ModDir + "gfx\\FX");

                }
                else
                {
                    DelDir(Globals.MapDir + "map", Globals.ModDir + "map");
                    DelDir(Globals.MapDir + "gfx\\FX", Globals.ModDir + "gfx\\FX");
                }

            }
            else
            {

            }

            String filename = Globals.ModDir + "map\\definition.csv";
            List<String> defs = new List<string>();
            using (System.IO.StreamReader filein =
                 new System.IO.StreamReader(Globals.MapDir + "map\\definition.csv", Encoding.GetEncoding(1252)))
            {
                string line = "";
                int count = 0;
                while ((line = filein.ReadLine()) != null)
                {
                    //  if (count > 0)
                    {

                        defs.Add(line);
                    }
                }

                filein.Close();
            }
            File.Delete(filename);
            int n = 0;

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {

                foreach (var def in defs)
                {
                    String[] split = def.Split(';');
                    if (split.Length < 5)
                    {
                        file.Write(def + Environment.NewLine);
                        continue;
                    }
                    if (split[0] == "province")
                    {
                        file.Write(def + Environment.NewLine);
                        continue;

                    }

                    int i = -1;
                    try
                    {
                        i = Convert.ToInt32(split[0]);
                    }
                    catch (Exception)
                    {

                        //   throw;
                    }
                    if (!ProvinceIDMap.ContainsKey(i))
                    {
                        String outStr = split[0] + ";" + split[1] + ";" + split[2] + ";" + split[3] + ";;" + split[5].Trim();
                        file.Write(outStr + Environment.NewLine);
                        continue;
                    }

                    if (split[4].Trim().Length > 0 && ProvinceIDMap.ContainsKey(i) && ProvinceIDMap[i].land && ProvinceIDMap[i].title != null)
                    {
                        String outStr = split[0] + ";" + split[1] + ";" + split[2] + ";" + split[3] + ";" + ProvinceIDMap[Convert.ToInt32(split[0])].title.Replace("c_", "") +
                                        ";" + split[5].Trim();
                        file.Write(outStr + Environment.NewLine);
                        continue;
                    }
                    String ooo = split[0] + ";" + split[1] + ";" + split[2] + ";" + split[3] + ";;" + split[5].Trim();
                    file.Write(ooo + Environment.NewLine);

                }

                file.Close();
            }


        }

        public void CopyDefaultMap()
        {
            if (File.Exists(Globals.ModDir + "map\\default.map"))
                File.Delete(Globals.ModDir + "map\\default.map");
            String file = Globals.MapDir + "map\\default.map";
            bool bStripped = false;
            using (System.IO.StreamReader load =
                new System.IO.StreamReader(file, Encoding.GetEncoding(1252)))
            {
                using (System.IO.StreamWriter file2 =
                    new System.IO.StreamWriter(
                        Globals.ModDir + "map\\default.map", false,
                        Encoding.GetEncoding(1252)))
                {
                    try
                    {
                        while (!load.EndOfStream)
                        {
                            string str = load.ReadLine().Trim();
                            if (str.Contains("# European Seas"))
                            {
                                break;
                            }
                            if (str.StartsWith("max_provinces"))
                            {
                                file2.Write("max_provinces = " + (MapGenManager.instance.Provinces.Count + 1) + Environment.NewLine);
                                continue;
                            }
                            file2.Write(str + Environment.NewLine);

                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (!bStripped)
                {
                    try
                    {
                        File.Delete(Globals.ModDir + "localisation\\" + file.Substring(file.LastIndexOf('\\')));
                    }
                    catch (Exception ex)
                    {

                    }
                }




            }
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
                    if (file.Contains("definition.csv") || file.Contains("geographical_region.txt"))
                        continue;
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

        public Bitmap ProvinceBitmap { get; set; }
        public Font Font { get; set; }

        public int MaxLevelAsRank
        {
            get
            {
                switch (MaxLevel)
                {
                    case PoliticalLevel.Independent:
                        return 99;
                        break;
                    case PoliticalLevel.Kingdom:
                        return 3;
                        break;
                    case PoliticalLevel.Duchie:
                        return 2;
                        break;
                }

                return 99;
            }
        }

        public bool IsVanilla
        {
            get { return Globals.MapDir == Globals.GameDir; }
        }

        public Dictionary<int, String> LoadedTerrain = new Dictionary<int, string>();

        public LockBitmap globalLockBitmap;

        ColorMatrix cm = new ColorMatrix();
        ColorMatrix cm2 = new ColorMatrix();
        ImageAttributes ia = new ImageAttributes();
        public Dictionary<String, ProvinceParser.Barony> Temples = new Dictionary<string, ProvinceParser.Barony>();
        public MapModeType MapMode = MapModeType.Political;
        public enum MapModeType
        {
            Culture,
            Religion,
            Dynasty,
            Political,
            Government
        }
        public Bitmap colorMap;
        public Color[] cache;
        public int[] cacheid;
        public List<ProvinceParser> GetProvincesInRect(int x, int y, int size)
        {
            float w = Form1.instance.renderPanel.Width;
            float h = Form1.instance.renderPanel.Height;
            float xrat = (float)ProvinceBitmap.Width / w;
            float yrat = (float)ProvinceBitmap.Height / h;
            float xx = x;// * RenderMod * xrat;
            float yy = y;// * RenderMod * yrat;

            x = (int)ConvertCanvasXToWorld(x);
            y = (int)ConvertCanvasYToWorld(y);

            var hit = Provinces.Where(p => p.land && p.Bounds.IntersectsWith(new Rectangle(new Point((int)((int)x - ((size / Zoom) / 2)), (int)((int)y - ((size / Zoom) / 2))), new Size((int)(size / Zoom), (int)(size / Zoom))))).ToList();

            hit = hit.Where(p => p.Points.Any(pp => new Rectangle(new Point((int)((int)x - ((size / Zoom) / 2)), (int)((int)y - ((size / Zoom) / 2))), new Size((int)(size / Zoom), (int)(size / Zoom))).Contains(pp))).ToList();


            return hit;
        }
        public ProvinceParser GetProvinceAt(int x, int y)
        {
            if (ProvinceBitmap == null)
                return null;

            float w = Form1.instance.renderPanel.Width;
            float h = Form1.instance.renderPanel.Height;
            float xrat = (float)ProvinceBitmap.Width / w;
            float yrat = (float)ProvinceBitmap.Height / h;
            float xx = x * RenderMod * xrat;
            float yy = y * RenderMod * yrat;

            x = (int)ConvertCanvasXToWorld(x);
            y = (int)ConvertCanvasYToWorld(y);

            var hit = Provinces.Where(p => p.Bounds.Contains(new Point((int)x, (int)y))).ToList();

            if (hit.Count > 0)
            {
                foreach (var provinceParser in hit)
                {
                    if (provinceParser.Points.Contains(new Point((int)x, (int)y)))
                        return provinceParser;
                }
            }

            return null;
        }

        public enum PoliticalLevel
        {
            Independent,
            Kingdom,
            Duchie
        }
        public float ConvertCanvasXToWorld(float x)
        {
            if (colorMap == null)
            {
                if (Form1.instance.GenerateMap)
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.ModDir + "map\\terrain.bmp");
                else
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.MapDir + "map\\terrain\\colormap.dds");



            }
            if (Centre.X < 0)
            {
                Centre.X = colorMap.Width / 2.0f;
                Centre.Y = colorMap.Height / 2.0f;
            }

            float xdif = x - (Form1.instance.renderPanel.Width / 2.0f);

            xdif /= Zoom;

            //float xrat = Form1.instance.renderPanel.Width / (float)ProvinceBitmap.Width;
            //float yrat = Form1.instance.renderPanel.Height / (float)ProvinceBitmap.Height;

            xdif += (Centre.X);

            return xdif;
        }
        public float ConvertCanvasYToWorld(float y)
        {
            if (colorMap == null)
            {
                if (Form1.instance.GenerateMap)
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.ModDir + "map\\terrain.bmp");
                else
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.MapDir + "map\\terrain\\colormap.dds");



            }
            if (Centre.X < 0)
            {
                Centre.X = colorMap.Width / 2.0f;
                Centre.Y = colorMap.Height / 2.0f;
            }


            float ydif = y - (Form1.instance.renderPanel.Height / 2.0f);


            ydif /= Zoom;
            //float xrat = Form1.instance.renderPanel.Width / (float)ProvinceBitmap.Width;
            //float yrat = Form1.instance.renderPanel.Height / (float)ProvinceBitmap.Height;

            ydif += (Centre.Y);

            return ydif;
        }

        public float ConvertWorldXToCanvas(float x)
        {
            if (colorMap == null)
            {
                if (Form1.instance.GenerateMap)
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.ModDir + "map\\terrain.bmp");
                else
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.MapDir + "map\\terrain\\colormap.dds");



            }
            if (Centre.X < 0)
            {
                Centre.X = colorMap.Width / 2.0f;
                Centre.Y = colorMap.Height / 2.0f;
            }

            float xdif = x - Centre.X;

            xdif *= Zoom;

            //float xrat = Form1.instance.renderPanel.Width / (float)ProvinceBitmap.Width;
            //float yrat = Form1.instance.renderPanel.Height / (float)ProvinceBitmap.Height;

            xdif += (Form1.instance.renderPanel.Width / 2.0f);

            return xdif;
        }

        public float ConvertWorldYToCanvas(float y)
        {
            if (colorMap == null)
            {
                if (Form1.instance.GenerateMap)
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.ModDir + "map\\terrain.bmp");
                else
                    colorMap = DevIL.DevIL.LoadBitmap(Globals.MapDir + "map\\terrain\\colormap.dds");



            }
            if (Centre.X < 0)
            {
                Centre.X = colorMap.Width / 2.0f;
                Centre.Y = colorMap.Height / 2.0f;
            }

            float ydif = y - Centre.Y;

            ydif *= Zoom;

            //float xrat = Form1.instance.renderPanel.Width / (float)ProvinceBitmap.Width;
            //float yrat = Form1.instance.renderPanel.Height / (float)ProvinceBitmap.Height;

            ydif += (Form1.instance.renderPanel.Height / 2.0f);

            return ydif;
        }

        public PoliticalLevel MaxLevel = PoliticalLevel.Independent;
        public void Draw(Graphics graphics, float w, float h, Rectangle eClipRectangle)
        {
            System.Console.Out.WriteLine("Draw: " + eClipRectangle.ToString());
            if (ProvinceBitmap == null)
                return;

            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

#if DEBUG

            Form1.doneTick = true;

#endif
            bool cacheDraw = false;
            if (Form1.resetting)
                return;
            if (!Form1.doneTick)
                cacheDraw = true;
            try
            {
                if (colorMap == null)
                {
                    if (Form1.instance.GenerateMap)
                        colorMap = DevIL.DevIL.LoadBitmap(Globals.ModDir + "map\\terrain.bmp");
                    else
                        colorMap = DevIL.DevIL.LoadBitmap(Globals.MapDir + "map\\terrain\\colormap.dds");



                }
                if (Centre.X < 0)
                {
                    Centre.X = colorMap.Width / 2.0f;
                    Centre.Y = colorMap.Height / 2.0f;
                }

                float x = eClipRectangle.X;
                float y = eClipRectangle.Y;
                float x2 = eClipRectangle.Right;
                float y2 = eClipRectangle.Bottom;

                x = ConvertCanvasXToWorld(x);
                y = ConvertCanvasYToWorld(y);
                x2 = ConvertCanvasXToWorld(x2);
                y2 = ConvertCanvasYToWorld(y2);

                var dest = new Rectangle(eClipRectangle.X, eClipRectangle.Y, eClipRectangle.Width, eClipRectangle.Height);

                Rectangle srcClip = new Rectangle((int)(x), (int)(y), 1, 1);
                int xx2 = (int)x2;
                int yy2 = (int)y2;
                srcClip.Width = xx2 - srcClip.X;
                srcClip.Height = yy2 - srcClip.Y;
                graphics.DrawImage(colorMap, dest, srcClip, GraphicsUnit.Pixel);
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(96, 0, 0, 0)), new Rectangle(eClipRectangle.X, eClipRectangle.Y, eClipRectangle.Width, eClipRectangle.Height));
                // if(w > eClipRectangle.Width)
                //      graphics.FillRectangle(new SolidBrush(Color.FromArgb(96, 128, 0, 0)), eClipRectangle);

            }
            catch (Exception)
            {


            }

            int ii = 0;
            try
            {
                int maxRank = 5;

                switch (MaxLevel)
                {
                    case PoliticalLevel.Duchie:
                        maxRank = 2;
                        break;
                    case PoliticalLevel.Kingdom:
                        maxRank = 3;
                        break;
                }

                for (int index = 0; index < Provinces.Count; index++)
                {
                    ProvinceParser provinceParser = null;
                    ii = index;
                    if (ii == 393)
                    {
                        
                    }
                    if (!cacheDraw)
                    {
                        provinceParser = Provinces[index];
                        if (provinceParser.title == null && !Form1.instance.Paint)
                            continue;
                        if (!provinceParser.land)
                            continue;

                    }

                    if (cache == null)
                    {
                        cache = new Color[Provinces.Count];
                        cacheid = new int[Provinces.Count];
                    }

                    if (SimulationManager.instance.PreYear > 0 && !cacheDraw)
                    {
                        if (!TitleManager.instance.TitleMap.ContainsKey(provinceParser.title))
                            continue;
                        if (!TitleManager.instance.TitleMap[provinceParser.title].Active)
                            continue;
                        if (TitleManager.instance.TitleMap[provinceParser.title].Holder == null &&
                            TitleManager.instance.TitleMap[provinceParser.title].CurrentHolder == null)
                            continue;
                    }

                   
                    if ((!cacheDraw && this.ProvinceBitmaps.ContainsKey(provinceParser.id)) || cache[index] != null)
                    {
                        // Get a picture box's Graphics object
                        Graphics gra = graphics;

                        Color col = Color.Black;

                        if (!cacheDraw)
                        {
                            if (provinceParser == null)
                                continue;
                        

                            if (provinceParser.ProvinceOwner == null && !Form1.instance.Paint)
                                continue;

                            CharacterParser hh = null;
                            if (provinceParser.ProvinceOwner != null)
                                hh = provinceParser.ProvinceOwner.Holder;

                            if (SimulationManager.instance.PreYear > 0)
                                if (TitleManager.instance.TitleMap[provinceParser.title].CurrentHolder != null)
                                    hh = TitleManager.instance.TitleMap[provinceParser.title].CurrentHolder;

                            if (hh != null)
                                col = hh.Color;

                            if (MapMode == MapModeType.Culture)
                            {
                                col = provinceParser.Culture.color;
                            }

                            if (MapMode == MapModeType.Religion)
                            {
                                col = provinceParser.Religion.color;
                            }
                            if (MapMode == MapModeType.Government && provinceParser.Title != null)
                            {
                                switch (provinceParser.government)
                                {
                                    case "tribal":
                                        col = Color.FromArgb(255, 92, 31, 23);
                                        break;
                                    case "feudalism":
                                        col = Color.FromArgb(255, 91, 133, 207);
                                        break;
                                    case "republic":
                                        col = Color.FromArgb(255, 250, 0, 0);
                                        break;
                                    default:
                                        {

                                        }
                                        break;
                                }

                            }

                         
                            if (MapMode == MapModeType.Political)
                            {
                                if (SimulationManager.instance.PreYear > 0)
                                    if (TitleManager.instance.TitleMap[provinceParser.title].Holder.TopLiegeCharacter != null)
                                        col =
                                            TitleManager.instance.TitleMap[provinceParser.title].Holder.GetTopLiegeCharacter(maxRank)
                                                .PrimaryTitle.color;
                            }
                            cacheid[index] = provinceParser.id;
                        }
                        else
                        {
                            col = cache[index];
                        }


                        cache[index] = col;
                        //    col = Color.FromArgb(255, rand.Next(255), rand.Next(255), rand.Next(255));
                        // Create a new color matrix and set the alpha value to 0.5

                        if (!cacheDraw)
                        {
                            TitleParser tit = null;

                            if (SimulationManager.instance.PreYear > 0)
                                tit = TitleManager.instance.TitleMap[provinceParser.title];
                            //if (tit.Liege == null)

                            if (SimulationManager.instance.PreYear > 0)
                                while (tit.Liege != null && tit.Liege.Rank > tit.Rank)
                                    tit = tit.Liege;


                        }
                        if (!ProvinceBitmaps.ContainsKey(cacheid[index]))
                            continue;
                        var p = this.ProvinceBitmaps[cacheid[index]].MapPoint;
                        Bitmap bmp = this.ProvinceBitmaps[cacheid[index]].Bitmap;
                     
                        float x = ConvertWorldXToCanvas(p.X);
                        float y = ConvertWorldYToCanvas(p.Y);
                        float x2 = ConvertWorldXToCanvas(p.X + bmp.Width+1);
                        float y2 = ConvertWorldYToCanvas(p.Y + bmp.Height+1);
                        float ww = x2 - x;
                        float hhh = y2 - y;

                        Rectangle dRect = new Rectangle((int)x, (int)y, (int)ww, (int)hhh);
                        if (eClipRectangle.IntersectsWith(dRect))

                        {
                            Bitmap bmpo = this.ProvinceBitmaps[cacheid[index]].Outline;
                            cm.Matrix00 = (col.R / 255.0f); // * 0.7f;
                            cm.Matrix11 = (col.G / 255.0f); // * 0.7f;
                            cm.Matrix22 = (col.B / 255.0f); // * 0.7f;


                            cm.Matrix33 = 1.0f;
                            cm.Matrix44 = 1.0f;
                            cm.Matrix33 = 0.4f;

                            cm2.Matrix00 = 0; // * 0.7f;
                            cm2.Matrix11 = 0; // * 0.7f;
                            cm2.Matrix22 = 0; // * 0.7f;


                            cm2.Matrix33 = 1.0f;
                            cm2.Matrix44 = 1.0f;
                            cm2.Matrix33 = 0.3f;

                            if (SimulationManager.instance.PreYear == 0 && !Form1.instance.Paint)
                            {
                                cm.Matrix00 = (1.0f); // * 0.7f;
                                cm.Matrix11 = (1.0f); // * 0.7f;
                                cm.Matrix22 = (0.0f); // * 0.7f;
                                cm.Matrix33 = 1.0f;
                            }


                            if (SelectedProvinces.Contains(provinceParser))
                            {
                                cm.Matrix00 = (1.0f); // * 0.7f;
                                cm.Matrix11 = (0.0f); // * 0.7f;
                                cm.Matrix22 = (1.0f); // * 0.7f;
                                cm.Matrix33 = 0.7f;
                            }


                            ia.SetColorMatrix(cm);
                            graphics.DrawImage(bmp,
                                dRect, 0, 0,
                                bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
                            ia.SetColorMatrix(cm2);

                            if(Zoom > 0.45f)
                            graphics.DrawImage(bmpo,
                                                        dRect, 0, 0,
                                                        bmpo.Width, bmpo.Height, GraphicsUnit.Pixel, ia);

                        }

                    }
                    else
                    {

                    }
                }

              
                SolidBrush b = new SolidBrush(Color.White);
                SolidBrush b2 = new SolidBrush(Color.Black);


                if (MapMode == MapModeType.Political)
                {
                    for (var index = 0; index < TitleManager.instance.Titles.Count; index++)
                    {
                        var titleParser = TitleManager.instance.Titles[index];
                        if (titleParser.Name == "c_oriel")
                        {
                        }
                        if (titleParser.Liege == null || titleParser.Rank == maxRank)
                        {
                            if (titleParser.Rank == 1)
                            {
                            }
                            if (titleParser.Holder != null && titleParser.Holder.PrimaryTitle == titleParser &&
                                titleParser.Rank <= maxRank)
                            {
                                //                            if (titleParser.Bounds.Width > 48 && titleParser.Bounds.Height > 48)
                                {
                                    int cx = titleParser.TextPos.X;
                                    int cy = titleParser.TextPos.Y;
                                    cy -= 12;

                                    cx = (int) ConvertWorldXToCanvas(cx);
                                    cy = (int) ConvertWorldYToCanvas(cy);
                                    Font font = Form1.DefaultFont;
                                    Font font2 = Form1.DefaultFont2;

                                    Font font3 = Form1.DefaultFont3;
                                    Font font4 = Form1.DefaultFont4;
                                    Font font5 = Form1.DefaultFont5;
                                    if (titleParser.Rank >= 4)
                                        font = font5;
                                    else if (titleParser.Rank >= 3)
                                        font = font4;
                                    else if (titleParser.Rank == 2)
                                    {
                                        font = font3;
                                        if (Zoom < 0.8f)
                                            continue;
                                    }
                                    else if (titleParser.Rank == 1)
                                    {
                                        if (Zoom < 1.2f)
                                            continue;

                                        font = font2;
                                    }
                                    Rectangle dRect =
                                        new Rectangle(
                                            new Point(
                                                (int)
                                                ((cx -
                                                  (graphics.MeasureString(titleParser.LangRealmName, font).Width / 2)) +
                                                 1), cy + 1),
                                            new Size(
                                                (int) graphics.MeasureString(titleParser.LangRealmName, font).Width,
                                                (int) graphics.MeasureString(titleParser.LangRealmName, font).Height));
                                    dRect.Inflate(6, 6);
                                    if (eClipRectangle.IntersectsWith(dRect))

                                    {
                                        graphics.DrawString(titleParser.LangRealmName, font, b2,
                                            new PointF(
                                                (cx -
                                                 (graphics.MeasureString(titleParser.LangRealmName, font).Width / 2)) +
                                                1, cy + 1));
                                        graphics.DrawString(titleParser.LangRealmName, font, b,
                                            new PointF(
                                                cx - (graphics.MeasureString(titleParser.LangRealmName, font).Width / 2),
                                                cy));
                                    }
                                }
                            }
                        }
                    }
                }
                if (MapMode == MapModeType.Religion)
                {
                    foreach (var religion in ReligionManager.instance.AllReligions)
                    {
                        Font font = Form1.DefaultFont;
                      
                        //                            if (titleParser.Bounds.Width > 48 && titleParser.Bounds.Height > 48)
                        {
                            int cx = religion.TextPos.X;
                            int cy = religion.TextPos.Y;
                            cy -= 12;
                            cx = (int)ConvertWorldXToCanvas(cx);
                            cy = (int)ConvertWorldYToCanvas(cy);
                            Rectangle dRect = new Rectangle(new Point((int)((cx - (graphics.MeasureString(religion.LanguageName, font).Width / 2)) + 1), cy + 1), new Size((int)graphics.MeasureString(religion.LanguageName, font).Width, (int)graphics.MeasureString(religion.LanguageName, font).Height));
                            dRect.Inflate(6, 6);
                            if (eClipRectangle.IntersectsWith(dRect))

                            {

                                graphics.DrawString(religion.LanguageName, font, b2, new PointF((cx - (graphics.MeasureString(religion.LanguageName, font).Width / 2)) + 1, cy + 1));
                                graphics.DrawString(religion.LanguageName, font, b, new PointF(cx - (graphics.MeasureString(religion.LanguageName, font).Width / 2), cy));
                            }

                        }
                    }
                }

                if (MapMode == MapModeType.Culture)
                {
                    foreach (var religion in CultureManager.instance.AllCultures)
                    {

                        //                            if (titleParser.Bounds.Width > 48 && titleParser.Bounds.Height > 48)
                        {
                            Font font = Form1.DefaultFont;
                            int cx = religion.TextPos.X;
                            int cy = religion.TextPos.Y;
                            cy -= 12;
                            cx = (int)ConvertWorldXToCanvas(cx);
                            cy = (int)ConvertWorldYToCanvas(cy);
                            Rectangle dRect = new Rectangle(new Point((int)((cx - (graphics.MeasureString(religion.LanguageName, font).Width / 2)) + 1), cy + 1), new Size((int)graphics.MeasureString(religion.LanguageName, font).Width, (int)graphics.MeasureString(religion.LanguageName, font).Height));
                            dRect.Inflate(6, 6);
                            if (eClipRectangle.IntersectsWith(dRect))

                            {

                                graphics.DrawString(religion.LanguageName, font, b2, new PointF((cx - (graphics.MeasureString(religion.LanguageName, font).Width / 2)) + 1, cy + 1));
                                graphics.DrawString(religion.LanguageName, font, b, new PointF(cx - (graphics.MeasureString(religion.LanguageName, font).Width / 2), cy));
                            }

                        }
                    }
                }


            }
            catch (Exception ex)
            {

                Console.Out.WriteLine(ex.ToString());
            }

            if (SimulationManager.instance.Active && !cacheDraw)
                Form1.doneTick = false;
            if (!SimulationManager.instance.Active)
                Form1.doneTick = true;

            //     graphics.DrawImage(this.ProvinceBitmap, new Rectangle(0, 0, ProvinceBitmap.Width, ProvinceBitmap.Height));
        }

        public void UpdateOwnership()
        {
        }

        public void RegisterBarony(string name, TitleParser subTitle)
        {
            if (this.Temples.ContainsKey(name))
            {
                Temples[name].titleParser = subTitle;
            }
        }

        public ProvinceParser GetInhabited()
        {
            var province = Provinces[Rand.Next(Provinces.Count)];

            while (province.title == null)
            {
                province = Provinces[Rand.Next(Provinces.Count)];
            }

            return province;
        }

        public List<ProvinceParser> GetProvinceBlock(int count)
        {
            List<ProvinceParser> provs = new List<ProvinceParser>();

            List<ProvinceParser> choices = new List<ProvinceParser>();
            foreach (var provinceParser in Provinces)
            {
                if (provinceParser.land && provinceParser.title == null)
                {
                    choices.Add(provinceParser);
                }
            }

            if (choices.Count > 0)
            {
                var ch = choices[Rand.Next(choices.Count)];

                var before = provs.Count;
                provs.Add(ch);
                
                provs.ToList().ForEach(delegate(ProvinceParser parser)
                {
                    foreach (var provinceParser in parser.Adjacent)
                    {
                        if (provinceParser.land)
                        {
                            if (provinceParser.title == null && !provs.Contains(provinceParser))
                            {
                                provs.Add(provinceParser);
                            }
                        }
                        else
                        {
                            foreach (var provinceParser2 in parser.Adjacent)
                            {
                                if (provinceParser2.land)
                                {
                                    if (provinceParser2.title == null && !provs.Contains(provinceParser2))
                                    {
                                        provs.Add(provinceParser2);
                                    }
                                }
                               
                            }
                        }
                    }
                });

                if (before == provs.Count)
                {
                    
                }
            }

            return provs;
        }
    }
}
