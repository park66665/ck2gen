using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrusaderKingsStoryGen
{
    class CultureGroupParser : Parser
    {
        public List<CultureParser> Cultures = new List<CultureParser>();
        internal string chosenGfx;
        public int r;
        public int b;
        public int g;
        public List<Government> Governments = new List<Government>();
        private string preferedSucc;
        private string preferedGender;

        public String PreferedSuccession
        {
            get
            {
                if (preferedSucc == null)
                {
                    switch (Rand.Next(10))
                    {
                        case 0:
                        case 1:
                            preferedSucc = "succ_gavelkind";
                            break;
                        case 2:
                        case 3:
                            preferedSucc = "succ_primogeniture";
                            break;
                        case 4:
                            preferedSucc = "succ_feudal_elective";
                            break;
                        case 5:
                            preferedSucc = "succ_tanistry";
                            break;
                        case 6:
                            preferedSucc = "succ_ultimogeniture";
                            break;
                        case 7:
                            preferedSucc = "succ_seniority";
                            break;
                        case 8:
                            preferedSucc = "succ_elective_gavelkind";
                            break;
                        case 9:
                            preferedSucc = "succ_open_elective";
                            break;

                    }
                }
                return preferedSucc;
            }
        }
        public String PreferedGenderLaw
        {
            get
            {
                if (preferedGender == null)
                {
                    switch (Rand.Next(6))
                    {
                        case 0:
                        case 1:
                        case 2:
                            preferedGender = "agnatic_succession";
                            break;
                        case 3:
                        case 4:
                        case 5:
                            preferedGender = "cognatic_succession";
                            break;
                     
                            

                    }
                }
                return preferedGender;
            }
        }

        public CultureGroupParser(ScriptScope scope)
            : base(scope)
        {
            if (scope.UnsavedData.ContainsKey("color"))
            {
                var col = (Color) Scope.UnsavedData["color"];

                r = col.R;
                g = col.G;
                b = col.B;
            }
            foreach (var scriptScope in scope.Scopes)
            {

                if (CultureManager.instance.CultureMap.ContainsKey(scriptScope.Name))
                    Cultures.Add(CultureManager.instance.CultureMap[scriptScope.Name]);
            }
        }

        public ReligionGroupParser ReligionGroup { get; set; }

        public void RemoveCulture(String name)
        {
            var r = CultureManager.instance.CultureMap[name];
            Cultures.Remove(r);
            Scope.Remove(r);
        }

        public void AddCulture(CultureParser r)
        {
            r.group = this;
            if (r.Group != null)
            {
                r.Group.Scope.Remove(r.Scope);
            }
            Scope.Add(r.Scope);
            Cultures.Add(r);
            Cultures = Cultures.Distinct().ToList();
        }

        public void AddTreeNode(TreeView inspectTree)
        {
            var res = inspectTree.Nodes.Add(Name.Lang());
            res.Tag = this;
            res.ImageIndex = 0;
            foreach (var religionParser in Cultures)
            {
                if(religionParser.LanguageName == "")
                    continue;
                
                var res2 = res.Nodes.Add(religionParser.LanguageName);
                res2.Tag = religionParser;
                res2.ImageIndex = 0;
            }
        }
        public CultureParser AddCulture(String name)
        {
            string langName = "";
            if (name != "norse")
            {
                String oname = name;
                name = StarNames.SafeName(name);

                LanguageManager.instance.Add(name, oname);
                langName = oname;
            }


            ScriptScope scope = new ScriptScope();
            scope.Name = name;

            Scope.Add(scope);
            CultureParser r = new CultureParser(scope, this);
            CultureManager.instance.AllCultures.Add(r);
            r.LanguageName = langName;
            Cultures.Add(r);
            CultureManager.instance.CultureMap[name] = r;
            r.Name = name;
            r.Init();
            r.group = this;
            var col = Col();
            r.r = col.R;
            r.g = col.G;
            r.b = col.B;
            Scope.SetChild(r.Scope);
            return r;

        }

        private Color Col()
        {
            var r = color.R + Rand.Next(-40, 40);
            if (r > 255)
                r = 255;
            if (r < 0)
                r = 0;
            var g = color.G + Rand.Next(-40, 40);
            if (g > 255)
                g = 255;
            if (g < 0)
                g = 0;

            var b = color.B + Rand.Next(-40, 40);
            if (b > 255)
                b = 255;
            if (b < 0)
                b = 0;
            return Color.FromArgb(255, r, g, b);
        }

        public Color color;

        public override ScriptScope CreateScope()
        {
            return null;
        }

        public void Init()
        {
            Scope.Clear();
            chosenGfx = CultureParser.GetRandomCultureGraphics();
            Scope.Do(@"
                
	            graphical_cultures = { 
                  " + chosenGfx + @" 
                }
");

            r = Rand.Next(255);
            g = Rand.Next(255);
            b = Rand.Next(255);

            Scope.UnsavedData["color"] = Color.FromArgb(255, r, g, b);
        }

        public void RemoveProvince(ProvinceParser provinceParser)
        {
            Provinces.Remove(provinceParser);
        }

        public void AddProvince(ProvinceParser provinceParser)
        {
            if (!Provinces.Contains(provinceParser))
                Provinces.Add(provinceParser);
        }

        public List<ProvinceParser> Provinces = new List<ProvinceParser>();
    }

    class CultureParser : Parser
    {
        public CultureGroupParser Group
        {
            get { return group; }
            set { group = value; }
        }

        public string LanguageName { get; set; }
        public CulturalDna dna { get; set; }
     
        public List<Dynasty> Dynasties = new List<Dynasty>();
        private bool dirty = true;
        Rectangle _bounds = new Rectangle();
        public Rectangle Bounds
        {
            get
            {
                if (dirty)
                    _bounds = GetBounds();
                dirty = false;
                return _bounds;
            }
        }

        public Point TextPos
        {
            get
            {
                if (dirty)
                    _bounds = GetBounds();
                dirty = false;

                return _textPos;
            }
            set { _textPos = value; }
        }

        public Rectangle GetBounds()
        {
            var prov = Provinces;

            float avx = 0;
            float avy = 0;

            Rectangle tot = Rectangle.Empty;
            foreach (var provinceParser in prov)
            {
                int cx = provinceParser.Bounds.X + (provinceParser.Bounds.Width / 2);
                int cy = provinceParser.Bounds.Y + (provinceParser.Bounds.Height / 2);

                avx += cx;
                avy += cy;


                if (tot.Width == 0)
                    tot = provinceParser.Bounds;
                else
                {
                    if (tot.Left > provinceParser.Bounds.Left)
                    {
                        int right = tot.Right;
                        tot.X = provinceParser.Bounds.Left;
                        tot.Width = (right - tot.X);
                    }
                    if (tot.Top > provinceParser.Bounds.Top)
                    {
                        int right = tot.Top;
                        tot.Y = provinceParser.Bounds.Top;
                        tot.Height = (right - tot.Y);
                    }
                    if (tot.Right < provinceParser.Bounds.Right)
                    {
                        tot.Width = provinceParser.Bounds.Right - tot.X;
                    }

                    if (tot.Bottom < provinceParser.Bounds.Bottom)
                    {
                        tot.Height = provinceParser.Bounds.Bottom - tot.Y;
                    }
                }
            }
            avx /= prov.Count;
            avy /= prov.Count;
            TextPos = new Point((int)avx, (int)avy);
            return tot;
        }

        public CultureParser(ScriptScope scope, CultureGroupParser group) : base(scope)
        {
            this.group = group;
        }

        public override ScriptScope CreateScope()
        {
            return null;
        }


        public void Init()
        {
            if (Name == "kedaras")
            {

            }
            //        Scope.Clear();

            String fx = Group.chosenGfx;
            if (Group.chosenGfx == null)
            {
                fx = Group.Scope.Scopes[0].Data;
            }
            int r = Rand.Next(255);
            int g = Rand.Next(255);
            int b = Rand.Next(255);

            r = Group.r;
            g = Group.g;
            b = Group.b;
            switch (Rand.Next(3))
            {
                case 0:
                    r += Rand.Next(-45, 45);
                    g += Rand.Next(-25, 25);
                    b += Rand.Next(-15, 15);

                    break;
                case 1:
                    g += Rand.Next(-45, 45);
                    r += Rand.Next(-25, 25);
                    b += Rand.Next(-15, 15);

                    break;
                case 2:
                    b += Rand.Next(-45, 45);
                    g += Rand.Next(-25, 25);
                    r += Rand.Next(-15, 15);

                    break;
            }
            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;

            if (r < 0)
                r = 0;
            if (g < 0)
                g = 0;
            if (b < 0)
                b = 0;

            Scope.Do(@"
                
		         color = { " + r + " " + g + " " + b + @" }
        ");

        }

        public void DoDetailsForCulture()
        {
         
             dna.culture = this;
            if (dna.portraitPool.Count == 0)
            {
                int c = 1;
                String cul = GetRandomCultureGraphics();
                for (int i = 0; i < c; i++)
                {
                    dna.portraitPool.Add(cul);
                    cul = GetRelatedCultureGfx(cul);
                }

            }

            String portrait = "";

            foreach (var p in dna.portraitPool)
            {
                portrait += p + " ";
            }
            int r = Rand.Next(255);
            int g = Rand.Next(255);
            int b = Rand.Next(255);

            r = Group.r;
            g = Group.g;
            b = Group.b;
            switch (Rand.Next(3))
            {
                case 0:
                    r += Rand.Next(-45, 45);
                    g += Rand.Next(-25, 25);
                    b += Rand.Next(-15, 15);

                    break;
                case 1:
                    g += Rand.Next(-45, 45);
                    r += Rand.Next(-25, 25);
                    b += Rand.Next(-15, 15);

                    break;
                case 2:
                    b += Rand.Next(-45, 45);
                    g += Rand.Next(-25, 25);
                    r += Rand.Next(-15, 15);

                    break;
            }
            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;

            if (r < 0)
                r = 0;
            if (g < 0)
                g = 0;
            if (b < 0)
                b = 0;

            this.r = r;
            this.g = g;
            this.b = b;

            ScopeCultureDetails();

        }
      
        public int r = 0;
        public int g = 0;
        public int b = 0;
        public void ScopeCultureDetails()
        {
            List<string> maleNameBlockSet;//= new List<string>();
            List<string> femaleNameBlockSet;//= new List<string>();

                 maleNameBlockSet = dna.maleNameBlockSet;
                femaleNameBlockSet = dna.femaleNameBlockSet;

            Scope.Clear();
            var portrait = dna.portraitPool[0];
            Scope.Do(@"
            
               color = { " + (r) + " " + (g) + " " + (b) + @" }

               graphical_cultures = { 
                    " + portrait + @" 
                }
		
		        male_names = {
			        " + String.Join(" ", maleNameBlockSet.ToArray()) + @"
		        }
		        female_names = {
			        " + String.Join(" ", femaleNameBlockSet.ToArray()) + @"
		        }
		
		        dukes_called_kings =  " + (dna.dukes_called_kings ? "yes" : "no") + @"
		        baron_titles_hidden =  " + (dna.baron_titles_hidden ? "yes" : "no") + @"
		        count_titles_hidden =  " + (dna.count_titles_hidden ? "yes" : "no") + @"
		        horde = " + (dna.horde ? "yes" : "no") + @"
                founder_named_dynasties = " + (dna.founder_named_dynasties ? "yes" : "no") + @"
                dynasty_title_names = " + (dna.dynasty_title_names ? "yes" : "no") + @"

		        from_dynasty_prefix = [" + '"' + dna.from_dynasty_prefix + '"' + @"
		     
		        male_patronym = " + (dna.male_patronym) + @"
		        female_patronym =  " + (dna.female_patronym) + @"
		        prefix =  " + (dna.patronym_prefix ? "yes" : "no") + @" # The patronym is added as a suffix
		        # Chance of male children being named after their paternal or maternal grandfather, or their father. Sum must not exceed 100.
		        pat_grf_name_chance = 25
		        mat_grf_name_chance = 0
		        father_name_chance = 25
		
		        # Chance of female children being named after their paternal or maternal grandmother, or their mother. Sum must not exceed 100.
		        pat_grm_name_chance = 10
		        mat_grm_name_chance = 25
		        mother_name_chance = 25

		        modifier = default_culture_modifier
		
		        allow_looting =  " + (dna.allow_looting ? "yes" : "no") + @"
		        seafarer =  " + (dna.seafarer ? "yes" : "no") + @"
        ");
        }

        public string GetRelatedCultureGfx(string cul)
        {
            if (wh.Contains(cul))
            {
                return wh[Rand.Next(wh.Count)];
            }
            else if (!wh.Contains(cul))
            {
                return bl[Rand.Next(bl.Count)];
            }

            return null;
        }

        public List<String> male_names = new List<string>();
        public List<String> female_names = new List<string>();

        public enum gfxStyles
        {
            norsegfx,
            germangfx,
            frankishgfx,
            westerngfx,
            saxongfx,
            italiangfx,
            southerngfx,
            occitangfx,
            easterngfx,
            byzantinegfx,
            easternslavicgfx,
            westernslavicgfx,
            celticgfx,
            ugricgfx,
            turkishgfx,
            mongolgfx,
            muslimgfx,
            persiangfx,
            cumangfx,
            arabicgfx,
            andalusiangfx,
            africangfx,
            mesoamericangfx,
            indiangfx

        }

        public static String[] gfx =
        {
            "norsegfx", "germangfx", "frankishgfx", "westerngfx", "saxongfx", "italiangfx", "southerngfx", "occitangfx",
            "easterngfx", "byzantinegfx", "easternslavicgfx", "westernslavicgfx",
            "celticgfx", "ugricgfx", "turkishgfx", "mongolgfx", "muslimgfx", "persiangfx", "cumangfx", "arabicgfx",
            "andalusiangfx", "africangfx", "mesoamericangfx", "indiangfx"
        };

        private static List<String> wh = new List<string>()
        {
            "norsegfx",
            "germangfx",
            "frankishgfx",
            "westerngfx",
            "saxongfx",
            "italiangfx",
            "southerngfx",
            "occitangfx",
            "easterngfx",
            "byzantinegfx",
            "easternslavicgfx",
            "westernslavicgfx",
            "celticgfx"
        };

        private static List<String> bl = new List<string>()
        {
            "ugricgfx",
            "turkishgfx",
            "mongolgfx",
            "muslimgfx",
            "persiangfx",
            "cumangfx",
            "arabicgfx",
            "andalusiangfx",
            "africangfx",
            "mesoamericangfx",
            "indiangfx"
        };

        internal CultureGroupParser group;

        public Color color
        {
            get { return Color.FromArgb(255, r, g, b); }
            set
            {
                r = value.R;
                g = value.G;
                b = value.B;
            }
        }
        
        //   public String government = "tribal";


        internal static string GetRandomCultureGraphics(CultureGroupParser group = null)
        {
            if (group != null)
            {
                if (Rand.Next(3) == 0)
                {
                    switch (group.chosenGfx)
                    {
                        case "norsegfx":
                        case "germangfx":
                        case "frankishgfx":
                        case "westerngfx":
                        case "saxongfx":
                        case "italiangfx":
                        case "celticgfx":
                        case "mongolgfx":
                            return wh[Rand.Next(gfx.Count())];
                            break;
                        case "ugricgfx":
                        case "turkishgfx":
                        case "muslimgfx":
                        case "persiangfx":
                        case "cumangfx":
                        case "arabicgfx":
                        case "andalusiangfx":
                        case "africangfx":
                        case "mesoamericangfx":
                        case "indiangfx":
                            return bl[Rand.Next(gfx.Count())];
                            break;
                    }
                }
                else
                {
                    switch (group.chosenGfx)
                    {
                        case "norsegfx":
                        case "germangfx":
                        case "frankishgfx":
                        case "westerngfx":
                        case "saxongfx":
                        case "italiangfx":
                        case "celticgfx":
                        case "mongolgfx":
                            return bl[Rand.Next(gfx.Count())];
                            break;
                        case "ugricgfx":
                        case "turkishgfx":
                        case "muslimgfx":
                        case "persiangfx":
                        case "cumangfx":
                        case "arabicgfx":
                        case "andalusiangfx":
                        case "africangfx":
                        case "mesoamericangfx":
                        case "indiangfx":
                            return wh[Rand.Next(gfx.Count())];
                            break;
                    }
                }
            }

            return gfx[Rand.Next(gfx.Count())];
        }

        public String PickCharacterName()
        {
            return dna.GetMaleName();
        }

        public String PickCharacterName(bool isFemale)
        {
            String str = "";
            do
            {
                str = DoPickCharacterName(isFemale);
            } while (str.Trim().Length <= 1);

            return str;
        }

        public String DoPickCharacterName(bool isFemale)
        {
            if (isFemale)
                return dna.GetFemaleName();
            return dna.GetMaleName();
        }

        public void RemoveProvince(ProvinceParser provinceParser)
        {
            Group.RemoveProvince(provinceParser);

            Provinces.Remove(provinceParser);
            dirty = true;
        }

        public void AddProvince(ProvinceParser provinceParser)
        {
            if (provinceParser.Culture != null)
            {
                provinceParser.Culture.RemoveProvince(provinceParser);
            }
            if(!Group.Provinces.Contains(provinceParser))
                Group.AddProvince(provinceParser);

            if (!Provinces.Contains(provinceParser))
                Provinces.Add(provinceParser);

            dirty = true;
        }

        public void AddProvinces(List<ProvinceParser> instanceSelectedProvinces)
        {
            foreach (var provinceParser in instanceSelectedProvinces)
            {
                AddProvince(provinceParser);
                provinceParser.Culture = this;
            }
        }

        public List<ProvinceParser> Provinces = new List<ProvinceParser>();
        private Point _textPos;
    }

    class CultureManager
    {
        public static CultureManager instance = new CultureManager();

        private Script script;
        public List<CultureParser> AllCultures = new List<CultureParser>();
        public Dictionary<String, CultureParser> CultureMap = new Dictionary<String, CultureParser>();
        public Dictionary<String, CultureGroupParser> CultureGroupMap = new Dictionary<String, CultureGroupParser>();
        public List<CultureGroupParser> AllCultureGroups = new List<CultureGroupParser>();


        public Script Script
        {
            get { return script; }
            set { script = value; }
        }

        public Dictionary<String, CultureGroupParser> GroupMap = new Dictionary<string, CultureGroupParser>();
        private ProvinceParser distanceTest;

        public CultureGroupParser AddCultureGroup(string name, CultureGroupParser group = null)
        {
            ScriptScope scope = new ScriptScope();
            scope.Name = name;
            script.Root.Add(scope);

            CultureGroupParser r = new CultureGroupParser(scope);
            r.Name = scope.Name;
            r.Init();
            if (group != null)
            {
                r.chosenGfx = GetRelatedCultureGfx(group);
            }
            GroupMap[name] = r;
            AllCultureGroups.Add(r);
            AllCultureGroups = AllCultureGroups.Distinct().ToList();
            r.color = Color.FromArgb(255, Rand.Next(255), Rand.Next(255), Rand.Next(255));
            r.chosenGfx = scope.Scopes[0].Data;
            return r;
        }

        private string GetRelatedCultureGfx(CultureGroupParser group)
        {
            return CultureParser.GetRandomCultureGraphics(group);

        }

        public void Save()
        {
            if(SaveCultures)
            script.Save();
        }

        public void Init()
        {
            Script s = new Script();
            script = s;
            s.Name = Globals.ModDir + "common\\cultures\\00_cultures.txt";
            s.Root = new ScriptScope();
            CultureGroupParser r = AddCultureGroup("barbarian");

            AllCultureGroups.Add(r);
            CultureGroupMap["barbarian"] = r;
            var cul = r.AddCulture("norse");
            r.Name = "barbarian";
            cul.dna = CulturalDnaManager.instance.GetNewFromVanillaCulture();
            cul.dna.horde = false;

            cul.DoDetailsForCulture();
            LanguageManager.instance.Add("barbarian", cul.dna.GetPlaceName());

            cul.Name = cul.Scope.Name;
            CultureMap[cul.Scope.Name] = cul;

            AllCultures.Add(cul);


        }

        public CultureParser BranchCulture(string Culture)
        {
            var rel = this.CultureMap[Culture];
            var group = rel.Group;

            var naa = rel.dna.GetPlaceName();
            while (GroupMap.ContainsKey(StarNames.SafeName(naa)))
            {
                naa = rel.dna.GetPlaceName();
            }

            CultureParser rel2 = null;

            if (!allowMultiCultureGroups)
            {
                var na = rel.dna.GetPlaceName();
                while (GroupMap.ContainsKey(StarNames.SafeName(na)))
                {
                    na = rel.dna.GetPlaceName();
                }

                LanguageManager.instance.Add(StarNames.SafeName(na), na);
                
                var group2 = AddCultureGroup(StarNames.SafeName(na), group);
                group2.Name = StarNames.SafeName(na);
                rel2 = group2.AddCulture(naa);
                group2.AddCulture(rel2);

                rel2.Init();
                rel2.dna = rel.dna.Mutate(16, rel);
                rel2.dna.DoRandom();
                CultureGroupMap[group2.Name] = group2;
            

            }
            else
            {
                rel2 = group.AddCulture(naa);
                rel2.Init();
                rel2.dna = rel.dna.MutateSmall(4);

            }

            rel2.DoDetailsForCulture();

            return rel2;
        }

        public void CalculateCulturesProper()
        {
            foreach (var cultureGroupParser in AllCultureGroups)
            {
                if (cultureGroupParser.Name == "norse")
                    continue;
                if (cultureGroupParser.Provinces.Count == 0)
                    continue;

                var province = cultureGroupParser.Provinces[Rand.Next(cultureGroupParser.Provinces.Count)];

                List<ProvinceParser> target = new List<ProvinceParser>();
                target.Add(province);
                target.AddRange(province.Adjacent.Where(p => p.land == true && p.title != null));

                for (int x = 0; x < 8; x++)
                {
                    var toAdd = new List<ProvinceParser>();
                    target.ForEach(p => toAdd.AddRange(p.Adjacent.Where(pp => pp.land && pp.title != null && !target.Contains(pp))));
                    target.AddRange(toAdd);
                }
                HashSet<ProvinceParser> toDo = new HashSet<ProvinceParser>(target);
                foreach (var provinceParser in toDo)
                {
                    provinceParser.Culture = cultureGroupParser.Cultures[0];
                    if (provinceParser.Culture.Group.ReligionGroup != null)
                        provinceParser.Religion = provinceParser.Culture.Group.ReligionGroup.Religions[0];
                    else
                        provinceParser.Religion = ReligionManager.instance.AllReligions[0];
                }

            }

            for (int index = 0; index < AllCultureGroups.Count; index++)
            {
                var cultureGroupParser = AllCultureGroups[index];

                if (cultureGroupParser.Provinces.Count < 20)
                {
                    bool possible = true;
                    while (cultureGroupParser.Provinces.Count > 0 && possible)
                    {
                        for (int i = 0; i < cultureGroupParser.Provinces.Count; i++)
                        {
                            var provinceParser = cultureGroupParser.Provinces[i];
                            var difcul =
                                provinceParser.Adjacent.Where(
                                    p => p.Culture != provinceParser.Culture && p.Culture != null);
                            if (!difcul.Any())
                            {
                                if (i == cultureGroupParser.Provinces.Count - 1)
                                    possible = false;
                                continue;
                            }
                            var list = new List<ProvinceParser>(difcul);
                            provinceParser.Culture = list[Rand.Next(list.Count)].Culture;
                            provinceParser.Religion = list[Rand.Next(list.Count)].Religion;
                            break;
                        }

                    }
                }

                if (cultureGroupParser.Provinces.Count == 0)
                {
                    AllCultureGroups.Remove(cultureGroupParser);
                    Script.Root.Remove(cultureGroupParser.Scope);
                    CultureMap.Remove(cultureGroupParser.Cultures[0].Name);
                    AllCultures.Remove(cultureGroupParser.Cultures[0]);
                    foreach (var characterParser in CharacterManager.instance.Characters)
                    {
                        if (characterParser.culture == cultureGroupParser.Cultures[0].Name)
                        {
                            characterParser.culture = AllCultures[AllCultures.Count-1].Name;
                        }
                    }

                    foreach (var value in DynastyManager.instance.DynastyMap.Values)
                    {
                        if ((string)(value.Scope.Children[1] as ScriptCommand).Value == cultureGroupParser.Cultures[0].Name)
                        {
                            (value.Scope.Children[1] as ScriptCommand).Value = AllCultures[AllCultures.Count - 1].Name;
                        }
                    }
                    index--;
                }


            }
            allowMultiCultureGroups = true;
            for (int index = 0; index < AllCultureGroups.Count; index++)
            {
                var cultureGroupParser = AllCultureGroups[index];

                var provinces = new List<ProvinceParser>(cultureGroupParser.Provinces);
                // Now do the same for cultures...

                var mainCulture = cultureGroupParser.Cultures[0];

                int size = cultureGroupParser.Provinces.Count;
                if (size <= 4)
                    size = 2;
                else if (size < 12)
                    size = 4;
                else if (size < 24)
                    size = 5;
                else if (size < 32)
                    size = 6;
                else if (size < 40)
                    size = 7;
                else
                    size = 8;

                for (int c = 0; c < size; c++)
                {
                    if (provinces.Count == 0)
                        break;
                    var start = provinces[Rand.Next(provinces.Count)];

                    if (!CultureManager.instance.CultureMap.ContainsKey(mainCulture.Name))
                    {
                        mainCulture = cultureGroupParser.Cultures[cultureGroupParser.Cultures.Count-1];
                    }
                    if (!CultureMap.ContainsKey(mainCulture.Name))
                    {
                        CultureMap[mainCulture.Name] = mainCulture;
                    }

                    start.Culture = BranchCulture(mainCulture.Name);
                    var newC = start.Culture;
                    List<ProvinceParser> target = new List<ProvinceParser>();
                    target.Add(start);
                    target.AddRange(start.Adjacent.Where(p => provinces.Contains(p)));
                    int s = 1;
                    if (size > 8)
                        s = 2;
                    if (size > 15)
                        s = 3;

                    for (int x = 0; x < s; x++)
                    {
                        var toAdd = new List<ProvinceParser>();
                        target.ForEach(p => toAdd.AddRange(p.Adjacent.Where(pp => pp.land && pp.title != null)));
                        target.AddRange(toAdd);
                    }
                    HashSet<ProvinceParser> toDo = new HashSet<ProvinceParser>(target);
                    foreach (var provinceParser in toDo)
                    {
                        provinceParser.Culture = newC;
                        provinces.Remove(provinceParser);
                    }

                }
            }
            // Create big religion groups covering multiple culture groups

            foreach (var religionGroupParser in ReligionManager.instance.AllReligionGroups)
            {
                var cgenum = AllCultureGroups.Where(cg => cg.ReligionGroup == religionGroupParser);

                var cultureGroupList = new List<CultureGroupParser>(cgenum);

                int n = Rand.Next(5)+4;

                for (int x = 0; x < n; x++)
                {
                    var adjacentProv = new List<ProvinceParser>();
                    var adjacent = new HashSet<CultureGroupParser>();
                    cultureGroupList.ForEach(g=> g.Provinces.ForEach(p => adjacentProv.AddRange(p.Adjacent.Where(pa=>pa.land && pa.title != null && pa.Culture != null && pa.Culture.Group != g))));
                    adjacentProv.ForEach(p=> adjacent.Add(p.Culture.Group));

                    if (adjacent.Count > 0)
                    {
                        List<CultureGroupParser> list = new List<CultureGroupParser>(adjacent);

                        var chosen = list[Rand.Next(list.Count)];
                        chosen.ReligionGroup = religionGroupParser;
                        chosen.Provinces.ForEach(p=>p.Religion = religionGroupParser.Religions[0]);
                    }
                }
            }

            // Cut out small ones


            // Now find the biggest two and make them bigger...
            ReligionGroupParser biggest = null;
            ReligionGroupParser second = null;
      

            for (int index = 0; index < ReligionManager.instance.AllReligionGroups.Count; index++)
            {
                var religionGroupParser = ReligionManager.instance.AllReligionGroups[index];

                
                if (religionGroupParser.Provinces.Count < 50)
                {
                    while (religionGroupParser.Provinces.Count > 0)
                    {
                        bool possible = true;
                        while (religionGroupParser.Provinces.Count > 0 && possible)
                        {
                            for (int i = 0; i < religionGroupParser.Provinces.Count; i++)
                            {
                                var provinceParser = religionGroupParser.Provinces[i];
                                var difcul =
                                    provinceParser.Adjacent.Where(
                                        p => p.Religion != provinceParser.Religion && p.Religion != null);
                                if (!difcul.Any())
                                {
                                    if (i == religionGroupParser.Provinces.Count - 1)
                                        possible = false;
                                    continue;
                                }
                                var list = new List<ProvinceParser>(difcul);
                                provinceParser.Religion = list[Rand.Next(list.Count)].Religion;
                                break;
                            }


                        }

                        if (!possible)
                        {
                            var provinceParser = religionGroupParser.Provinces[0];

                            var list =
                                MapManager.instance.Provinces.Where(
                                    p => p.land && p.title != null && p.Religion.Group != religionGroupParser).ToList();

                            distanceTest = provinceParser;
                            list.Sort(SortByDistance);

                            provinceParser.Religion = list[0].Religion;
                            provinceParser.Culture.Group.ReligionGroup = list[0].Religion.Group;
                        }
                    }


               
                }
                
                if (religionGroupParser.Provinces.Count == 0)
                {
                    ReligionManager.instance.AllReligionGroups.Remove(religionGroupParser);
                    System.Console.Out.WriteLine(religionGroupParser.Religions[0].Name + " removed");
              
                    religionGroupParser.Scope.Remove(religionGroupParser.Religions[0].Scope);
                    ReligionManager.instance.Script.Root.Remove(religionGroupParser.Scope);
                    ReligionManager.instance.ReligionMap.Remove(religionGroupParser.Religions[0].Name);
                    ReligionManager.instance.AllReligions.Remove(religionGroupParser.Religions[0]);

                    foreach (var characterParser in CharacterManager.instance.Characters)
                    {
                        if (characterParser.religion == religionGroupParser.Religions[0].Name)
                        {
                            characterParser.religion = ReligionManager.instance.AllReligions[ReligionManager.instance.AllReligions.Count-1].Name;
                        }
                    }
                    index--;
                }


            }

            ReligionManager.instance.AllReligionGroups.Sort(ReligionManager.instance.SortByBelievers);

            biggest = ReligionManager.instance.AllReligionGroups[0];
            if (ReligionManager.instance.AllReligionGroups.Count > 1)
            {
                second = ReligionManager.instance.AllReligionGroups[1];

            }

            for (int index = 0; index < ReligionManager.instance.AllReligionGroups.Count; index++)
            {
                var religionGroup = ReligionManager.instance.AllReligionGroups[index];

                var provinces = new List<ProvinceParser>(religionGroup.Provinces);
                // Now do the same for cultures...

                var mainReligion = religionGroup.Religions[0];

                int size = religionGroup.Provinces.Count;
                if (size <= 4)
                    size = 1;
                else if (size < 12)
                    size = 1;
                else if (size < 32)
                    size = 2;
                else
                    size = 3;


                if (biggest == religionGroup || second == religionGroup)
                {
                    size = 3;
                }

                for (int c = 0; c < size; c++)
                {
                    if (provinces.Count == 0)
                        break;


                    var start = provinces[Rand.Next(provinces.Count)];

                    start.Religion = ReligionManager.instance.BranchReligion(mainReligion.Name, start.Culture.Name);
                    var newC = start.Religion;
                    List<ProvinceParser> target = new List<ProvinceParser>();
                    target.Add(start);
                    target.AddRange(start.Adjacent.Where(p => provinces.Contains(p)));
                    int s = 2;
                    if (size > 16)
                        s = 3;
                    if (size > 32)
                        s = 4;

                    if (biggest == religionGroup || second == religionGroup)
                    {
                        if (c <= 2)
                        {
                            s += 2;
                        }
                    }

                    for (int x = 0; x < s; x++)
                    {
                        var toAdd = new List<ProvinceParser>();
                        target.ForEach(p => toAdd.AddRange(p.Adjacent.Where(pp => pp.land && pp.title != null && pp.Religion.Group == start.Religion.Group)));
                        target.AddRange(toAdd);
                    }
                    HashSet<ProvinceParser> toDo = new HashSet<ProvinceParser>(target);
                    foreach (var provinceParser in toDo)
                    {
                        provinceParser.Religion = newC;
                        provinces.Remove(provinceParser);
                    }

                }
            }

            for (int index = 0; index < ReligionManager.instance.AllReligionGroups.Count; index++)
            {
                var religionParser = ReligionManager.instance.AllReligionGroups[index];
                if (religionParser.Provinces.Count == 0)
                {
                    ReligionManager.instance.AllReligionGroups.Remove(religionParser);
                    index--;
                    continue;
                }
                religionParser.TryFillHolySites();
            }

            for (int index = 0; index < ReligionManager.instance.AllReligions.Count; index++)
            {
                var religionParser = ReligionManager.instance.AllReligions[index];
                if (religionParser.Provinces.Count == 0)
                {
                    System.Console.Out.WriteLine(religionParser.Name + " removed");
                    if (religionParser.Scope.Name == "enuique")
                    {

                    }
           
                    ReligionManager.instance.AllReligions.Remove(religionParser);
                    ReligionManager.instance.ReligionMap.Remove(religionParser.Name);
                    religionParser.Group.Scope.Remove(religionParser.Scope);
                    index--;
                    continue;
                }
                religionParser.TryFillHolySites();
            }


            foreach (var characterParser in CharacterManager.instance.Characters)
            {
                if (characterParser.Titles.Count > 0)
                {
                    if (characterParser.PrimaryTitle.Rank == 2)
                    {
                        characterParser.culture = characterParser.PrimaryTitle.SubTitles.First().Value.Owns[0].Culture.Name;
                        characterParser.religion = characterParser.PrimaryTitle.SubTitles.First().Value.Owns[0].Religion.Name;
                    }
                    else
                    {
                        characterParser.culture = characterParser.PrimaryTitle.Owns[0].Culture.Name;
                        characterParser.religion = characterParser.PrimaryTitle.Owns[0].Religion.Name;

                    }

                    foreach (var titleParser in characterParser.Titles)
                    {
                        titleParser.culture = characterParser.culture;
                    }
                }
              
            }

            var l = MapManager.instance.Provinces.Where(p => p.title != null).ToList();

            foreach (var provinceParser in l)
            {
                provinceParser.initialReligion = provinceParser.Religion.Name;
                provinceParser.initialCulture = provinceParser.Culture.Name;
            }


            foreach (var religionParser in ReligionManager.instance.AllReligions)
            {
           
                if (religionParser.Provinces.Count > 0)
                    ReligionManager.instance.ReligionMap[religionParser.Name] = religionParser;
                if (religionParser.Provinces.Count > 0 && (religionParser.hasLeader || religionParser.autocephaly))
                {
                    religionParser.DoLeader(religionParser.Provinces[Rand.Next(religionParser.Provinces.Count)]);

                }
            }
        }

        private int SortByDistance(ProvinceParser x, ProvinceParser y)
        {
            float dist = x.DistanceTo(distanceTest);
            float dist2 = y.DistanceTo(distanceTest);

            if (dist < dist2)
                return -1;
            if (dist > dist2)
                return 1;

            return 0;
        }

        public bool allowMultiCultureGroups { get; set; }

        public void LoadVanilla()
        {
            SaveCultures = false;
            var files = ModManager.instance.GetFileKeys("common\\cultures");
            foreach (var file in files)
            {
                Script s = ScriptLoader.instance.LoadKey(file);
                foreach (var rootChild in s.Root.Children)
                {

                    CultureGroupParser p = new CultureGroupParser(rootChild as ScriptScope);

                    AllCultureGroups.Add(p);
                    CultureGroupMap[p.Name] = p;
                    foreach (var scopeChild in p.Scope.Children)
                    {
                        if (scopeChild is ScriptScope)
                        {
                            var sc = scopeChild as ScriptScope;

                            if (sc.Name == "graphical_cultures")
                            {
                                continue;
                            }

                            CultureParser r = new CultureParser(sc, p);
                            AllCultures.Add(r);
                            CultureMap[r.Name] = r;
                            p.Cultures.Add(r);
                            r.Group = p;
                            r.LanguageName = LanguageManager.instance.Get(r.Name);
                            CulturalDna dna = new CulturalDna();
                            foreach (var scope in r.Scope.Scopes)
                            {
                                if (scope.Name == "male_names" || scope.Name == "female_names")
                                {

                                    String[] male_names = scope.Data.Split(new[] { ' ', '_', '\t' });
                                    foreach (var maleName in male_names)
                                    {
                                        var mName = maleName.Trim();
                                        if (mName.Length > 0)
                                            dna.Cannibalize(mName);

                                    }

                                }

                            }

                            r.dna = dna;

                        }
                    }
                }
            }
         
        }

        public bool SaveCultures { get; set; } = true;
    }
}
