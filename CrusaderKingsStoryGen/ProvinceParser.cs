using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class ProvinceParser :  Parser
    {
        public String Name { get; set; }
        public int id;
        public int provinceRCode;
        public int provinceGCode;
        public int provinceBCode;
        public int militaryTechPoints = 0;
        public int cultureTechPoints = 0;
        public int economicTechPoints = 0;
        public String title { get; set; }
        public int max_settlements { get; set; } = 7;

        public TitleParser ProvinceOwner
        {
            get { return Title; }
            set
            {
                if (value != null)
                    title = value.Name;
                else
                    title = null;
            }
        }

        public List<Barony> baronies = new List<Barony>();
        public String initialReligion = "";
        public String initialCulture =  "";
        public void Save()
        {
   
            if (title == null)
                return;
            if (Title == null)
                return;

            if (!TitleManager.instance.Titles.Contains(Title))
            {
                int gfdgfd = 0;
            }
            string dir = Globals.ModDir + "history\\provinces\\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            String provincesDir = Globals.MapDir + "history\\provinces\\" + id.ToString() + " - " + Title.Name.Replace("c_", "") + ".txt";
        
            Script s = new Script();
            s.Root = new ScriptScope();
            s.Name = provincesDir;
            s.Root.Add(new ScriptCommand("title", Title.Name, s.Root));
            s.Root.Add(new ScriptCommand("max_settlements", max_settlements, s.Root));
            


            if (Culture != null)
            {
                s.Root.Add(new ScriptCommand("culture", initialCulture, s.Root));
                s.Root.Add(new ScriptCommand("religion", initialReligion, s.Root));
                
            }
            if (MapManager.instance.LoadedTerrain.ContainsKey(id))
                s.Root.Add(new ScriptCommand("terrain", MapManager.instance.LoadedTerrain[id], s.Root));


        
            int cc = 0;
            foreach (var barony in baronies)
            {
                if (baronies[0].type == "tribal" && barony.type != "temple" && cc > 0)
                    continue;
                if(barony.enabled && cc==0)
                    s.Root.Add(new ScriptCommand(barony.title, barony.type, s.Root));
                cc++;
            }

            var cities = baronies.Where(c => c.type == "city" && c.enabled);

            if(cities.Any())
            if (Adjacent.Where(o => !o.land).Count() > 0)
                s.Root.Do(@"1.1.1 = { 
                   " + cities.First().title + @" = ct_port_1
                  }");

            if (terrain != null)
                s.Root.Add(new ScriptCommand("terrain", terrain, s.Root));

            foreach (var scriptScope in dateScripts)
            {
                s.Root.SetChild(scriptScope);
            }

            s.Save();
        }

        public void Rename(string name)
        {
            if (StarNames.SafeName(name) == "selj")
            {

            }
            String oldName = Name;
            Name = StarNames.SafeName(name);
            LanguageManager.instance.Remove(Name);
            LanguageManager.instance.Add(StarNames.SafeName(name), name);
            LanguageManager.instance.Add("c_" + StarNames.SafeName(name), name);
            LanguageManager.instance.Add("PROV" + id, name);
            MapManager.instance.ProvinceMap.Remove(oldName);
            MapManager.instance.ProvinceMap[Name] = this;

            if (Title != null)
            {
                Title.Rename(name, true);
            }
        }
        public void RenameSafe(string name)
        {
       
            String oldName = Name;
            Name = name;
            LanguageManager.instance.Remove(Name);         
            MapManager.instance.ProvinceMap.Remove(oldName);
            MapManager.instance.ProvinceMap[Name] = this;
            title = Name;
            if (Title != null)
            {
                Title.RenameSafe(name, true);
            }
        }

        public void RenameForCulture(CultureParser culture)
        {
           LanguageManager.instance.Remove(Name);

            String namem = null;
           
            do
            {
                var name = culture.dna.GetPlaceName();
                namem = name;
   
            } while (MapManager.instance.ProvinceMap.ContainsKey(StarNames.SafeName(namem)));

            Rename(namem);

        }
        public void AddTemple(CultureParser culture)
        {
            AddBarony("temple", culture);
        }

        public void CreateProvinceDetails(CultureParser culture)
        {
           
            {
                AddBarony("tribal", culture);

            }
            AddAdditionalBaronies(culture);
        }

        private void AddAdditionalBaronies(CultureParser culture)
        {
            for (int x = 0; x < 7; x++)
            {
                AddBarony("tribal", culture, false);
           
            }
        }

        public void AddBarony(CultureParser culture)
        {
            AddBarony("tribal", culture);

        }

        public void AddBarony2(CultureParser culture)
        {
            AddBarony("tribal", culture);

        }

        public void AddBarony(string type, CultureParser culture, bool enabled = true)
        {
            TitleParser title = TitleManager.instance.CreateBaronyScriptScope(this, culture);
            baronies.Add(new Barony() { province = this, title = title.Name, titleParser = title, type = type, enabled = enabled });
            this.Title.AddSub(title);
        }
        public void AddBarony(string name, TitleParser title)
        {
             baronies.Add(new Barony() { province = this, title = name, titleParser = title, enabled =false});
         }

        public CharacterParser TotalLeader
        {
            get
            {
                var title = this.ProvinceOwner;
                if (title == null)
                    return null;
                while (title.Liege != null && title.Rank < title.Liege.Rank)
                {
                    title = title.Liege;
                }
                return title.Holder;
            }
        }

        public Color Color { get; set; }


        public TitleParser Title
        {
            get
            {
                if (title == null)
                    return null;

                if (!TitleManager.instance.TitleMap.ContainsKey(title))
                {
                    if (Name != null)
                    {
                        if (TitleManager.instance.TitleMap.ContainsKey(Name))
                        {
                            title = Name;
                                return TitleManager.instance.TitleMap[Name];

                        }
                    }
            
                    return null;
                }

                return TitleManager.instance.TitleMap[title];
            }
        }
        public bool loadingFromHistoryFiles { get; set; }
        public CultureParser Culture
        {
            get { return _culture; }
            set
            {
                if (_culture != null)
                {
                    _culture.RemoveProvince(this);
                }
                if (_culture != value && value != null && initialCulture!="" && !loadingFromHistoryFiles)
                {
                    ScriptScope thing = new ScriptScope();
                    thing.Name = SimulationManager.instance.Year + ".4.1";
                    thing.Add(new ScriptCommand() { Name = "culture", Value = value.Name });

                    dateScripts.Add(thing);
                }
                _culture = value;
                _culture.AddProvince(this);
            }
        }
        public List<ScriptScope> dateScripts = new List<ScriptScope>();

        public ReligionParser Religion
        {
            get { return _religion; }
            set
            {
                if (_religion != null)
                {
                    _religion.RemoveProvince(this);
                }
                if (_religion != value && value != null && initialReligion != "")
                {
                    ScriptScope thing = new ScriptScope();
                    thing.Name = SimulationManager.instance.Year + ".3.1";
                    thing.Add(new ScriptCommand() { Name = "religion", Value = value.Name });

                    dateScripts.Add(thing);
                }

                _religion = value;
                _religion.AddProvince(this);

            }
        }

        public int ActiveBaronies
        {
            get
            {
                int c = 0;
                foreach (var barony in baronies)
                {
                    if (barony.enabled)
                        c++;
                }

                return c;
            }
        }

        public string government { get; set; } = "tribal";

        public int CastleCount
        {
            get
            {
                int c = 0;
                foreach (var barony in baronies)
                {
                    if (barony.enabled && (barony.type == "castle" || barony.type == "tribal"))
                        c++;
                }

                return c;
            }
        }
        public int TempleCount
        {
            get
            {
                int c = 0;
                foreach (var barony in baronies)
                {
                    if (barony.enabled && barony.type == "temple")
                        c++;
                }

                return c;
            }
        }

        public int TownCount
        {
            get
            {
                int c = 0;
                foreach (var barony in baronies)
                {
                    if (barony.enabled && barony.type == "city")
                        c++;
                }

                return c;
            }
        }

        public Point Range { get; set; }
        public Rectangle Bounds { get; set; }
        public string EditorName { get; set; }

        public List<Point> Points = new List<Point>();
        public override string ToString()
        {
            return id + " - " + title;
        }

        public ProvinceParser(ScriptScope scope) : base(scope)
        {
            int line = 0;
            foreach (var child in scope.Children)
            {
                if (child is ScriptCommand)
                {
                    RegisterProperty(line, ((child as ScriptCommand).Name), child);

                }
                line++;
                if (child is ScriptScope)
                {
                    var subscope = (child as ScriptScope);
                   
                }
            }
           
        }

        public void DoTitleOwnership()
        {
            if (title != null && TitleManager.instance.TitleMap.ContainsKey(title))
            {
                TitleManager.instance.TitleMap[title].Owns.Add(this);
                ProvinceOwner = TitleManager.instance.TitleMap[title];
            }
        }

        public override ScriptScope CreateScope()
        {
            return null;
        }

        public List<ProvinceParser> Adjacent = new List<ProvinceParser>();
        public void AddAdjacent(ProvinceParser prov)
        {
            if (!Adjacent.Contains(prov))
                Adjacent.Add(prov);
            if (!prov.Adjacent.Contains(this))
                prov.Adjacent.Add(this);
        }

        public bool IsAdjacentToUnclaimed()
        {
            foreach (var provinceParser in Adjacent)
            {
                if (!provinceParser.Title.Claimed)
                    return true;
            }
            return false;
        }

        public class Barony
        {
            public String type;
            public String title;
            public ProvinceParser province;
            public TitleParser titleParser;
            public int level;
            public bool enabled { get; set; }
        }

        public List<Barony> Temples = new List<Barony>();
        public bool land = false;
        public int templeRequirement;
        private CultureParser _culture;
        private ReligionParser _religion;
        public bool republic;
        public string terrain;
        public bool river;

        public void GatherBaronies()
        {
            foreach (var child in Scope.Children)
            {
                if (child is ScriptCommand)
                {
                    ScriptCommand c = (ScriptCommand) child;

                    if (c.Name.StartsWith("b_"))
                    {
                        String str = c.Value.ToString();
                    
                        if (c.Value.ToString() == "temple")
                        {
                            var t = new Barony() {type = c.Value.ToString(), title = c.Name, province = this};
                            Temples.Add(t);
                            MapManager.instance.Temples[c.Name] = t;
                        }
                    }
                }
            }
        }

        public TitleParser CreateTitle()
        {
            this.title = "c_" + this.Name;

        
            var scope = new ScriptScope();
            scope.Name = this.title;
            var c = new TitleParser(scope);
            c.capital = this.id;
            c.CapitalProvince = this;
            c.Owns.Add(this);
            TitleManager.instance.AddTitle(c);
            return c;
        }

        public CharacterParser GetCurrentHolder()
        {
            if (this.title == null)
                return null;
            if (this.Title.CurrentHolder != null)
                return this.Title.CurrentHolder;

            return this.Title.Holder;
        }

        public float DistanceTo(ProvinceParser other)
        {
            var x = other;
            var p = this.Points[0];
            float a = p.X - x.Points[0].X;
            float b = p.Y - x.Points[0].Y;

            return (Math.Abs(a) + Math.Abs(b));
        }

        public bool IsAdjacentToSea()
        {
            foreach (var provinceParser in Adjacent)
            {
                if (!provinceParser.land)
                    return true;
            }

            return false;
        }

        public void AddTown()
        {
            for (int index = 1; index < baronies.Count; index++)
            {
                var barony = baronies[index];
                if (!barony.enabled)
                {
                    barony.enabled = true;
                    barony.type = "town";
                    return;
                }
            }
        }

        public Barony GetLastEnabledBarony()
        {
            for (int index = 0; index < baronies.Count; index++)
            {
                var barony = baronies[index];
                if (!barony.enabled)
                {
                    return baronies[index - 1];
                }
            }

            return null;
        }
        public Barony GetNextBarony()
        {
            for (int index = 0; index < baronies.Count; index++)
            {
                var barony = baronies[index];
                if (!barony.enabled)
                {
                    return baronies[index];
                }
            }

            return null;
        }

        public void ActivateBarony(string eName, string type)
        {
            List<Barony> newList = new List<Barony>();
            List<Barony> newListUnenabled = new List<Barony>();

            for (int index = 0; index < baronies.Count; index++)
            {
                var barony = baronies[index];
                if (barony.titleParser.Name == eName)
                {
                    if (type == "castle" || type == "temple" || type == "city" || type == "tribal")
                    {

                        barony.enabled = true;
                        barony.type = type;

                        newList.Add(barony);

                    }
                }
                else if (barony.enabled)
                {
                    newList.Add(barony);

                }
                else
                {
                    newListUnenabled.Add(barony);
                }
            }

            baronies = new List<Barony>();
            baronies.AddRange(newList);
            baronies.AddRange(newListUnenabled);
        }

        public List<ProvinceParser> GetAdjacentLand(int i, List<ProvinceParser> list)
        {
            if (i <= 0)
                return list;
             foreach (var provinceParser in Adjacent)
            {
                if (provinceParser.land)
                    list.Add(provinceParser);
                else
                    provinceParser.GetAdjacentLand(i - 1, list);
            }

            return list;;
        }

     
    }
}