using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using BackgroundWorkerDemo;
using CrusaderKingsStoryGen.MapGen;
using CrusaderKingsStoryGen.PropertyPageProxies;
using CrusaderKingsStoryGen.Simulation;
using Microsoft.Win32;

namespace CrusaderKingsStoryGen
{


    public partial class Form1 : Form
    {
        private SolidBrush brush;
        public static bool loaded = false;
        public static bool autoload = false;
        Random rand = new Random();
        BackgroundWorker resetWorker = new BackgroundWorker();
        BackgroundWorker exportWorker = new BackgroundWorker();
        BackgroundWorker tickWorker = new BackgroundWorker();
        public int Speed = 10;
        public bool GenerateMap = false;
        public bool Paint { get; set; } = false;

        
        public Form1()
        {
            //    Rand.SetSeed();
            DefaultFont2 = new Font(DefaultFont, FontStyle.Italic);
            DefaultFont3 = new Font(DefaultFont.FontFamily, 9.5f, FontStyle.Regular);
            DefaultFont4 = new Font(DefaultFont.FontFamily, 10.5f, FontStyle.Bold);
            DefaultFont5 = new Font(DefaultFont.FontFamily, 12.5f, FontStyle.Bold);
            String filename = ".\\settings.txt";
        
            Closing  += (sender, args) => EventLogger.instance.Save();

            //MapGen.MapGenManager.instance.Create();

            brush = new SolidBrush(Color.White);
          //  Rand.SetSeed();
            InitializeComponent();

        
            selectMode.Checked = true;
            viewIndependent.Checked = true;
            resetWorker.DoWork += delegate { LoadFiles(resetWorker); };
            resetWorker.RunWorkerCompleted += ResetWorkerRunResetWorkerResetCompleted;
            resetWorker.ProgressChanged += ResetWorkerOnProgressChanged;
            exportWorker.DoWork += delegate { Export(); };
            exportWorker.RunWorkerCompleted += WorkerRunWorkerExportCompleted;
            exportWorker.ProgressChanged += ResetWorkerOnProgressChanged;
            tickWorker.RunWorkerCompleted += TickWorkerOnRunWorkerCompleted;
            tickWorker.DoWork += delegate
            {  
                //if (!exporting)
                {
                    
                    SimulationManager.instance.TickSystem();
                    if (SimulationManager.instance.Year > SimulationManager.StartYear)
                    {
                        for(int x=0;x< Speed-1;x++)
                            SimulationManager.instance.TickSystem();

                    }
       
                }
            };

            renderPanel.MouseWheel += RenderPanel_MouseWheel;

            string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            mydocs += "\\Paradox Interactive\\Crusader Kings II\\mod\\";

            Globals.ModRoot = mydocs;

            Globals.ModDir = Globals.ModRoot + modname.Text + "\\";
#if DEBUG
            timer1.Interval = 100;
#endif
            if (File.Exists(filename))
            {
                try
                {
                    using (System.IO.StreamReader file =
                                   new System.IO.StreamReader(filename, Encoding.GetEncoding(1252)))
                    {
                        string line = file.ReadLine();

                        if (!line.Contains("="))
                        {
                            Globals.MapDir = line;

                            Globals.GameDir = file.ReadLine();
                        }
                        else
                        {
                            Globals.LoadSettings(file, line);
                        }

                    }
                 
                }
                catch (Exception ex)
                {
                    Globals.GameDir = Globals.MapDir;

                }
                if(Globals.GameDir == null || Globals.GameDir.Length==0)
                    Globals.GameDir = Globals.MapDir;

                if (Directory.Exists(Globals.GameDir) && File.Exists(Globals.GameDir + "ck2game.exe"))
                {
                    ck2dir.Text = Globals.GameDir;
                }
                if (Directory.Exists(Globals.MapDir + "map"))
                {
                    mapDir.Text = Globals.MapDir;
                }
                else
                {
                    autoload = false;
                    Globals.GameDir = "";
                    Globals.MapDir = "";
                    exportButton.Enabled = false;
                    start.Enabled = false;
                    stop.Enabled = false;

                }
                
            }
            else
            {
                string userRoot = "HKEY_LOCAL_MACHINE";
                string subkey = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 203770";
                string keyName = userRoot + "\\" + subkey;
                 string noSuch = (string)Registry.GetValue(keyName,
                 "InstallLocation",
                 null);
       
                if (noSuch != null)
                {
                    ck2dir.Text = noSuch;

                    Globals.GameDir = ck2dir.Text;
                    if (!Globals.GameDir.EndsWith("\\"))
                        Globals.GameDir += "\\";


                }
                else
                {
                    ck2dir.Text = "";

                }
              
                if (Globals.GameDir == null || !Directory.Exists(Globals.GameDir))
                {
                    // doesn't exist, so don't load everything yet...
                    autoload = false;
                    exportButton.Enabled = false;
                    start.Enabled = false;
                    stop.Enabled = false;
 
                }
            }
         
            if (autoload && !GenerateMap)
            {
                mainTabs.SelectTab("generateTab");
                Globals.SaveSettings();
            }

            start.Enabled = autoload;
            stop.Enabled = false;
            resetButton.Enabled = false;
            exportButton.Enabled = false;

            if (autoload)
            {
                reset_Click(null, new EventArgs());
            }
            instance = this;
            numericUpDown1.Value = rand.Next(10000000);
            toolStripButton1.CheckState = CheckState.Checked;
     
            String[] traits = ModManager.instance.GetFiles("common\\traits");

         //   propertyGrid.SelectedObject = new Test();
            foreach (var trait in traits)
            {
                
            }


        }

        public static Font DefaultFont4 { get; set; }
        public static Font DefaultFont5 { get; set; }

        private void RenderPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (MapManager.instance.ProvinceBitmap == null)
                return;

            if (e.Delta > 0)
            {
                MapManager.instance.Zoom *= 1.2f;
            }
            else
                MapManager.instance.Zoom /= 1.3f;

            MapManager.instance.Zoom  =Math.Max(MapManager.instance.Zoom, 0.2f);

