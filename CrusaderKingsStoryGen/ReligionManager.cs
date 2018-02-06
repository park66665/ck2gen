using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{        
    class ReligionManager : ISerializeXml
    {
        public static ReligionManager instance = new ReligionManager();
        private Script script;
        public List<ReligionParser> AllReligions = new List<ReligionParser>();
        public Dictionary<String, ReligionParser> ReligionMap = new Dictionary<String, ReligionParser>();
        public List<ReligionGroupParser> AllReligionGroups = new List<ReligionGroupParser>();
        
        public ReligionManager()
        {
        
        }

        public Script Script
        {
            get { return script; }
            set { script = value; }
        }

        public Dictionary<String, ReligionGroupParser> GroupMap = new Dictionary<string, ReligionGroupParser>();

        public ReligionGroupParser AddReligionGroup(String name)
        {
            ScriptScope scope = new ScriptScope();
            scope.Name = name;
            
            script.Root.Add(scope);
            
            ReligionGroupParser r = new ReligionGroupParser(scope);

            r.Init();
            GroupMap[name] = r;
            AllReligionGroups.Add(r);

            ScripterTriggerManager.instance.AddTrigger(r);
            return r;
        }

     
        public void DoReligiousEquivelents()
        {
            if (!SaveReligions)
                return;

            if (AllReligionGroups.Count == 1)
                return;
            ReligionGroupParser biggestGroup = null;
            ReligionGroupParser secondGroup = null;
            ReligionGroupParser thirdGroup = null;
            ReligionGroupParser fourthGroup = null;
            ReligionGroupParser fifthGroup = null;
            ReligionGroupParser sixthGroup = null;
            AllReligionGroups.Sort(SortByBelievers);

            if (AllReligionGroups.Count >= 6)
            {

                this.ChristianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count) / 2];
                this.ChristianGroupSub.Name = this.ChristianGroupSub.Scope.Name;
                AllReligionGroups.Remove(ChristianGroupSub);
                this.MuslimGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)/2];
                this.MuslimGroupSub.Name = this.MuslimGroupSub.Scope.Name;
                AllReligionGroups.Remove(MuslimGroupSub);
                this.IndianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)/2];
                this.IndianGroupSub.Name = this.IndianGroupSub.Scope.Name;
                AllReligionGroups.Remove(IndianGroupSub);
                this.ZoroGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)/2];
                ZoroGroupSub.Name = this.ZoroGroupSub.Scope.Name;
                AllReligionGroups.Remove(ZoroGroupSub);
                this.PaganGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)/2];
                PaganGroupSub.Name = this.PaganGroupSub.Scope.Name;
                AllReligionGroups.Remove(PaganGroupSub);
                this.JewGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)/2];
                JewGroupSub.Name = this.JewGroupSub.Scope.Name;
                AllReligionGroups.Remove(JewGroupSub);

                while (ZoroGroupSub == MuslimGroupSub)
                {
                    ZoroGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                }

                while (ChristianGroupSub == MuslimGroupSub)
                {
                    ChristianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                }
                while (JewGroupSub == MuslimGroupSub)
                {
                    JewGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                }

            }
            else
            {
                if (AllReligionGroups.Count > 3)
                {
                    this.ChristianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count/2)];
                    this.ChristianGroupSub.Name = this.ChristianGroupSub.Scope.Name;
                    AllReligionGroups.Remove(ChristianGroupSub);
                    this.MuslimGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count / 2)];
                    this.MuslimGroupSub.Name = this.MuslimGroupSub.Scope.Name;
                    AllReligionGroups.Remove(MuslimGroupSub);
                }
                else
                {
                    this.ChristianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                    this.ChristianGroupSub.Name = this.ChristianGroupSub.Scope.Name;
                    this.MuslimGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                    this.MuslimGroupSub.Name = this.MuslimGroupSub.Scope.Name;

                }
                this.IndianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                this.IndianGroupSub.Name = this.IndianGroupSub.Scope.Name;
                this.ZoroGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                ZoroGroupSub.Name = this.ZoroGroupSub.Scope.Name;
                this.PaganGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                PaganGroupSub.Name = this.PaganGroupSub.Scope.Name;
                this.JewGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                JewGroupSub.Name = this.JewGroupSub.Scope.Name;

                while (ZoroGroupSub == MuslimGroupSub)
                {
                    ZoroGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                }

                while (ChristianGroupSub == MuslimGroupSub)
                {
                    ChristianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                }
                while (JewGroupSub == MuslimGroupSub)
                {
                    JewGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
                }

            }

            this.ChristianGroupSub.Religions.Sort(SortByBelieversReligion);

            this.CatholicSub = this.ChristianGroupSub.Religions[0];
        //    this.CatholicSub.Name = this.CatholicSub.Scope.Name;
            this.OrthodoxSub = this.ChristianGroupSub.Religions[1];
         //   this.OrthodoxSub.Name = this.OrthodoxSub.Scope.Name;
            this.MuslimGroupSub.Religions.Sort(SortByBelieversReligion);

            this.SunniEquiv = this.MuslimGroupSub.Religions[0];
        //    this.SunniEquiv.Name = this.SunniEquiv.Scope.Name;
            this.ShiiteEquiv = this.MuslimGroupSub.Religions[1];
         //   this.ShiiteEquiv.Name = this.ShiiteEquiv.Scope.Name;

            while (IndianGroupSub.Religions.Count < 3)
            {
                this.IndianGroupSub = AllReligionGroups[Rand.Next(AllReligionGroups.Count)];
            //    this.IndianGroupSub.Name = this.IndianGroupSub.Scope.Name;
            }
            { 
                this.HinduEquiv = this.IndianGroupSub.Religions[0];
            //    this.HinduEquiv.Name = this.HinduEquiv.Scope.Name;
                this.BuddhistEquiv = this.IndianGroupSub.Religions[1];
            //    this.BuddhistEquiv.Name = this.BuddhistEquiv.Scope.Name;
                this.JainEquiv = this.IndianGroupSub.Religions[2];
           //     this.JainEquiv.Name = this.JainEquiv.Scope.Name;


            }

            this.PaganGroupSub.Religions.Sort(SortByBelieversReligion);
            this.NorseSub = this.PaganGroupSub.Religions[0];
          //  this.NorseSub.Name = this.NorseSub.Scope.Name;
            this.NorseReformSub = this.PaganGroupSub.Religions[1];
         //   this.NorseReformSub.Name = this.NorseSub.Scope.Name;

            foreach (var religionParser in PaganGroupSub.Religions)
            {
                religionParser.polytheism = true;
                religionParser.hasLeader = false;
            }

            foreach (var religionParser in this.ChristianGroupSub.Religions)
            {
                religionParser.hasLeader = true;
            }
            this.NorseSub.allow_viking_invasion = true;
            this.NorseSub.allow_looting = true;
            this.JainEquiv.pacifist = false;
            this.JainEquiv.can_call_crusade = true;
            this.HinduEquiv.pacifist = true;
            this.HinduEquiv.can_call_crusade = false;
            this.BuddhistEquiv.pacifist = true;
            this.BuddhistEquiv.can_call_crusade = false;

            foreach (var religionParser in AllReligions)
            {
                religionParser.RedoReligionScope();
            }
        }

        public ReligionParser JainEquiv { get; set; }

        public ReligionParser BuddhistEquiv { get; set; }

        public ReligionParser HinduEquiv { get; set; }

        public ReligionParser OrthodoxSub { get; set; }

        public ReligionParser ShiiteEquiv { get; set; }

        public ReligionParser SunniEquiv { get; set; }

        public ReligionParser NorseSub { get; set; }

        public ReligionParser CatholicSub { get; set; }
        public ReligionParser NorseReformSub { get; set; }


        public void Save()
        {
            if (!SaveReligions)
                return;

            int biggest = -1;
         
            LanguageManager.instance.SetupReligionEventSubsitutions();

            var list = new List<ScriptScope>();
            foreach (var child in script.Root.Children)
            {
                list.AddRange((child as ScriptScope)?.Children.Where((o => o is ScriptScope)).Where(o => !((ScriptScope)o).Name.Contains("_names")).Cast<ScriptScope>());                     
            }
            foreach (var religionParser in AllReligions)
            {
                if(religionParser.Provinces.Count > 0)
                    religionParser.CreateSocietyDetails(religionParser.Provinces[0].Culture.Name);

            }

            foreach (var scriptScope in list)
            {
                String name = (scriptScope as ScriptScope)?.Name;
                if (AllReligions.All(c => c.Name != name))
                {
                    System.Console.Out.WriteLine("Cannot find " + name);
            
                }
            }

            script.Save();
        }

        public int SortByBelievers(ReligionGroupParser x, ReligionGroupParser y)
        {
            if (x.Provinces.Count > y.Provinces.Count)
                return -1;
            if (x.Provinces.Count < y.Provinces.Count)
                return 1;

            return 0;
        }

        private int SortByBelieversReligion(ReligionParser x, ReligionParser y)
        {
            if (x.Provinces.Count > y.Provinces.Count)
                return -1;
            if (x.Provinces.Count < y.Provinces.Count)
                return 1;

            return 0;
        }
        public ReligionGroupParser PaganGroupSub { get; set; }
        public ReligionGroupParser JewGroupSub { get; set; }

        public ReligionGroupParser MuslimGroupSub { get; set; }
        public ReligionGroupParser ChristianGroupSub { get; set; }
        public ReligionGroupParser IndianGroupSub { get; set; }

        public ReligionGroupParser ZoroGroupSub { get; set; }
    
        public void Init()
        {
            LanguageManager.instance.Add("norse", StarNames.Generate(Rand.Next(1000000)));
            LanguageManager.instance.Add("pagan", StarNames.Generate(Rand.Next(1000000)));
            LanguageManager.instance.Add("christian", StarNames.Generate(Rand.Next(1000000)));
            
            Script s = new Script();
            script = s; 
            s.Name = Globals.ModDir + "common\\religions\\00_religions.txt";
            s.Root = new ScriptScope();
            ReligionGroupParser r = AddReligionGroup("pagan");
            r.Init();
            var pagan = r.AddReligion("pagan");

            pagan.CreateRandomReligion(null); 
            
            AllReligionGroups.Add(r);
         
        }

        public ReligionParser BranchReligion(string religion,  String culture)
        {
            var rel = this.ReligionMap[religion];
            var group = rel.Group;
            int totCount = 0;
            foreach (var religionParser in rel.Group.Religions)
            {
                totCount += religionParser.Provinces.Count;
            }
            if (!CultureManager.instance.allowMultiCultureGroups)
            {
                String name = StarNames.Generate(culture);
                String safe = StarNames.SafeName(name);
                while (ReligionManager.instance.GroupMap.ContainsKey(safe))
                {
                    name = StarNames.Generate(culture);
                    safe = StarNames.Generate(culture);
                }

                LanguageManager.instance.Add(safe, name);
                group = AddReligionGroup(safe);
                name = StarNames.Generate(culture);
                while (ReligionManager.instance.ReligionMap.ContainsKey(safe))
                {
                    name = StarNames.Generate(culture);
                    safe = StarNames.SafeName(name);
                }

                var rel2 = group.AddReligion(name);
                 rel2.RandomReligionProperties();
                rel2.CreateRandomReligion(group);
                return rel2;
            }
            else
            {
                var rell = StarNames.Generate(culture);

                while (ReligionManager.instance.ReligionMap.ContainsKey(StarNames.SafeName(rell)))
                {

                    rell = StarNames.Generate(culture);
                }
                var rel2 = group.AddReligion(rell);

                rel2.RandomReligionProperties();
                rel2.CreateRandomReligion(group);
  
                rel2.Mutate(rel, CultureManager.instance.CultureMap[culture], 6);
                return rel2;
            }
         
        }

        public void SaveProject(XmlWriter writer)
        {
            writer.WriteStartElement("religions");

            foreach (var religionGroupParser in AllReligionGroups)
            {
                writer.WriteStartElement("group");

                religionGroupParser.SaveXml(writer);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public void LoadProject(XmlReader reader)
        {
            
        }

        public void LoadVanilla()
        {
            SaveReligions = false;
            var files = ModManager.instance.GetFileKeys("common\\religions");
            foreach (var file in files)
            {
                Script s = ScriptLoader.instance.LoadKey(file);
                foreach (var rootChild in s.Root.Children)
                {
                    if ((rootChild as ScriptScope).Name == "secret_religion_visibility_trigger")
                        continue;

                    ReligionGroupParser p = new ReligionGroupParser(rootChild as ScriptScope);

                    AllReligionGroups.Add(p);
                    foreach (var scopeChild in p.Scope.Children)
                    {
                        if (scopeChild is ScriptScope)
                        {
                            var sc = scopeChild as ScriptScope;

                            if (sc.Name == "male_names" ||
                                sc.Name == "female_names")
                            {
                                continue;
                            }

                            ReligionParser r = new ReligionParser(sc);
                            AllReligions.Add(r);
                                ReligionMap[r.Name] = r;
                            p.Religions.Add(r);
                            r.Group = p;
                            r.LanguageName = LanguageManager.instance.Get(r.Name);
                        }
                    }
                }
            }
        }

        public bool SaveReligions { get; set; } = true;
    }
}