            MapManager.instance.Zoom = Math.Min(MapManager.instance.Zoom, 7f);

            renderPanel.Invalidate();
        }

        private void TickWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            doneTick = true;
            exporting = false;
            renderPanel.Invalidate();
        }


        private void LoadFiles(BackgroundWorker worker)
        {
            if (loaded)
                return;
#if DEBUG
            Rand.SetSeed(567);
#else
            if (Rand.Seed != (int)numericUpDown1.Value)
                Rand.SetSeed((int)numericUpDown1.Value);

#endif
            //if (Rand.Seed != (int)numericUpDown1.Value)
              //  Rand.SetSeed((int)numericUpDown1.Value);

            MapManager.instance.Load(worker);
            //  TitleManager.instance.Load();

           TechnologyManager.instance.Init();
            SocietyManager.instance.Load();
            ScripterTriggerManager.instance.Load();
            OnActionsManager.instance.Load();
            //  SimulationManager.instance.RunSimulationFromHistoryFiles((int)simulationDate.Value);
           // SimulationManager.instance.RunSimulationFromHistoryFiles((int)simulationDate.Value);
            start.Enabled = true;

            loaded = true;
            
            // start_Click(null, new EventArgs());

        }

        private void renderPanel_Paint(object sender, PaintEventArgs e)
        {
            if (MapManager.instance.ProvinceBitmap == null)
                return;

            if (toAdd.Count > 0)
            {
                String str = "";
                if (toAdd.TryDequeue(out str))
                {
                    logView.Items.Add(str);
                }
            }
            e.Graphics.Clear(Color.Black);
            MapManager.instance.Font = DefaultFont;
           if(Globals.GameDir!="")
            MapManager.instance.Draw(e.Graphics, renderPanel.Width, renderPanel.Height, e.ClipRectangle);
            if(SimulationManager.instance.Year > SimulationManager.StartYear)
                e.Graphics.DrawString(SimulationManager.instance.Year.ToString(), DefaultFont, brush, new PointF(10, 10));
            else
                e.Graphics.DrawString("Pre-history", DefaultFont, brush, new PointF(10, 10));
        }

        private void renderPanel_Resize(object sender, EventArgs e)
        {
            if (MapManager.instance.ProvinceBitmap == null)
                return;

            renderPanel.Invalidate();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
#if DEBUG
            if (SimulationManager.instance.Active)
            {
                SimulationManager.instance.TickSystem();
                if (SimulationManager.instance.Year > SimulationManager.StartYear)
                {
                    //for (int x = 0; x < 9; x++)
                    //    SimulationManager.instance.TickSystem();

                }
                renderPanel.Invalidate();
            }


            return;
#endif
            if (!tickWorker.IsBusy && requestReset)
            {
                doneTick = true;
                reset_Click(sender, e);
                requestReset = false;
                return;
            }
            if (!tickWorker.IsBusy && !doneTick && SimulationManager.instance.Active)
            {
                
                tickWorker.RunWorkerAsync();
       

            }
            if (SimulationManager.instance.Active && SimulationManager.instance.Year < SimulationManager.instance.MaxYear + 4 && SimulationManager.instance.bDonePreStage1)
                exportButton.Enabled = true;


        }

        protected override void OnClosed(EventArgs e)
        {
            Globals.SaveSettings();

            base.OnClosed(e);
        }

        public static Form1 instance;
        public static bool exporting;
        public static bool doneTick = true;

        private void renderPanel_Click(object sender, EventArgs e)
        {
            
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


        public void Export()
        {

            if (!Directory.Exists(Globals.ModDir))
                Directory.CreateDirectory(Globals.ModDir);
            String fn = Globals.ModDir + "log.txt";
            using (System.IO.StreamWriter log =
             new System.IO.StreamWriter(fn, false, Encoding.GetEncoding(1252)))
            {
#if DEBUG

#else
                try
#endif
                {
                    LanguageManager.instance.Add("norse", StarNames.Generate(Rand.Next(1000000)));
                    LanguageManager.instance.Add("pagan", StarNames.Generate(Rand.Next(1000000)));

                    LanguageManager.instance.Add("pagan_group", StarNames.Generate(Rand.Next(1000000)));
                    log.WriteLine("Exporting at " + SimulationManager.instance.Year);
                    log.WriteLine("Characters #: " + CharacterManager.instance.Characters.Count);
                    log.WriteLine("Provinces #: " + MapManager.instance.Provinces.Count);
                    int progress = 0;
                    int maxProgress = 100;
                    MapManager.instance.UpdateOwnership();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    TitleManager.instance.FixupFinal();


#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Creating Mercs");
                    TitleManager.instance.CreateMercs();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    if (!Globals.ModDir.Contains("\\mod"))
                    {
                        if (Globals.ModDir.EndsWith("\\"))
                            Globals.ModDir += "mod\\" + Globals.ModName + "\\";
                        else
                        {
                            Globals.ModDir += "\\mod\\" + Globals.ModName + "\\";
                        }

                    }


                    if (CultureManager.instance.CultureMap.Count > 0)
                        foreach (var titleParser in TitleManager.instance.Titles)
                        {


                            if (titleParser.Holder != null && CultureManager.instance.CultureMap.ContainsKey(titleParser.Holder.culture) &&
                                CultureManager.instance.CultureMap[titleParser.Holder.culture].dna != null)
                            {
                                if (titleParser.Rank == 4)
                                    LanguageManager.instance.Add(titleParser.Name,
                                        LanguageManager.instance.Get(titleParser.Name.Replace(" Empire", "")));
                              //  else if (LanguageManager.instance.Get(titleParser.Name) == null)
                                //    titleParser.RenameForCulture(titleParser.Holder.Culture);

                            }

                        }


#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Calcing Religious Equivelents");
                    ReligionManager.instance.DoReligiousEquivelents();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress += 21;
#endregion

                  
                    DelDir(Globals.ModDir + "\\common\\governments\\", Globals.ModDir + "\\common\\governments\\");
                    CopyDir(Globals.MapDir + "\\common\\disease\\", Globals.ModDir + "\\common\\disease\\");
                    CopyDir(Globals.MapDir + "\\common\\trade_routes\\", Globals.ModDir + "\\common\\trade_routes\\");
                     log.WriteLine("Saving Religions");
                    ReligionManager.instance.Save();
                    SocietyManager.instance.Save();
                    TraitManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Characters");
                    CharacterManager.instance.Save();


#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress += 20;
#endregion


                    log.WriteLine("Saving Titles");
                    TitleManager.instance.SaveTitles();


#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress += 20;
#endregion


                    log.WriteLine("Saving Landed Titles");
                    TitleManager.instance.LandedTitlesScript.Save();

                    log.WriteLine("Saving Map");
                    MapManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Governments");
                    GovernmentManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Cultures");
                    CultureManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Events");
                    EventManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Definitions");
                    MapManager.instance.SaveDefinitions();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Regions");
                    RegionManager.instance.Save();
                    ScripterTriggerManager.instance.Save();
                    OnActionsManager.instance.Save();
#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Dynasties");
                    DynastyManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    //            ModularFunctionalityManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Sprite Definitions");
                    SpriteManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Bookmark");
                    BookmarkManager.instance.Save();
                    log.WriteLine("Saving Technology");
                    TechnologyManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Flags");
                    FlagManager.instance.AssignAndSave();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress += 20;
#endregion

                    if (!Directory.Exists(Globals.ModDir + "history\\provinces\\"))
                        Directory.CreateDirectory(Globals.ModDir + "history\\provinces\\");
                    var files = Directory.GetFiles(Globals.ModDir + "history\\provinces\\");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    log.WriteLine("Saving Provinces");
                    foreach (var provinceParser in MapManager.instance.Provinces)
                    {
                        if (provinceParser.land && provinceParser.title != null)
                        {
                            provinceParser.Save();
                        }
                    }


#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion


                    log.WriteLine("Saving Decisions");
                    DecisionManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    log.WriteLine("Saving Language");
                    LanguageManager.instance.DoSubstitutes();

                    LanguageManager.instance.Save();

#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    int y = 200;

                    if(File.Exists(Globals.ModDir + "interface\\coat_of_arms.txt"))
                        File.Delete(Globals.ModDir + "interface\\coat_of_arms.txt");
                    File.Copy(Globals.GameDir + "interface\\coats_of_arms.txt", Globals.ModDir + "interface\\coats_of_arms.txt");
                    ArbitaryFileEditor.instance.CopyAndSubstitute("common\\defines.lua", new Dictionary<string, string>()
                    {
                        ["DONT_EXECUTE_TECH_BEFORE"] = "DONT_EXECUTE_TECH_BEFORE = " + (y) + ","

                    }
                    );


#region Progress
                    { DoProgress(exportWorker, progress, maxProgress); }
                    progress++;
#endregion

                    string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    mydocs += "\\Paradox Interactive\\Crusader Kings II\\" + modname.Text + "\\";
                    log.WriteLine("Deleting Cache");
                    DelDir(mydocs, mydocs);

                }
#if DEBUG

#else
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace, ex.Message);
                    log.WriteLine(ex.StackTrace);
                }
#endif              
            }
            String filename = Globals.ModRoot + modname.Text + ".mod";
            using (System.IO.StreamWriter file =
             new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {

                String dep = ModManager.instance.GetDependencies();



               file.Write(@"name="""+ modname.Text +@"""
path=""mod/" + modname.Text + @"""
" + dep + @"
user_dir = """ + modname.Text + @"""
replace_path=""history/titles""
replace_path=""history/characters""
replace_path=""history/wars""
replace_path=""history/provinces""
replace_path=""history/technology""
replace_path=""events""
replace_path =""gfx/flags""
replace_path=""common/landed_titles""
replace_path=""common/bookmarks""
replace_path=""common/dynasties""
replace_path=""common/religious_titles""
");

                // replace_path = ""common / cultures""
                // replace_path = ""common / religions""

                file.Close();
            }
            filename = Globals.ModDir + "seed.txt";
            using (System.IO.StreamWriter file =
             new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {

                file.Write("# Seed = " + Rand.Seed);

                file.Close();
            }


            filename = Globals.ModDir + "common\\province_setup\\00_province_setup.txt";

            if (File.Exists(filename))
                File.Delete(filename);
            using (System.IO.StreamWriter file =
             new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {


                file.Close();
            }

            filename = Globals.ModDir + "common\\trade_routes\\00_silk_route.txt";

            if (File.Exists(filename))
                File.Delete(filename);
            using (System.IO.StreamWriter file =
             new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {


                file.Close();
            }
        
            //     this.Close();

            SimulationManager.instance.Active = false;
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
  
      
        private void stop_Click(object sender, EventArgs e)
        {
            start.Enabled = true;
            stop.Enabled = false;
            resetButton.Enabled = true;
            exportButton.Enabled = true;
            SimulationManager.instance.Active = false;
            if (SimulationManager.instance.Year <= SimulationManager.StartYear)
                return;

        //    while (tickWorker.IsBusy)
            {
                Thread.Sleep(1000);
            }

            RefreshTree();

            renderPanel.Invalidate();
        }

        internal void RefreshTree(TitleParser title = null)
        {
            switch (MapManager.instance.MapMode)
            {
                case MapManager.MapModeType.Political:
                    FillTreePolitical();
                    break;
                case MapManager.MapModeType.Religion:
                    FillTreeReligion();
                    break;
                case MapManager.MapModeType.Culture:
                    FillTreeCulture();
                    break;
            }
            renderPanel.Invalidate();
        }

        private void FillTreeCulture()
        {
            inspectTree.Nodes.Clear();
            foreach (var group in CultureManager.instance.AllCultureGroups)
            {

                {
                    group.AddTreeNode(inspectTree);
                }

            }
        }
        private void FillTreeReligion()
        {
            inspectTree.Nodes.Clear();
            foreach (var group in ReligionManager.instance.AllReligionGroups)
            {

                {
                    group.AddTreeNode(inspectTree);
                }

            }
        }
        private void FillTreePolitical()
        {
            inspectTree.Nodes.Clear();
            foreach (var titleParser in TitleManager.instance.Titles)
            {
                if (titleParser.Liege == null)
                {
                    if (titleParser.capital == 0)
                        continue;

                    var root = titleParser.AddTreeNode(inspectTree);
                }

            }
        }

        private void start_Click(object sender, EventArgs e)
        {
            start.Enabled = false;
            stop.Enabled = true;
            doneTick = false;
            resetButton.Enabled = true;


#if !DEBUG
            
              if (!SimulationManager.instance.bTicked)            
#endif
            SimulationManager.instance.Active = true;
            renderPanel.Invalidate();
        }

        private bool requestReset = false;
        public static bool resetting;

        public void reset_Click(object sender, EventArgs e)
        {

            SimulationManager.instance.Active = false;


#if DEBUG

            ArbitaryFileEditor.instance = new ArbitaryFileEditor();
            ScripterTriggerManager.instance = new ScripterTriggerManager();
            OnActionsManager.instance = new OnActionsManager();

            ModManager.instance = new ModManager();
            ModManager.instance.Init();
            ModManager.instance.LoadMods();
            SocietyManager.instance = new SocietyManager();
            BookmarkManager.instance = new BookmarkManager();
            DynastyManager.instance = new DynastyManager();
            EventManager.instance = new EventManager();
            GovernmentManager.instance = new GovernmentManager();
            TechnologyManager.instance = new TechnologyManager();
            SimulationManager.instance.Active = false;
            Thread.Sleep(200);
            SimulationManager.instance = new SimulationManager();
            FlagManager.instance = new FlagManager();
            CulturalDnaManager.instance = new CulturalDnaManager();
            CultureManager.instance = new CultureManager();
            ReligionManager.instance = new ReligionManager();
            CharacterManager.instance = new CharacterManager();
            LanguageManager.instance = new LanguageManager();
            MapManager.instance = new MapManager();
            TitleManager.instance = new TitleManager();
            EventManager.instance = new EventManager();
            DecisionManager.instance = new DecisionManager();
            SpriteManager.instance = new SpriteManager();
            TraitManager.instance = new TraitManager();

            LoadFiles(resetWorker);

            return;
#endif
            while (!doneTick)
            {
                requestReset = true;
                return;
            }
            resetting = true;
            loaded = false;
     
            ArbitaryFileEditor.instance = new ArbitaryFileEditor();
            ScripterTriggerManager.instance = new ScripterTriggerManager();
            OnActionsManager.instance = new OnActionsManager();

            ModManager.instance = new ModManager();
            ModManager.instance.Init();
            ModManager.instance.LoadMods();
            SocietyManager.instance = new SocietyManager();
            BookmarkManager.instance = new BookmarkManager();
            DynastyManager.instance = new DynastyManager();
            EventManager.instance = new EventManager();
            GovernmentManager.instance = new GovernmentManager();
            TechnologyManager.instance = new TechnologyManager();
            SimulationManager.instance.Active = false;
            Thread.Sleep(200);
            SimulationManager.instance = new SimulationManager();
            FlagManager.instance = new FlagManager();
            CulturalDnaManager.instance = new CulturalDnaManager();
            CultureManager.instance = new CultureManager();
            ReligionManager.instance = new ReligionManager();
            CharacterManager.instance = new CharacterManager();
            LanguageManager.instance = new LanguageManager();
            MapManager.instance = new MapManager();
            TitleManager.instance = new TitleManager();
            EventManager.instance = new EventManager();
            DecisionManager.instance = new DecisionManager();
            SpriteManager.instance = new SpriteManager();
            TraitManager.instance = new TraitManager();
            start.Enabled = false;
            stop.Enabled = false;
            resetButton.Enabled = false;
            exportButton.Enabled = false;
            button2.Enabled = true;

            alert = new AlertForm();
            // event handler for the Cancel button in AlertForm
            alert.buttonCancel.Visible = false;
            alert.Text = "Resetting simulation state...";
           // alert.Parent = this;
            alert.Show(this);
            resetWorker.WorkerReportsProgress = true;
            resetWorker.RunWorkerAsync();
  
         }
        public void exportButton_Click(object sender, EventArgs e)
        {
            SimulationManager.instance.Active = false;
            Thread.Sleep(120);
            while (exporting && !doneTick)
            {
                Thread.Sleep(30);
            }
            exporting = true;
            start.Enabled = false;
            stop.Enabled = false;
            resetButton.Enabled = false;
            exportButton.Enabled = false;

#if DEBUG
            Export();
            return;
#endif

            alert = new AlertForm();
            // event handler for the Cancel button in AlertForm
            alert.buttonCancel.Visible = false;
            alert.Text = "Exporting mod data from generated history...";
            // alert.Parent = this;
            alert.Show(this);
            exportWorker.WorkerReportsProgress = true;
            exportWorker.RunWorkerAsync();
            
        }

        private void ResetWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
 
            alert.Message = "In progress, please wait... " + e.ProgressPercentage.ToString() + "%";
            alert.ProgressValue = e.ProgressPercentage;
        }

        public static AlertForm alert { get; set; }
        public static Font DefaultFont2 { get; set; }
        public static Font DefaultFont3 { get; set; }

        private void WorkerRunWorkerExportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            start.Enabled = false;
            stop.Enabled = false;
            exportButton.Enabled = false;
            resetButton.Enabled = true;
            alert.Close();
            alert = null;
            renderPanel.Invalidate();
            exporting = false;
         }

        private void ResetWorkerRunResetWorkerResetCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            start.Enabled = true;
            resetButton.Enabled = false;     
            alert.Close();
            alert = null;
            renderPanel.Invalidate();
            exporting = false;
            resetting = false;
        }

    
        private void label3_Click(object sender, EventArgs e)
        {

        }

        public void button1_Click(object sender, EventArgs e)
        {
            Globals.MapDir = Globals.GameDir;
            mapDir.Text = Globals.GameDir;
            exportButton.Enabled = true;
            start.Enabled = true;
            stop.Enabled = true;
     
            Globals.SaveSettings();

            reset_Click(sender, e);
        }
        private void selectCK2MapModDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();

            if (d.ShowDialog() == DialogResult.OK)
            {
                Globals.MapDir = d.SelectedPath;
                if (Globals.MapDir.Length > 0 && !Globals.MapDir.EndsWith("\\"))
                    Globals.MapDir += "\\";

                if (Directory.Exists(Globals.MapDir + "map"))
                {
           
                    exportButton.Enabled = true;
                    start.Enabled = true;
                    stop.Enabled = true;

                    Globals.SaveSettings();

                }
                else
                {
                    MessageBox.Show(
                        "Error: Could not find map mod in specified directory.",
                        "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    exportButton.Enabled = false;
                    start.Enabled = false;
                    stop.Enabled = false;
    
                }

                this.mapDir.Text = Globals.MapDir;
            }
        }

        private void selectCK2Dir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();

            if (d.ShowDialog() == DialogResult.OK)
            {
                Globals.GameDir = d.SelectedPath;
                if (Globals.GameDir.Length > 0 && !Globals.GameDir.EndsWith("\\"))
                    Globals.GameDir += "\\";

                Globals.MapDir = Globals.GameDir;
                if (Directory.Exists(Globals.MapDir) && File.Exists(Globals.GameDir + "ck2game.exe"))
                {
                    resetButton.Enabled = false;
                    exportButton.Enabled = false;
                    start.Enabled = true;
                    stop.Enabled = true;
             
                    Globals.SaveSettings();
                 
                }
                else
                {
                    MessageBox.Show(
                        "Error: Could not find CK2 base files in specified directory. Make sure you are pointing where CK2 is INSTALLED (for example in C:\\Program Files (x86)\\Steam\\steamapps\\common\\Crusader Kings II) NOT the My Documents folder",
                        "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    exportButton.Enabled = false;
                    exportButton.Enabled = false;
                    start.Enabled = false;
                    stop.Enabled = false;
                    resetButton.Enabled = false;
                    exportButton.Enabled = false;
                }

                this.mapDir.Text = Globals.GameDir;
                this.ck2dir.Text = Globals.GameDir;
                
            }
        }

        private void modname_TextChanged(object sender, EventArgs e)
        {
            Globals.ModDir = Globals.ModRoot + modname.Text + "\\";
            Globals.ModName = modname.Text;
        }

        private void ck2dir_KeyPress(object sender, KeyPressEventArgs e)
        {
            Globals.GameDir = ck2dir.Text;
            if (Globals.GameDir.Length > 0 && !Globals.GameDir.EndsWith("\\"))
                Globals.GameDir += "\\";

            Globals.MapDir = Globals.GameDir;
            if(Directory.Exists(Globals.MapDir))
                LoadFiles(resetWorker);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
           
        }

        private void ck2dir_TextChanged(object sender, EventArgs e)
        {
            Globals.GameDir = ck2dir.Text;
        }

        private void mapDir_TextChanged(object sender, EventArgs e)
        {
            Globals.MapDir = mapDir.Text;
        }

        private void ForceStart_Click(object sender, EventArgs e)
        {
            MapManager.instance.ToFill.Clear();
            SimulationManager.instance.bDonePreStage1 = true;
        
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void kingStability_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.KingdomStability = kingStability.SelectedIndex;
        }

        private void empStability_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.EmpireStability = empStability.SelectedIndex;
        }

        private void conquererAmount_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.Conquerers = conquererAmount.SelectedIndex;
        }

        private void govMutate_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.GovernmentMutate = govMutate.SelectedIndex;

        }

        private void relMutate_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.ReligionMutate = relMutate.SelectedIndex;

        }

        private void culMutate_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.CultureMutate = culMutate.SelectedIndex;

        }

        private void techAdvanceSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.TechAdvanceRate = techAdvanceSpeed.SelectedIndex;

        }

        private void techSpreadSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.TechSpreadRate = techSpreadSpeed.SelectedIndex;

        }

        private void holdingDevSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerationOptions.HoldingDevSpeed = holdingDevSpeed.SelectedIndex;

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripButton4.CheckState = CheckState.Unchecked;
            toolStripButton1.CheckState = CheckState.Checked;
            toolStripButton2.CheckState = CheckState.Unchecked;
            government.CheckState = CheckState.Unchecked;
            MapManager.instance.MapMode = MapManager.MapModeType.Political;
            CaptureMode.Text = "Conquer";
            RefreshTree();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            toolStripButton4.CheckState = CheckState.Unchecked;
            toolStripButton2.CheckState = CheckState.Checked;
            toolStripButton1.CheckState = CheckState.Unchecked;
            government.CheckState = CheckState.Unchecked;
            MapManager.instance.MapMode = MapManager.MapModeType.Religion;
            CaptureMode.Text = "Spread Religion";
            RefreshTree();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            toolStripButton4.CheckState = CheckState.Checked;
            toolStripButton2.CheckState = CheckState.Unchecked;
            toolStripButton1.CheckState = CheckState.Unchecked;
            government.CheckState = CheckState.Unchecked;
            MapManager.instance.MapMode = MapManager.MapModeType.Culture;
            CaptureMode.Text = "Spread Culture";
            RefreshTree();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            toolStripButton4.CheckState = CheckState.Unchecked;
            toolStripButton2.CheckState = CheckState.Unchecked;
            toolStripButton1.CheckState = CheckState.Unchecked;
            government.CheckState = CheckState.Checked;
            MapManager.instance.MapMode = MapManager.MapModeType.Government;

        }
        private void inspectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MapManager.instance.SelectedProvinces.Clear();



            switch (MapManager.instance.MapMode)
            {
                case MapManager.MapModeType.Political:
                {
                    var title = (e.Node.Tag as TitleParser);

                    var list = title.GetAllProvincesTitleOnly();
                    MapManager.instance.SelectedProvinces.AddRange(list);
                    List<object> proxies = new List<object>();
                    foreach (var inspectTreeSelectedNode in inspectTree.SelectedNodes)
                    {
                        proxies.Add(new TitleProxy(inspectTreeSelectedNode.Tag as TitleParser));
                    }

                    propertyGrid.SelectedObjects = proxies.ToArray();

                    break;
                }
                case MapManager.MapModeType.Religion:
               //     if (SelectedReligion != null)
                    {
                        var title = (e.Node.Tag as ReligionParser);
                        if (title != null)
                        {
                            var list = title.Provinces;
                            MapManager.instance.SelectedProvinces.AddRange(list);

                        }
                        List<object> relproxies = new List<object>();
                        List<object> relgproxies = new List<object>();
                        foreach (var inspectTreeSelectedNode in inspectTree.SelectedNodes)
                        {
                            if (inspectTreeSelectedNode.Tag is ReligionParser)
                                relproxies.Add(new ReligionProxy(inspectTreeSelectedNode.Tag as ReligionParser));
                            else
                                relgproxies.Add(
                                    new ReligionGroupProxy(inspectTreeSelectedNode.Tag as ReligionGroupParser));
                        }

                        if (relproxies.Count == 0 && relgproxies.Count > 0)
                            propertyGrid.SelectedObjects = relgproxies.ToArray();
                        else
                        {
                            propertyGrid.SelectedObjects = relproxies.ToArray();
                        }

                        break;
                    }
                    break;
                case MapManager.MapModeType.Culture:
                    if (SelectedCulture != null)
                    {
                        var title = (e.Node.Tag as CultureParser);
                        if (title != null)
                        {
                            var list = title.Provinces;
                            MapManager.instance.SelectedProvinces.AddRange(list);

                        }
                        List<object> relproxies = new List<object>();
                        List<object> relgproxies = new List<object>();
                        foreach (var inspectTreeSelectedNode in inspectTree.SelectedNodes)
                        {
                            if (inspectTreeSelectedNode.Tag is CultureParser)
                                relproxies.Add(new CultureProxy(inspectTreeSelectedNode.Tag as CultureParser));
                            else
                                relgproxies.Add(
                                    new CultureGroupProxy(inspectTreeSelectedNode.Tag as CultureGroupParser));
                        }
                        if(relproxies.Count==0 && relgproxies.Count > 0)
                            propertyGrid.SelectedObjects = relgproxies.ToArray();
                        else
                        {
                            propertyGrid.SelectedObjects = relproxies.ToArray();
                        }
                        

                    }
                    break;
            }
            renderPanel.Invalidate();
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            MapGenerator gen = new MapGenerator();

            gen.ShowDialog(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            autoload = true;
            reset_Click(null, e);
        }

        public void clear()
        {
            ArbitaryFileEditor.instance = new ArbitaryFileEditor();
            ScripterTriggerManager.instance = new ScripterTriggerManager();
            OnActionsManager.instance = new OnActionsManager();

            BookmarkManager.instance = new BookmarkManager();
            ModManager.instance = new ModManager();
            ModManager.instance.Init();
            DynastyManager.instance = new DynastyManager();
            EventManager.instance = new EventManager();
            GovernmentManager.instance = new GovernmentManager();
            TechnologyManager.instance = new TechnologyManager();
            SimulationManager.instance.Active = false;
            Thread.Sleep(200);
            SimulationManager.instance = new SimulationManager();
            FlagManager.instance = new FlagManager();
            CulturalDnaManager.instance = new CulturalDnaManager();
            CultureManager.instance = new CultureManager();
            ReligionManager.instance = new ReligionManager();
            CharacterManager.instance = new CharacterManager();
            LanguageManager.instance = new LanguageManager();
            MapManager.instance = new MapManager();
            TitleManager.instance = new TitleManager();
            EventManager.instance = new EventManager();
            DecisionManager.instance = new DecisionManager();
            SpriteManager.instance = new SpriteManager();
            TraitManager.instance = new TraitManager();
            
            start.Enabled = false;
            stop.Enabled = false;
            resetButton.Enabled = false;
            exportButton.Enabled = false;
            button2.Enabled = true;
        }

        public void SetMap(string mapOutputTotalDir)
        {
            mapDir.Text = mapOutputTotalDir;
            Globals.MapDir = mapOutputTotalDir;
            reset_Click(null, new EventArgs());
        }
        ConcurrentQueue<String> toAdd = new ConcurrentQueue<string>();
        public void Log(string s)
        {
            
            toAdd.Enqueue(s);
            
        }

        private void chooseMods_Click(object sender, EventArgs e)
        {
            ModSelect sel = new ModSelect();
            sel.ShowDialog();

            int i = 1;
            Globals.Settings.Where(k=>k.Key.StartsWith("Mod")).ToList().ForEach(o=> Globals.Settings.Remove(o.Key));
            ModManager.instance.ModsToLoad.Clear();
      
            foreach (var item in sel.activeMods.Items)
            {
                String mod = item.ToString();
                Globals.Settings["Mod" + i] = mod;             
            }
            Globals.SaveSettings();

            reset_Click(null, new EventArgs());
        }

        private bool bDown = false;
        private void renderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (MapManager.instance.ProvinceBitmap == null)
                return;

            if (e.Button == MouseButtons.Right)
            {
                DoMiddleMouseDown(e);
                return;
            }
            bDown = true;
            ProvinceParser province = MapManager.instance.GetProvinceAt(e.X, e.Y);
            if (province == null)
                return;
            if (province.Title == null)
                return;
            Rectangle rect = new Rectangle();
            
            if (MapManager.instance.SelectedProvinces != null)
            {
                rect = GetInvalidateRect(MapManager.instance.SelectedProvinces);

                renderPanel.Invalidate(rect);
            }
            if (mouseMode == MouseMode.Select)
            {
                List<ProvinceParser> provs = GetAllProvincesForMapMode(province);
                MapManager.instance.SelectedProvinces = provs.ToList();
                MapManager.instance.SelectedProvinces = MapManager.instance.SelectedProvinces.Distinct().ToList();


            }
            rect = GetInvalidateRect(MapManager.instance.SelectedProvinces);

            renderPanel.Invalidate(rect);

            if (mouseMode == MouseMode.Capture)
            {
                var provs = GetAllProvincesForMapMode(province); ;
                MapManager.instance.SelectedProvinces = provs.ToList();
                MapManager.instance.SelectedProvinces = MapManager.instance.SelectedProvinces.Distinct().ToList();
                SelectedTitle = province.Title.Holder.GetTopLiegeCharacter(MapManager.instance.MaxLevelAsRank).PrimaryTitle;
                SelectedTitleLast = SelectedTitle;
                SelectedReligion = province.Religion;
                SelectedCulture = province.Culture;
            }

            rect = GetInvalidateRect(MapManager.instance.SelectedProvinces);

            renderPanel.Invalidate();
        }

        private PointF panStart;
        private bool panning = false;
        private void DoMiddleMouseDown(MouseEventArgs e)
        {
            panning = true;
            panStart = new PointF(MapManager.instance.ConvertCanvasXToWorld(e.X), MapManager.instance.ConvertCanvasYToWorld(e.Y));
            

        }
        private void DoMiddleMouseUp(MouseEventArgs e)
        {
            panning = false;
        }
   

        private List<ProvinceParser> GetAllProvincesForMapMode(ProvinceParser province)
        {
            switch (MapManager.instance.MapMode)
            {
                case MapManager.MapModeType.Political:
                    var t = province.Title.Holder.GetTopLiegeCharacter(MapManager.instance.MaxLevelAsRank);
                    t.GetIslands();
                    var v = t.GetAllProvinces();
                    
                    return v;
                case MapManager.MapModeType.Religion:
                    return province.Religion.Provinces.ToList();
                case MapManager.MapModeType.Culture:
                    return province.Culture.Provinces.ToList();
                    break;
            }

            return null;
        }

        private Rectangle GetInvalidateRect(List<ProvinceParser> selectedProvinces)
        {
            Rectangle r = new Rectangle(0,0,0,0);
            foreach (var selectedProvince in selectedProvinces)
            {
                if (MapManager.instance.ProvinceBitmaps.ContainsKey(selectedProvince.id))
                {
                    var p = MapManager.instance.ProvinceBitmaps[selectedProvince.id];

                    int xx = (int) MapManager.instance.ConvertWorldXToCanvas(p.MapPoint.X);
                    int yy = (int) MapManager.instance.ConvertWorldYToCanvas(p.MapPoint.Y);
                    int ww = (int)MapManager.instance.ConvertWorldXToCanvas(p.MapPoint.X+p.Bitmap.Width);
                    int hh = (int)MapManager.instance.ConvertWorldYToCanvas(p.MapPoint.Y + p.Bitmap.Height);
                    ww -= xx;
                    hh -= yy;
                    if (r.Width == 0)
                    {
                        r.X = xx;
                        r.Y = yy;
                        r.Width = 1;
                        r.Height = 1;
                        int w = ww;
                        int h = hh;
                        r.X -= w / 2;
                        r.Y -= h / 2;
                        r.Width = w;
                        r.Height = h;
                    }
                    else
                    {
                        if (r.X > xx)
                        {
                            int or = r.Right;
                            r.X = xx;
                            r.Width = or - r.X;
                        }
                        if (r.Y > yy)
                        {
                            int or = r.Bottom;
                            r.Y = yy;
                            r.Height = or - r.Y;
                        }
                        if (r.Right < xx)
                        {
                            r.Width = xx - r.X;
                        }
                        if (r.Bottom < yy)
                        {
                            r.Height = yy - r.Y;
                        }

                    }


                }
            }

            r.Inflate(96, 96);
            return r;
        }

        private void renderPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (MapManager.instance.ProvinceBitmap == null)
                return;

            if (e.Button == MouseButtons.Right)
            {
                DoMiddleMouseUp(e);
                return;
            }

            bDown = false;
            ProvinceParser province = MapManager.instance.GetProvinceAt(e.X, e.Y);
            if (province != null)

            {
                if (province.Title == null)
                    return;
                if (mouseMode == MouseMode.Select)
                {
                    var provs = GetAllProvincesForMapMode(province); ;
                    MapManager.instance.SelectedProvinces = provs.ToList();
                    MapManager.instance.SelectedProvinces = MapManager.instance.SelectedProvinces.Distinct().ToList();

                    SelectedTitle = province.Title.Holder.GetTopLiegeCharacter(MapManager.instance.MaxLevelAsRank).PrimaryTitle;
                    SelectedTitleLast = SelectedTitle;
                    SelectedReligion = province.Religion;
                    SelectedCulture = province.Culture;
                    
                }
            }
         
            if (mouseMode == MouseMode.Capture)
            {
                switch (MapManager.instance.MapMode)
                {
                     case MapManager.MapModeType.Political:
                        if (SelectedTitle != null)
                        {
                            MapManager.instance.SelectedProvinces.RemoveAll(a => a.Title == null || a.Title.TopmostTitle == SelectedTitle);
                            SelectedTitle.Capture(MapManager.instance.SelectedProvinces);
                            SimulationManager.instance.TickSystem(true);
                            Form1.doneTick = true;
                            RefreshTree();
                        }
                        break;
                    case MapManager.MapModeType.Religion:
                        if (SelectedReligion != null)
                        {
                            SelectedReligion.AddProvinces(MapManager.instance.SelectedProvinces);
                            SimulationManager.instance.TickSystem(true);
                            Form1.doneTick = true;
                            RefreshTree();
                        }
                        break;
                    case MapManager.MapModeType.Culture:
                        if (SelectedCulture != null)
                        {
                            SelectedCulture.AddProvinces(MapManager.instance.SelectedProvinces);
                            SimulationManager.instance.TickSystem(true);
                            Form1.doneTick = true;
                            RefreshTree();
                        }
                        break;

                }

                MapManager.instance.SelectedProvinces = new List<ProvinceParser>();
            }

            renderPanel.Invalidate();
            if (province != null)

            {
                switch (MapManager.instance.MapMode)
                {
                    case MapManager.MapModeType.Political:
                        {
                            TitleProxy proxy = new TitleProxy(SelectedTitle);
                            propertyGrid.SelectedObject = proxy;

                            break;
                        }
                    case MapManager.MapModeType.Religion:
                        if (SelectedReligion != null)
                        {
                            propertyGrid.SelectedObject = new ReligionProxy(SelectedReligion);
                        }
                        break;
                    case MapManager.MapModeType.Culture:
                        if (SelectedCulture != null)
                        {
                            propertyGrid.SelectedObject = new CultureProxy(SelectedCulture);
                        }
                        break;
                }
            }
        }

        internal TitleParser SelectedTitle;
        internal TitleParser SelectedTitleLast;
        private ReligionParser SelectedReligion;
        private CultureParser SelectedCulture;
        private void renderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (MapManager.instance.ProvinceBitmap == null)
                return;

            if (panning)
            {
                float difx = MapManager.instance.ConvertCanvasXToWorld(e.X) - panStart.X;
                float dify = MapManager.instance.ConvertCanvasYToWorld(e.Y) - panStart.Y;


                MapManager.instance.Centre.X -= difx;
                MapManager.instance.Centre.Y -= dify;

                panStart.X = MapManager.instance.ConvertCanvasXToWorld(e.X);
                panStart.Y = MapManager.instance.ConvertCanvasYToWorld(e.Y);
               renderPanel.Invalidate();
            }
            if (bDown)
            {
                List<ProvinceParser> province = MapManager.instance.GetProvincesInRect(e.X, e.Y, 16);
               
                
                if (mouseMode == MouseMode.Capture)
                {
                    MapManager.instance.SelectedProvinces.AddRange(province);
                    MapManager.instance.SelectedProvinces = MapManager.instance.SelectedProvinces.Distinct().ToList();
                }
                Rectangle rect = GetInvalidateRect(province);

               
                if(MapManager.instance.Zoom > 2)
                    renderPanel.Invalidate();
                else
                {
                    renderPanel.Invalidate(rect);
                }

            }
          
            

  
        }

        private void rerollName_Click(object sender, EventArgs e)
        {
            if (MapManager.instance.SelectedProvinces.Count == 0)
                return;

            var tit = SelectedTitle;
     
            tit.RenameForCulture(tit.Culture);
            TitleProxy proxy = new TitleProxy(tit);
            propertyGrid.SelectedObject = proxy;
            renderPanel.Invalidate();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            renderPanel.Invalidate();
        }

        private void selectMode_Click(object sender, EventArgs e)
        {
            mouseMode = MouseMode.Select;
            selectMode.Checked = true;
            CaptureMode.Checked = false;
        }

        public enum MouseMode
        {
            Select,
            Capture
        }

        public MouseMode mouseMode = MouseMode.Select;
        private void CaptureMode_Click(object sender, EventArgs e)
        {
            mouseMode = MouseMode.Capture;
            selectMode.Checked = false;
            CaptureMode.Checked = true;
        }
        
        private void viewIndependent_Click(object sender, EventArgs e)
        {
            MapManager.instance.MaxLevel = MapManager.PoliticalLevel.Independent;
            viewIndependent.Checked = true;
            viewDuchies.Checked = false;
            viewKingdoms.Checked = false;
        }

        private void viewKingdoms_Click(object sender, EventArgs e)
        {
            MapManager.instance.MaxLevel = MapManager.PoliticalLevel.Kingdom;
            viewKingdoms.Checked = true;
            viewIndependent.Checked = false;
            viewDuchies.Checked = false;
        }

        private void viewDuchies_Click(object sender, EventArgs e)
        {
            MapManager.instance.MaxLevel = MapManager.PoliticalLevel.Duchie;
            viewDuchies.Checked = true;
            viewKingdoms.Checked = false;
            viewIndependent.Checked = false;

        }

        private void addBookmark_Click(object sender, EventArgs e)
        {
            CreateBookmark b = new CreateBookmark();

            if (b.ShowDialog(this) == DialogResult.OK)
            {
                BookmarkManager.instance.AddImportantYear(SimulationManager.instance.Year, b.bookMarkTitle.Text, b.bookMarkDescription.Text);
            }
        }

        private void addDecade_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 10; x++)
            {
                SimulationManager.instance.TickSystem(true);
            }
            renderPanel.Invalidate();
        }

        private void addYear_Click(object sender, EventArgs e)
        {
            SimulationManager.instance.TickSystem(true);
            renderPanel.Invalidate();
        }

        private void renderPanel_MouseHover(object sender, EventArgs e)
        {
            renderPanel.Focus();
        }

        private void SaveProject(XmlWriter writer, string filename)
        {
            //  root.FirstChild.

            CharacterManager.instance.SaveProject(writer);
            TitleManager.instance.SaveProject(writer);
            ReligionManager.instance.SaveProject(writer);
        }

        private void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "CK2 Generator|*.ck2gen";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
            }
        }

        private void saveProjectToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog openFileDialog1 = new SaveFileDialog();

            openFileDialog1.Filter = "CK2 Generator (.ck2gen)|*.ck2gen";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var filename = openFileDialog1.FileName;
                var Settings =  new XmlWriterSettings() { Indent = true, IndentChars = ("\t"), NewLineHandling = NewLineHandling.Entitize };

                using (XmlWriter writer = XmlWriter.Create(filename, Settings))
                {
                 

                    writer.WriteStartDocument();
                    writer.WriteStartElement("project");
                    SaveProject(writer, filename);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

            }
        }

        private void loadHistoryToDate_Click(object sender, EventArgs e)
        {
            SimulationManager.instance.RunSimulationFromHistoryFiles((int)simulationDate.Value);
        }

        private void placeVanillaOnMap_Click(object sender, EventArgs e)
        {
            SimulationManager.instance.PlaceVanillaOnMap();

        }
    }
}
