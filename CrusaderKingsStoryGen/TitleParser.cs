using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class TitleParser : Parser
    {
        public int Rank = 1;
        public Dictionary<String, TitleParser> SubTitles = new Dictionary<string, TitleParser>();
        public List<ScriptScope> titleScripts = new List<ScriptScope>();
        public void SetCapital(ProvinceParser cap)
        {
            if(Scope.Find("capital")!=null)
                Scope.Remove(Scope.Find("capital"));
            Scope.Add(new ScriptCommand() { Name = "capital", Value = cap.id });
           
        }
        public List<TitleParser> CurrentSubTitles = new List<TitleParser>();
        public float FinalTechLevel = 0.0f;

        public Rectangle Bounds
        {
            get
            {
                if (dirty)
                    _bounds = GetBounds();
                if(!_bounds.IsEmpty)
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
            var prov = GetAllProvinces();

            float avx = 0;
            float avy = 0;
            if (Rank == 1 && Holder != null && Holder.PrimaryTitle.Rank == 1)
            {
                
            }
            Rectangle tot = Rectangle.Empty;
            foreach (var provinceParser in prov)
            {
                int cx = provinceParser.Bounds.X + (provinceParser.Bounds.Width/2);
                int cy = provinceParser.Bounds.Y + (provinceParser.Bounds.Height/2);

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
            TextPos = new Point((int) avx, (int) avy);
            return tot;
        }

        public TitleParser Liege
        {
            get
            {
                if (_liege == this)
                    return null;
                return _liege;
            }
            private set
            {
                if (this.Name == "k_armenia")
                {

                }
                if (value != null && value.Holder != null && value.Holder.PrimaryTitle.Rank <= Rank)
                    return;
                if (value != null && value.Rank <= Rank)
                    return;
                if (value != null)
                {
                    if (value.Active == false)
                        return;
                }
                if (_liege != value)
                {

                    if (_liege != null)
                    {
                        _liege.dirty = true;

                        _liege.SubTitles.Remove(this.Name);
                    }

                }
                _liege = value;
                if (_liege != null)
                {
                    if (!_liege.SubTitles.ContainsKey(Name))
                    {
                        _liege.dirty = true;
                        _liege.SubTitles[Name] = this;
                    }

                }
                if (Liege != null && SimulationManager.instance.AutoValidateRealm && !bSkipValidate)                
                    Liege.ValidateRealm(new List<CharacterParser>(), TopmostTitle);
            }
        }

        private static bool bSkipValidate = false;
        public TitleParser LiegeDirect
        {
            get
            {
                if (_liege == this)
                    return null;
                return _liege;
            }
             set
            {
             
                if (value != null && value.Holder != null && value.Holder.PrimaryTitle.Rank <= Rank)
                    return;
                if (value != null && value.Rank <= Rank)
                    return;
            
                if (_liege != value)
                {

                    if (_liege != null)
                    {
                        _liege.dirty = true;

                        _liege.SubTitles.Remove(this.Name);
                    }

                }
                _liege = value;
                if (_liege != null)
                {
                    if (!_liege.SubTitles.ContainsKey(Name))
                    {
                        _liege.dirty = true;
                        _liege.SubTitles[Name] = this;
                    }

                }
                if (Liege != null && SimulationManager.instance.AutoValidateRealm)
                    Liege.ValidateRealm(new List<CharacterParser>(), TopmostTitle);
            }
        }

        public void ValidateRealm(List<CharacterParser> listOfCharacters, TitleParser expectedHead )
        {
            if (Holder != null)
            {
                listOfCharacters.Add(Holder);

               
                var list = Holder.Titles.ToList();
                foreach (var titleParser in list)
                {
    
                    if (titleParser.Rank < Holder.PrimaryTitle.Rank && titleParser != Holder.PrimaryTitle )
                    {
                        if (titleParser.Liege != null && titleParser.Liege.Holder != Holder)
                        {

                            if (titleParser.Liege != null && titleParser.Liege.Holder != Holder)
                            {
                                titleParser.Log("Setting to vassal of " + Holder.PrimaryTitle + " in Validate Realm");
                                titleParser.DoSetLiegeEvent(Holder.PrimaryTitle);
                                continue;
                            }


                            if (titleParser.HasLiegeInChain(Holder.PrimaryTitle.Liege) || Holder.PrimaryTitle.HasLiegeInChain(titleParser.Liege))
                            {
                                continue;
                            }
                            titleParser.DoSetLiegeEvent(Holder.PrimaryTitle);
                        }
                    }
                    else if (titleParser.Rank == Holder.PrimaryTitle.Rank && titleParser != Holder.PrimaryTitle && titleParser.Liege != Holder.PrimaryTitle.Liege)
                    {
                        if (titleParser.Liege != null && titleParser.Liege.Holder != Holder)
                        {

                            if (titleParser.Liege != null)
                            {
                                if (titleParser.Name == "d_holland")
                                {
                                    
                                }
                                titleParser.Log("Setting independent in Validate Realm");
                                titleParser.DoSetLiegeEvent(null);
                            
                            }

                        }
                    }
                }
            }

            foreach (var titleParser in SubTitles.ToArray())
            {
                titleParser.Value.ValidateRealm(new List<CharacterParser>(), TopmostTitle);
            }
        }

        internal void Log(string text)
        {
            if (Rank >= 3 || Liege == null)
            {
                EventLogger.instance.AddTitle(this.Name);
            }
            EventLogger.instance.Log(Name, text);

            if (Liege != null)
            {
                EventLogger.instance.Log(Liege.Name, "vassal: " + Name + " - " + text);
                EventLogger.instance.Log(TopmostTitle.Name, "sub vassal: " + Name + " - " + text);
            }
        }

        public TitleParser ConquerLiege { get; set; }

        public Government Government { get; set; }

        public bool rebel { get; set; }
        public bool landless { get; set; }
        public bool primary { get; set; }
        public string culture { get; set; }
        public bool tribe { get; set; }
        public Color color { get; set; }
        public Color color2 { get; set; }
        public int capital { get; set; }
        public int dignity { get; set; }
        public bool Active { get; set; }
        public List<ProvinceParser> Owns = new List<ProvinceParser>();
        private CharacterParser _holder;
        public List<TitleParser> AdjacentToTitle = new List<TitleParser>();
        public HashSet<TitleParser> AdjacentToTitleSet = new HashSet<TitleParser>();
        private TitleParser _liege;

        public void RenameForCulture(CultureParser culture)
        {
            String namem = null;
            do
            {

                var name = culture.dna.GetPlaceName();
                namem = name;
             
            } while (Rename(namem));

            if (Rank == 1 && Owns.Count > 0)
            {
                Owns[0].Rename(namem);
                Owns[0].title = Name;
            }
            //Rename(namem);
        }
        public bool RenameSafe(String name, bool fromProvince = false)
        {
            String oldName = Name;
            LanguageManager.instance.Remove(Name);
            TitleManager.instance.TitleMap.Remove(oldName);
            this.Name = StarNames.SafeName(name);
          

            bool was = TitleManager.instance.TitleMap.ContainsKey(Name);
            TitleManager.instance.TitleMap[Name] = this;

            if (TitleManager.instance.TieredTitles.ContainsKey(oldName))
            {
                TitleManager.instance.TieredTitles.Remove(oldName);
                TitleManager.instance.TieredTitles[Name] = this;

                if (Liege != null)
                {
                    Liege.SubTitles.Remove(oldName);
                    Liege.SubTitles[Name] = this;

                }
            }

            Scope.Parent.ChildrenMap.Remove(oldName);
            Scope.Parent.ChildrenMap[Name] = this;


            return was;
        }
        public bool Rename(String name, bool fromProvince = false)
        {
            String oldName = Name;
            LanguageManager.instance.Remove(Name);
            TitleManager.instance.TitleMap.Remove(oldName);
            this.Name = StarNames.SafeName(name);
            if (Rank == 0)
                Name = "b_" + Name;
            if (Rank == 1)
                Name = "c_" + Name;
            if (Rank == 2)
                Name = "d_" + Name;
            if (Rank == 3)
                Name = "k_" + Name;
            if (Rank == 4)
                Name = "e_" + Name;

            LanguageManager.instance.Add(Name, name);
            bool was = TitleManager.instance.TitleMap.ContainsKey(Name);
            TitleManager.instance.TitleMap[Name] = this;

            if (TitleManager.instance.TieredTitles.ContainsKey(oldName))
            {
                TitleManager.instance.TieredTitles.Remove(oldName);
                TitleManager.instance.TieredTitles[Name] = this;

                if (Liege != null)
                {
                    Liege.SubTitles.Remove(oldName);
                    Liege.SubTitles[Name] = this;

                }
            }

            Scope.Parent.ChildrenMap.Remove(oldName);
            Scope.Parent.ChildrenMap[Name] = this;


            return was;
        }
        public void RenameSoft(String name, bool fromProvince = false)
        {
            String oldName = Name;
            LanguageManager.instance.Remove(Name);

            LanguageManager.instance.Add(Name, name);
        }


        public void AddSub(TitleParser sub)
        {
            if (sub.Rank >= this.Rank)
                return;
          //  if (SubTitles.ContainsKey(sub.Name))
           //     return;
            if (this == sub)
                return;

            var liege = sub;
            dirty = true;
            while (liege.Liege != null && liege.Liege.Rank > liege.Rank)
            {
                if (liege == this)
                {

                    return;
                }
                liege = liege.Liege;
                
            }
            if (sub.Liege != null)
            {
                sub.Liege.dirty = true;

                sub.Liege.SubTitles.Remove(sub.Name);
            }
            SubTitles[sub.Name] = sub;
            if(sub.Scope.Parent != null)
                sub.Scope.Parent.Remove(sub.Scope);
            else
                TitleManager.instance.LandedTitlesScript.Root.Remove(sub.Scope);
            Scope.SetChild(sub.Scope);
            sub.Liege = this;


            {

                ScriptScope thing = new ScriptScope();
                thing.Name = SimulationManager.instance.Year + ".2.1";

                thing.Add(new ScriptCommand() { Name = "liege", Value = Name });

                sub.titleScripts.Add(thing);

            }

            sub.Dejure = this;
        }
        public TitleParser(ScriptScope scope, bool addToList = true)
            : base(scope)
        {
            String newName = "";
            Name = scope.Name;
            if (Name == "k_armenia")
            {
                
            }
            if (Name == "d_vastergotland")
            {
                
            }
            if (Name.StartsWith("b_"))
            {
          //      newName = LanguageManager.instance.Add(Name, StarNames.Generate(culture)); 
                Rank = 0;
            }
            if (Name.StartsWith("c_"))
            {
           //     newName = LanguageManager.instance.Add(Name, StarNames.Generate(culture)); 
                Rank = 1;
            }
            if (Name.StartsWith("d_"))
            {
           //     newName = LanguageManager.instance.Add(Name, StarNames.Generate(culture)); 
                Rank = 2;
            }
            if (Name.StartsWith("k_"))
            {
             //   newName = LanguageManager.instance.Add(Name, StarNames.Generate(culture));
                Rank = 3;
            }
            if (Name.StartsWith("e_"))
            {
               // newName = LanguageManager.instance.Add(Name, StarNames.Generate(culture) + " Empire");
                Rank = 4;
            }

            color = scope.GetColor("color");

            if (addToList)
            {
                TitleManager.instance.TitleMap[Name] = this;
                TitleManager.instance.Titles.Add(this);

            }
            int line = 0;

            foreach (var child in scope.Children)
            {
                if (child is ScriptCommand)
                {
                    RegisterProperty(line, ((child as ScriptCommand).Name), child);
                    var com = (child as ScriptCommand);

                    if (com.Name == "capital" && MapManager.instance.IsVanilla)
                    {
                        capital = Convert.ToInt32(com.Value);
                        DejureCapitalProvince = MapManager.instance.ProvinceIDMap[capital];
                    }

                }
                line++;
                if (child is ScriptScope)
                {
                    var subscope = (child as ScriptScope);
                    if (subscope.Name == "c_narke")
                    {

                    }

                    if (subscope.Name == "OR" || subscope.Name == "NOT" || subscope.Name == "AND" ||
                        subscope.Name == "allow" ||
                        subscope.Name == "male_names" ||
                        subscope.Name == "coat_of_arms" ||
                        subscope.Name  == "pagan_coa" ||
                        subscope.Name == "layer")
                        continue;
                    SubTitles[subscope.Name] = new TitleParser(subscope, addToList);
                    SubTitles[subscope.Name].Dejure = this;
                    SubTitles[subscope.Name].LiegeDirect = this;
                    if (subscope.Name.StartsWith("b_"))
                    {
                        MapManager.instance.RegisterBarony(subscope.Name, SubTitles[subscope.Name]);
                     
                    }
                }
            }

            if (capital != 0)
            {
                if (MapManager.instance.ProvinceIDMap.ContainsKey(capital))
                {
                    ProvinceParser provinceParser = MapManager.instance.ProvinceIDMap[capital];
                    CapitalProvince = provinceParser;
                    if (Name.StartsWith("c_"))
                    {
                        Owns.Add(CapitalProvince);
                        CapitalProvince.title = this.Name;
                    }

                }


            }
            else if (MapManager.instance.ProvinceMap.ContainsKey(Name) && Rank == 1)
            {
                ProvinceParser provinceParser = MapManager.instance.ProvinceMap[Name];
                CapitalProvince = provinceParser;
                if (!Name.StartsWith("d_"))
                {
                    Owns.Add(CapitalProvince);
                    CapitalProvince.title = this.Name;
                }

            }
        }

        public ProvinceParser DejureCapitalProvince { get; set; }

        public void DoCapital()
        {
           
        }

        public override string ToString()
        {
            return Name;
        }
        
        public ProvinceParser CapitalProvince
        {
            get
            {
                if (_capitalProvince == null && !SimulationManager.instance.AllowCapitalPicking)
                {
                    return null;
                }
                if (_capitalProvince == null && GetAllProvincesTitleOnly().Count > 0)
                {
                    if (!PickNewCapital())
                    {
                        this.FlagProblem = true;
                        return _capitalProvince;
                    }    

                    
                }
                if (_capitalProvince != null && _capitalProvince.title != null && (_capitalProvince.Title == this || _capitalProvince.Title.HasLiegeInChain(this)))
                    return _capitalProvince;
                if (!PickNewCapital())
                {
                    this.FlagProblem = true;
                    PickNewCapital();
                }
                return _capitalProvince;
            }
            set { _capitalProvince = value; }
        }

        public bool FlagProblem { get; set; }

        public bool PickNewCapital()
        {
            var c = GetAllDirectProvinces().OrderBy(o=>o.TempleCount + o.TownCount + o.TownCount).Reverse().ToList();

            if (government == "republic")
            {
                c = c.Where(p => p.IsAdjacentToSea() && p.government == "republic").ToList();
            }

            if (c.Count == 0)
            {
                c = GetAllProvincesTitleOnly().Where(p=>p.republic == (government == "republic")).OrderBy(o => o.TempleCount + o.TownCount + o.CastleCount).Reverse().ToList();
                if (c.Count == 0)
                    return false;
                if (Holder != null)
                    Holder.GiveTitleSoft(c[0].Title);
               
            }

            if (c.Count == 0)
            {
                _capitalProvince = null;
                return false;
            }

            CapitalProvince = c[0];

            return true;
        }

        public class HeldDate
        {
            public int date;
            public int chrid;
        }
        public List<String> prevLeaderNames = new List<string>();

        public List<HeldDate> heldDates = new List<HeldDate>();
        public string government = "tribal";

        
        public CharacterParser Holder
        {
            get { return _holder; }
            set
            {

                if (_holder != null && _holder != value)
                {
                    _holder.Titles.Remove(this);
                }
                _holder = value;

                if (value != null)
                {
                    culture = _holder.culture;
                    foreach (var provinceParser in Owns)
                    {
                        Color col = value.Color;
                        SetProperty("color", col);
                        SetProperty("color2", col);
                    }
                    
                }
             
            }
        }

        public bool Claimed
        {
            get
            {
                var t = this;
                return !(!t.Active || t.Holder == null);
            }
        }

        public int LandedTitlesCount
        {
            get
            {
                int c = 0;
                if (Rank == 1)
                    return 1;

                foreach (var titleParser in SubTitles)
                {
                    c += titleParser.Value.LandedTitlesCount;
                }

                return c;
            }
        }
        public int DirectLandedTitlesCount
        {
            get
            {
                int c = 0;
                if (Rank == 1)
                    return 1;
            
                return c;
            }
        }
        public int DirectVassalLandedTitlesCount
        {
            get
            {
                int c = 0;
                if (Rank == 1)
                    return 1;

                foreach (var titleParser in SubTitles)
                {
                    c += titleParser.Value.DirectLandedTitlesCount;
                }

                return c;
            }
        }

        public TitleParser TopmostTitle
        {
            get
            {
                if (Holder != null)
                    return Holder.TopLiegeCharacter.PrimaryTitle;

                var liege = Liege;

                if (liege == null || liege.Holder == null)
                    return this;

                while (liege.Liege != null && liege.Liege.Holder != null && liege.Liege.Rank > liege.Rank)
                {
                    liege = liege.Liege;
                }

                return liege;
            }
        }
        public TitleParser TopmostTitleDirect
        {
            get
            {
              
                var liege = Liege;

                if (liege == null || liege.Holder == null)
                    return this;

                while (liege.Liege != null && liege.Liege.Holder != null && liege.Liege.Rank > liege.Rank)
                {
                    liege = liege.Liege;
                }

                return liege;
            }
        }

        public bool Religious { get; set; }
        public CharacterParser CurrentHolder { get; set; }

        public CultureParser Culture
        {
            get
            {
                if(CultureManager.instance.CultureMap.ContainsKey(culture))
                    return CultureManager.instance.CultureMap[culture];
                else
                {
                    return CultureManager.instance.CultureMap.OrderBy(o=> Rand.Next(1000000)).First().Value;
                }
            }
        }

        public TitleParser Dejure { get; set; }

        public int TotalTech
        {
            get
            {
                int tot = 0;
                if (Rank == 1)
                {
                    return Owns[0].cultureTechPoints;
                }

                foreach (var titleParser in SubTitles)
                {
                    if (titleParser.Value.Rank < Rank)
                    if (titleParser.Value.Rank > 1)
                        tot += titleParser.Value.TotalTech;
                        else if (titleParser.Value.Rank == 1)
                        {
                            tot += titleParser.Value.Owns[0].cultureTechPoints;
                        }


                }

                return tot;
            }
        }
        public int AverageTech
        {
            get
            {
                var p = GetAllProvinces();

                if (p.Count == 0)
                    return 0;

                int tot = 0;
                if (Rank == 1)
                {
                    if (Owns.Count == 0)
                    {
                        
                    }
                    return Owns[0].cultureTechPoints;
                }

                foreach (var titleParser in SubTitles)
                {
                    if (titleParser.Value.Rank > 1)
                        tot += titleParser.Value.TotalTech;
                    else if(titleParser.Value.Rank==1)
                    {
                        if (titleParser.Value.Owns.Count == 0)
                        {
                            continue;
                        }
                        tot += titleParser.Value.Owns[0].cultureTechPoints;
                    }


                }


                return tot / p.Count;
            }
        }

        public string GenderLaw { get; set; }
        public string Succession { get; set; }

        public String LangName
        {
            get
            {
                if (Holder != null && this.Culture.dna.dynasty_title_names)
                {
                    if (Holder.Dynasty.Name == null)
                        Holder.Dynasty.Name = Culture.dna.GetDynastyName();
                    return Holder.Dynasty.Name;
                }
                else
                {
                    if (LanguageManager.instance.Get(Name) == null)
                    {
                        foreach (var scopeChild in Scope.Children)
                        {
                            if (scopeChild is ScriptCommand)
                            {
                                var name = (scopeChild as ScriptCommand).Name;

                                if (name == "capital" ||
                                    name == "color" ||
                                    name == "color2" ||
                                    name == "religion" ||
                                    name == "culture" ||
                                    name == "holy_site" ||
                                    !((scopeChild as ScriptCommand).Value is String))
                                {

                                }
                                else
                                {
                                    return (scopeChild as ScriptCommand).Value.ToString();
                                }
                            }
                        }
                    }

                    if (LanguageManager.instance.Get(Name) == null || LanguageManager.instance.Get(Name) == "")
                    {

                        if (Name.StartsWith("c_"))

                        {
                            if (culture != null)
                            {
                                if (LanguageManager.instance.Get(Name.Replace("c_", "b_")) == null ||
                                    LanguageManager.instance.Get(Name.Replace("c_", "b_")) == "")
                                {
                            //        RenameForCulture(Culture);
                                }
                                else
                                {
                                    return LanguageManager.instance.Get(Name.Replace("c_", "b_"));
                                }
                            }
                            else
                            {
                                return Name;

                            }
                        }
                        else
                        {


                            if (culture != null)
                            {
                                if (LanguageManager.instance.Get(Name.Replace("c_", "b_")) == null ||
                                    LanguageManager.instance.Get(Name.Replace("c_", "b_")) == "")
                                {
                                    return Name;
                                }
                                else
                                {
                                    return LanguageManager.instance.Get(Name.Replace("c_", "b_"));
                                }
                            }


                        }
                    }




                    return LanguageManager.instance.Get(Name);
                }
            }
        }

        public String LangRealmName
        {
            
            get
            {
                if (LanguageManager.instance.Get(Name) == null)
                {
                    foreach (var scopeChild in Scope.Children)
                    {
                        if (scopeChild is ScriptCommand)
                        {
                            var name = (scopeChild as ScriptCommand).Name;

                            if (name == "capital" ||
                                name == "color" ||
                                name == "color2" ||
                                name == "religion" ||
                                name == "culture" ||
                                name == "holy_site" || 
                                !((scopeChild as ScriptCommand).Value is String))
                            {

                            }
                            else
                            {
                                return (scopeChild as ScriptCommand).Value.ToString();
                            }
                        }
                    }
                }

                if (LanguageManager.instance.Get(Name) == "")
                {
                    foreach (var scopeChild in Scope.Children)
                    {
                        if (scopeChild is ScriptCommand)
                        {
                            var name = (scopeChild as ScriptCommand).Name;

                            if (name == "capital" ||
                                name == "color" ||
                                name == "color2" ||
                                name == "religion" ||
                                name == "culture" ||
                                name == "holy_site" ||
                                !((scopeChild as ScriptCommand).Value is String))
                            {

                            }
                            else
                            {
                                return (scopeChild as ScriptCommand).Value.ToString();
                            }
                        }
                    }
                }

              
                switch (Rank)
                {
                    case 1:
                            return Owns[0].EditorName;
                    
                        break;
                    case 2:
                        return LangName;
                        break;
                    case 3:
                    
                        return LangName;
                        break;
                    case 4:
                      
                        return LangName;
                        break;
                }

                return LangName;
            }
        }

        public String LangTitleName
        {
            get
            {
             
                switch (Rank)
                {
                    case 1:
                     //   if (LanguageManager.instance.Get(Name) == null ||
                      //LanguageManager.instance.Get(Name) == "")
                     //       RenameForCulture(Culture);
                        if (LanguageManager.instance.Get(Name) == "")
                            return Owns[0].EditorName;
                        return LanguageManager.instance.Get(Culture.dna.countTitle) + " of " + LanguageManager.instance.Get(Name);
                        break;
                    case 2:
                    //    if (LanguageManager.instance.Get(Name) == null ||
                 //  LanguageManager.instance.Get(Name) == "")
                     //       RenameForCulture(Culture);
                        return LanguageManager.instance.Get(Culture.dna.dukeTitle) + " of " + LanguageManager.instance.Get(Name);
                        break;
                    case 3:
                    //    if (LanguageManager.instance.Get(Name) == null ||
                    // LanguageManager.instance.Get(Name) == "")
                     //       RenameForCulture(Culture);
                        return LanguageManager.instance.Get(Culture.dna.kingTitle) + " of " + LanguageManager.instance.Get(Name);
                        break;
                    case 4:
                     //   if (LanguageManager.instance.Get(Name) == null ||
                    //  LanguageManager.instance.Get(Name) == "")
                      //      RenameForCulture(Culture);
                        return LanguageManager.instance.Get(Culture.dna.empTitle) + " of " + LanguageManager.instance.Get(Name);
                        break;
                }

                return LanguageManager.instance.Get(Name);
            }
        }

        public ProvinceParser PalaceLocation { get; set; }
        public ScriptScope TechGroup { get; set; }
        public TitleParser StartLiege { get; set; }
        public Dictionary<string, TitleParser> DejureSub = new Dictionary<string, TitleParser>();

        public List<TitleParser> palaces = new List<TitleParser>();

        public List<int> republicdynasties = new List<int>();

        public List<TitleParser> Wars = new List<TitleParser>();

        public override ScriptScope CreateScope()
        {
            return null;
        }

        public void Remove()
        {
            Scope.Parent.Remove(Scope);
        }

        public bool SameRealm(TitleParser title)
        {          
            var liege = this;

            while (liege.Liege != null && liege.Rank < liege.Liege.Rank)
            {
                
                liege = liege.Liege;
            }
            var liege2 = title;

            while (liege2.Liege != null && liege2.Rank < liege2.Liege.Rank)
            {
                liege2 = liege2.Liege;
            }

            return liege == liege2;

        }
        
        public void AddChildProvinces(List<ProvinceParser> targets)
        {
      
            foreach (var subTitle in SubTitles)
            {
                if (subTitle.Value.Rank >= Rank)
                    continue;

                targets.AddRange(subTitle.Value.Owns);
                subTitle.Value.AddChildProvinces(targets);
            }
        }

        public bool Adjacent(TitleParser other)
        {

            return IsAdjacent(other);
        }

        private void AddAdj(TitleParser other)
        {
            if (AdjacentToTitleSet.Contains(other))
                return;

            AdjacentToTitleSet.Add(other);
            AdjacentToTitle.Add(other);
            other.AdjacentToTitleSet.Add(this);
            other.AdjacentToTitle.Add(this);

        }
        private void AddNotAdj(TitleParser other)
        {
            if (AdjacentToTitleSet.Contains(other))
                return;

            AdjacentToTitleSet.Add(other);
            other.AdjacentToTitleSet.Add(this);
          
        }


        public void RemoveVassal(TitleParser titleParser)
        {
            this.SubTitles.Remove(titleParser.Name);
            if (titleParser.Liege == this)
                titleParser.Liege = null;

            Scope.Remove(titleParser.Scope);
        }

        public void AddVassals(ICollection<TitleParser> vassals)
        {
            var a = vassals.ToArray();
            foreach (var titleParser in a)
            {
                if (titleParser != null && titleParser.Rank >= Rank)
                    continue;
                SubTitles[titleParser.Name] = titleParser;
                titleParser.Liege = this;
                Scope.Add(titleParser.Scope);
            }
        }

        public TitleParser GetRandomLowRankLandedTitle()
        {
            List<TitleParser> choices = new List<TitleParser>();

            GetRandomLowRankLandedTitle(choices);
            if (choices.Count == 0)
                return null;
            return choices[Rand.Next(choices.Count)];
     
        }
        
        public void GetRandomLowRankLandedTitle(List<TitleParser> choices)
        {
            if(this.Owns.Count > 0 && this.Holder.PrimaryTitle.Rank==1)
                choices.Add(this);

            foreach (var titleParser in SubTitles)
            {
                titleParser.Value.GetRandomLowRankLandedTitle(choices);
            }

            if (choices.Count == 0)
                return;

        }

        public void SplitLands()
        {
          
            if (Rank == 2)
            {
                List<ProvinceParser> titles = GetAllProvinces();
                List<ProvinceParser> half = GetAdjacentGroup(titles, titles.Count / 2);
                List<TitleParser> tits = new List<TitleParser>();
                foreach (var provinceParser in half)
                {
                    tits.Add(TitleManager.instance.TitleMap[provinceParser.title]);
                }
              //  TitleManager.instance.PromoteNewRuler(TitleManager.instance.CreateDuke(tits));
            }
        }

        private List<ProvinceParser> GetAdjacentGroup(List<ProvinceParser> provinces, int preferedSize)
        {
            List<ProvinceParser> split = new List<ProvinceParser>();

            var start = provinces[Rand.Next(provinces.Count)];

            foreach (var provinceParser in provinces)
            {
                if(start.Adjacent.Contains(provinceParser))
                    split.Add(provinceParser);

                if (split.Count >= preferedSize)
                    break;
            }

            if (split.Count < preferedSize)
            {
                foreach (var provinceParser in provinces)
                {
                    var a = split.ToArray();
                    foreach (var chosen in a)
                    {
                        if (start.Adjacent.Contains(provinceParser))
                            split.Add(provinceParser);

                        if (split.Count >= preferedSize)
                            break; 
                    }
                 
                }

            }

            return split;
        }

        public List<ProvinceParser> GetAllDejureProvinces()
        {
            List<ProvinceParser> provinces = new List<ProvinceParser>();

            GetAllDejureProvinces(provinces);

            return provinces;
        }


        public List<ProvinceParser> GetAllProvinces()
        {

            if (Rank == 0)
                return new List<ProvinceParser>();
            if (Holder != null)
                return Holder.GetAllProvinces();

            List<ProvinceParser> provinces = new List<ProvinceParser>();

            GetAllProvinces(provinces);

            return provinces;
        }

        public List<ProvinceParser> GetAllProvincesTitleOnly()
        {

            if (Rank == 0)
                return new List<ProvinceParser>();

            List<ProvinceParser> provinces = new List<ProvinceParser>();

            GetAllProvinces(provinces);

            return provinces;
        }
        public List<ProvinceParser> GetAllDirectProvinces()
        {
            List<ProvinceParser> provinces = new List<ProvinceParser>();

            GetAllProvinces(provinces);

            provinces = provinces.Where(p => p.Title.Holder == Holder).ToList();

            return provinces;
        }

        public List<ProvinceParser> GetAllDejureProvinces(List<ProvinceParser> provinces)
        {

            if (Owns.Count > 0 && !provinces.Contains(Owns[0]))
                provinces.Add(Owns[0]);

            foreach (var subTitle in DejureSub)
            {
                if (subTitle.Value.Rank >= Rank)
                    continue;

                    subTitle.Value.GetAllDejureProvinces(provinces);
            }

       
            return provinces;
        }
        public List<ProvinceParser> GetAllProvinces(List<ProvinceParser> provinces)
        {

            if (Owns.Count > 0 && !provinces.Contains(Owns[0]))
                provinces.Add(Owns[0]);

            foreach (var subTitle in SubTitles)
            {
                if (subTitle.Value.Rank >= Rank)
                    continue;
                if (subTitle.Value.Rank == 0)
                    continue;
                if (subTitle.Value.Liege == this)
                    subTitle.Value.GetAllProvinces(provinces);
            }


            provinces = provinces.Where(a => Holder != null && a.Title.Holder.TopLiegeCharacter == Holder.TopLiegeCharacter).ToList();
            return provinces;
        }

        public void Kill()
        {
            if (this.Liege != null)
            {
                this.Liege.SubTitles.Remove(this.Name);
            }

            this.Scope.Parent.Remove(this.Scope);
        }

        public bool AnyHolder()
        {
            return Holder != null || CurrentHolder != null;
        }

        public void SetRealmReligion(string religion)
        {
            if (Holder != null)
            {
                Holder.religion = religion;
                Holder.UpdateCultural();
            }

            foreach (var titleParser in SubTitles)
            {
                titleParser.Value.SetRealmReligion(religion);
            }
        }

        public bool HasLiegeInChain(CharacterParser titleParser)
        {
            var c = this;

            c = Liege;

            while (c != null)
            {
                if (c.Holder == titleParser)
                    return true;

                c = c.Liege;
            }

            return false;
        }
        public bool HasCharacterInChain(CharacterParser titleParser)
        {
            var c = this;

            while (c != null)
            {
                if (c.Holder == titleParser)
                    return true;

                if (c.Holder == null)
                    c = c.Liege;
                else
                    c = c.Holder.PrimaryTitle.Liege;
            }

            return false;
        }
        public bool HasLiegeInChain(TitleParser titleParser)
        {
            var c = this;

            c = Liege;

            while (c != null)
            {
                if (c == titleParser)
                    return true;

                c = c.Liege;
            }

            return false;
        }
        public bool HasTitleInChain(TitleParser titleParser)
        {
            var c = this;

            while (c != null)
            {
                if (c == titleParser)
                    return true;

                c = c.Liege;
            }

            return false;
        }

        /*   public void DoSetLiegeEvent(TitleParser titleParser)
           {
               if (Liege == titleParser)
                   return;
               if (government == "republic" && titleParser != null)
                   return;

               if (titleParser != null && titleParser.Rank <= Rank)
                   return;

               if (titleParser != null && Holder != null && Holder.PrimaryTitle.Rank >= titleParser.Rank)
               {
                   return;
               }

               SimulationManager.instance.DebugTest(this);

               Liege = titleParser;

               {

                   ScriptScope thing = new ScriptScope();
                   thing.Name = SimulationManager.instance.Year + ".2.1";
                   if (titleParser == null)
                       thing.Add(new ScriptCommand() { Name = "liege", Value = 0 });
                   else
                       thing.Add(new ScriptCommand() { Name = "liege", Value = titleParser.Name });

                   titleScripts.Add(thing);

               }

               SimulationManager.instance.DebugTest(this);
           }*/
        public void DoSetLiegeEventDejure(TitleParser titleParser)
        {
            if (Liege == titleParser)
                return;
            if (government == "republic" && titleParser != null)
                return;

            if (titleParser != null)
            {

                if (titleParser != null && titleParser.Rank <= Rank)
                {
                    return;
                }

                if (Holder != null && Holder.PrimaryTitle.Rank >= titleParser.Rank)
                {
                    return;
                }
                var parent = TitleManager.instance.LandedTitlesScript.Root;

                if (Scope.Parent != null)
                {
                    Scope.Parent.Remove(Scope);
                    if (Liege != null)
                        Liege.SubTitles.Remove(this.Name);
                }
                else
                {
                    parent.Remove(Scope);
                }

                titleParser.Scope.SetChild(Scope);
                titleParser.SubTitles[titleParser.Name] = this;

                Liege = titleParser;

            }
            else
            {
                var parent = TitleManager.instance.LandedTitlesScript.Root;

                if (Scope.Parent != null)
                {
                    Scope.Parent.Remove(Scope);
                    if (Liege != null)
                        Liege.SubTitles.Remove(this.Name);
                }
                else
                {
                    parent.Remove(Scope);
                }

                parent.SetChild(Scope);

                Liege = null;

            }
            {

                ScriptScope thing = new ScriptScope();
                thing.Name = SimulationManager.instance.Year + ".2.1";
                if (titleParser == null)
                    thing.Add(new ScriptCommand() { Name = "liege", Value = 0 });
                else
                    thing.Add(new ScriptCommand() { Name = "liege", Value = titleParser.Name });

                titleScripts.Add(thing);

            }
        }
        public bool DoSetLiegeEvent(TitleParser titleParser, bool skipValidate = false)
        {
            
            if (titleParser != null && titleParser.Rank <= Rank)
                return false;

            if (Liege == titleParser)
                return false;
            if (government == "republic" && titleParser != null)
                return false;

            if (titleParser != null)
            {
            
                if (titleParser != null && titleParser.Rank <= Rank)
                {
                    return false;
                }

                if (Holder != null &&  Holder != titleParser.Holder && Holder.PrimaryTitle.Rank >= titleParser.Rank)
                {
                    return false;
                }
                /*   var parent = TitleManager.instance.LandedTitlesScript.Root;

                   if (Scope.Parent != null)
                   {
                       Scope.Parent.Remove(Scope);

                   }
                   else
                   {
                       parent.Remove(Scope);
                   }

                   titleParser.Scope.SetChild(Scope);
               */
                if (Liege != null)
                {                    
                    Liege.Log("Lost vassal: " + Name);
                }
                bSkipValidate = skipValidate;
                Liege = titleParser;
                bSkipValidate = false;
                if (Liege != null)
                {
                    Liege.Log("Gained vassal: " + Name);

                }


                SimulationManager.instance.DestroyedTitles.Remove(titleParser);
                SimulationManager.instance.DestroyedTitles.Remove(this);

            }
            else
            {
               
                    Log("Made independent");

                /*                var parent = TitleManager.instance.LandedTitlesScript.Root;

                                if (Scope.Parent != null)
                                {
                                    Scope.Parent.Remove(Scope);
                                }
                                else
                                {
                                    parent.Remove(Scope);
                                }

                                parent.SetChild(Scope);
                                */
                Liege = (null);
                SimulationManager.instance.DestroyedTitles.Remove(this);

            }
            {

                ScriptScope thing = new ScriptScope();
                thing.Name = SimulationManager.instance.Year + ".2.1";
                if (titleParser == null)
                    thing.Add(new ScriptCommand() { Name = "liege", Value = 0 });
                else
                    thing.Add(new ScriptCommand() { Name = "liege", Value = titleParser.Name });

                titleScripts.Add(thing);

            }

            return true;
        }

        public void SoftDestroy()
        {
            var list = this.SubTitles.Values.ToList();
            TitleParser lastLiege = null;
            if (Liege != null)
            {
                lastLiege = Liege;
                DoSetLiegeEvent(null);
            }

            this.Log("Title destroyed");
            SimulationManager.instance.DestroyedTitles.Add(this);
            if (Holder != null)
            {
                Holder.Titles.Remove(this);
                Holder = null;
            }
          
            foreach (var titleParser in list)
            {
                if (lastLiege == null)
                {
                    titleParser.Log("Setting independent due to liege destruction");
                    titleParser.DoSetLiegeEvent(null);

                }
                else
                {
                    titleParser.Log("Setting liege to this liege in SoftDestroy");
                    titleParser.DoSetLiegeEvent(lastLiege);

                }
            }
        }

        public int republicdynasty;
        public string republicreligion;
        private ProvinceParser _capitalProvince;
        public string religion;
        private Rectangle _bounds;
        private bool dirty = true;
        private Point _textPos;

        public void DoTechPointTick()
        {
            float mil = 0;
            float cul = 0;
            float eco = 0;
            if (TechnologyManager.instance.CentresOfTech.Count == 0)
            {
                int max = 1;
                 max = 2;
                for (int x = 0; x < max; x++)
                {
                    var count = TitleManager.instance.GetRandomCount();
                    bool doIt = true;
                    foreach (var provinceParser in TechnologyManager.instance.CentresOfTech)
                    {
                        if (provinceParser.DistanceTo(count) < 600)
                        {
                            doIt = false;
                        }
                    }
                    if (!doIt)
                    {
                        x--;
                        continue;
                    }
                    TechnologyManager.instance.CentresOfTech.Add(count);

                }
            }
            
            if (Liege == null)
            {
                if (Rank == 2)
                {
                    mil += 2;
                    cul += 2;
                    eco += 2;
                }
                if (Rank == 3)
                {
                    mil += 3;
                    cul += 3;
                    eco += 3;
                }
                if (Rank == 4)
                {
                    mil += 4;
                    cul += 4;
                    eco += 4;
                }
          
       

                var p = GetAllProvinces();
                foreach (var provinceParser in p)
                {
                    float cul2 = cul;
                    float eco2 = eco;
                    float mil2 = mil;
                    var cap = CapitalProvince;

               
                    {
                   
                        if (provinceParser.Title.Holder != null && provinceParser.Title.Holder.TopLiegeCharacter == provinceParser.Title.Holder)
                        {
                            cul2 += 1;
                            eco2 += 1;
                            mil2 += 4;
                        }
                    }

                    if (TopmostTitle.Holder == provinceParser.Title.Holder)
                    {
                        mil2 += 2;
                    }
                    if (provinceParser.Title.Holder.PrimaryTitle.Rank == 1)
                    {
                        mil2 -= 1;
                    }
                    if (provinceParser.Title.Holder.PrimaryTitle.Rank == 2)
                    {
                        mil2 += 4;
                    }
                    if (provinceParser.Title.Holder.PrimaryTitle.Rank == 3)
                    {
                        mil2 += 6;
                    }
                    if (provinceParser.Title.Holder.PrimaryTitle.Rank == 4)
                    {
                        mil2 += 8;
                    }

                    if (provinceParser.Title.TopmostTitle.Holder != null)
                    {
                        cap = provinceParser.Title.TopmostTitle.Holder.EffectiveCapitalProvince;
                    }
                    if (provinceParser == cap)
                    {
                        cul2 += 1;
                        eco2 += 1;
                        mil2 += 4;
                    }
                    if (provinceParser.Adjacent.Contains(cap))
                    {
                        cul2 += 1;
                        eco2 += 1;
                        mil2 += 1;

                    }

                    if (provinceParser.Title.Liege != null && provinceParser.Title.Liege.Holder != null)
                    {
                        cap = provinceParser.Title.Liege.Holder.EffectiveCapitalProvince;
                    }
                    if (provinceParser == cap)
                    {                      
                        mil2 += 2;
                    }

                    float closest = 1000000;
                
                    foreach (var parser in TechnologyManager.instance.CentresOfTech)
                    {
                        float dist = parser.DistanceTo(provinceParser);
                        if (dist > TitleManager.instance.maxDist)
                            TitleManager.instance.maxDist = dist;
                        if (dist < closest)
                            closest = dist;
                    }

                
                    float techAdd = closest/1500.0f;
                    if (techAdd > 1.0f)
                        techAdd = 1.0f;
                    if (techAdd < 0)
                        techAdd = 0;
                    techAdd = 1.0f - techAdd;
       
                   // if (TechnologyManager.instance.CentresOfTech.Contains(provinceParser))
                    {
                        cul2 += 20 * techAdd;
                        eco2 += 20 * techAdd;
                        mil2 += 7 * techAdd;

                    }

                    provinceParser.cultureTechPoints += (int)cul2;
                    provinceParser.economicTechPoints += (int)eco2;
                    provinceParser.militaryTechPoints += (int)mil2;
                    foreach (var parser in provinceParser.Adjacent)
                    {
                        //     if(Rand.Next(4)==0)
                        if (parser.militaryTechPoints < provinceParser.militaryTechPoints)
                        {
                            parser.militaryTechPoints++;
                        }
                        if (parser.cultureTechPoints < provinceParser.cultureTechPoints)
                        {
                            parser.cultureTechPoints++;
                        }
                        if (parser.economicTechPoints < provinceParser.economicTechPoints)
                        {
                            parser.economicTechPoints++;
                        }
                     
                        if (parser.cultureTechPoints < provinceParser.cultureTechPoints)
                        {
                            parser.cultureTechPoints++;
                        }
                        if (parser.economicTechPoints < provinceParser.economicTechPoints)
                        {
                            parser.economicTechPoints++;
                        }
            
                        if (parser.cultureTechPoints < provinceParser.cultureTechPoints)
                        {
                            parser.cultureTechPoints++;
                        }
                        if (parser.economicTechPoints < provinceParser.economicTechPoints)
                        {
                            parser.economicTechPoints++;
                        }
     
                    }
                }
            }
        }

        public bool IsAdjacent(TitleParser titleParser)
        {
            var a = titleParser.GetAllProvinces();
            var b = GetAllProvinces();

            foreach (var provinceParser in a)
            {
                foreach (var parser in b)
                {
                    if (parser.Adjacent.Contains(provinceParser))
                        return true;
                }
            }

            return false;
        }
        public bool IsAdjacentSea(TitleParser titleParser)
        {
            var a = titleParser.GetAllProvinces();
            var b = GetAllProvinces();

            foreach (var provinceParser in a)
            {
                foreach (var parsera in provinceParser.Adjacent)
                {
                    if (!parsera.land)
                    {
                        foreach (var parser in b)
                        {
                            if (parser.Adjacent.Contains(parsera))
                                return true;
                        }
                    }
                }
                foreach (var parser in b)
                {
                    if (parser.Adjacent.Contains(provinceParser))
                        return true;
                }
            }

            return false;
        }

        public List<ProvinceParser> OverseaProvinces()
        {
            var a = GetAllProvinces();
            List<ProvinceParser> oversea = new List<ProvinceParser>();
            foreach (var provinceParser in a)
            {
                foreach (var parser in provinceParser.Adjacent)
                {
                    if (!parser.land)
                    {
                        List<ProvinceParser> landAdjacent = new List<ProvinceParser>(); 
                        
                        landAdjacent = parser.GetAdjacentLand(6, landAdjacent);
                        oversea.AddRange(landAdjacent);



                    }
                }
            }
            oversea = oversea.Distinct().ToList();
            oversea.RemoveAll(b => a.Contains(b));

            return oversea;
        }

        public bool IsAdjacentSea2(TitleParser titleParser)
        {
            var a = titleParser.GetAllProvinces();
            var b = OverseaProvinces();

            foreach (var provinceParser in a)
            {

                foreach (var parser in b)
                {
                    if (provinceParser == (parser))
                        return true;
                }
            }

            return false;
        }
    
        public bool IsAdjacent(TitleParser titleParser, int num)
        {
            var a = titleParser.GetAllProvinces();
            var b = GetAllProvinces();
            int i = 0;
            foreach (var provinceParser in a)
            {
                foreach (var parser in b)
                {
                    if (parser.Adjacent.Contains(provinceParser))
                        i++;
                    if (i >= num)
                        return true;
                }
            }

            return false;
        }

        public bool HasGovernmentInChain(string type)
        {
            if (government == type)
                return true;

            if (Liege != null)
            {
                return Liege.HasGovernmentInChain(type);
            }

            return false;
        }

        public void SetRealmCulture(string s)
        {
            culture = s;
            foreach (var value in SubTitles.Values)
            {
                value.SetRealmCulture(culture);
            }
        }

        public TreeNode AddTreeNode(TreeView inspectTree, TreeNode parent=null)
        {
            if (this.Name.StartsWith("e_spain"))
            {
                
            }
          
            CultureParser c = null;
            if (Rank == 0)
                return null;
            else if (Rank == 1)
                c = Owns[0].Culture;
            else
            {
                c = FindBestCulture();
            }

            if (c == null)
                return null;
      
            String tit = "";
            switch (Rank)
            {
                case 0:
                    tit = LanguageManager.instance.Get(c.dna.baronTitle);
                    break;
                case 1:
                    tit = LanguageManager.instance.Get(c.dna.countTitle);
                    break;
                case 2:
                    tit = LanguageManager.instance.Get(c.dna.dukeTitle);
                    break;
                case 3:
                    tit = LanguageManager.instance.Get(c.dna.kingTitle);
                    break;
                case 4:
                    tit = LanguageManager.instance.Get(c.dna.empTitle);
                    break;
            }
            TreeNode res = null;
            if (parent != null)
            {
                if (Rank == 1)
                {
                    res = parent.Nodes.Add(Owns[0].EditorName + " " + "(" + tit + ")");
                }
                else 
                    res = parent.Nodes.Add(LanguageManager.instance.Get(Name) + " " + "("+tit+")");
                res.Tag = this;
                res.ImageIndex = Rank;
            }

            else
            {
                if (Rank == 1)
                {
                    res = inspectTree.Nodes.Add(Owns[0].EditorName + " " + "(" + tit + ")");
                }
                else res = inspectTree.Nodes.Add(LanguageManager.instance.Get(Name) + " " + "(" + tit + ")");
                res.Tag = this;
                res.ImageIndex = Rank;
            }
            foreach (var titleParser in SubTitles)
            {
                titleParser.Value.AddTreeNode(inspectTree, res);
            }
            return res;
        }

        private CultureParser FindBestCulture()
        {
            var provs = GetAllProvinces();

            var groups = provs.GroupBy(a => a.Culture);

            groups = groups.OrderBy(g => g.Count()).Reverse();
            if (groups.Count() == 0)
                return null;

            return groups.First().Key;
        }

        public void Capture(List<ProvinceParser> provinces)
        {
            List<TitleParser> titles = new List<TitleParser>();
            foreach (var provinceParser in provinces)
            {
                if (provinceParser.Title == null)
                    continue;

                if (provinceParser.Title.TopmostTitle == this)
                    continue;

                var title = provinceParser.Title;//provinceParser.Title.FindTopmostTitleContainingAllProvinces(provinces, Holder.PrimaryTitle);
                if (title.Rank < this.Rank && (title.Holder==null || title.Holder.PrimaryTitle.Rank < this.Rank))
                {
               //     if (!title.DoSetLiegeEvent(this))
                    {
                   //     this.Holder.GiveTitleSoft(title, true);
                        title.DoSetLiegeEvent(this, true);
                        titles.Add(title);
                    }
                }
                else
                {                    
                    if (title.Rank < this.Rank)
                    {
                        this.Holder.GiveTitleSoft(title, true, true, true);
                        title.DoSetLiegeEvent(this);
                        titles.Add(title);
                    }
                    else
                    {
                        this.Holder.GiveTitleSoft(title, true, true, true);
                        titles.Add(title);
                        title.DoSetLiegeEvent(null);
                     }
                }
            }

            titles = titles.Distinct().ToList();

            for (var index = 0; index < titles.Count; index++)
            {
                var titleParser = titles[index];
                if (titleParser.Dejure != null && (titleParser.Dejure.Holder == null || titleParser.Dejure.SubTitles.Count==0))
                {
                    int c = 0;
                    List<TitleParser> has = new List<TitleParser>();
                    foreach (var parser in titleParser.Dejure.DejureSub)
                    {
                        bool b = parser.Value.HasCharacterInChain(this.Holder);
                        c += b ? 1 : 0;
                        if (b)
                            has.Add(parser.Value);
                    }

                    float percent = c / (float) titleParser.Dejure.DejureSub.Count;

                    if (percent > 0.5f)
                    {                       
                        this.Holder.GiveTitleSoft(titleParser.Dejure, true, true);
                        titles.Add(titleParser.Dejure);
                        if(titleParser.Dejure.Rank < Rank)
                            titleParser.Dejure.DoSetLiegeEvent(this);
                        else
                            titleParser.Dejure.DoSetLiegeEvent(null);
                        foreach (var parser in has)
                        {
                            parser.DoSetLiegeEvent(titleParser.Dejure);
                        }
                    }
                }
            }

            TopmostTitle.ValidateRealm(new List<CharacterParser>(), TopmostTitle);
        }

        internal TitleParser FindTopmostTitleContainingAllProvinces(List<ProvinceParser> provinces, TitleParser except=null, bool dejure = true)
        {
            if (Owns.Count > 0)
            {
                if (provinces.Contains(Owns[0]))
                {
                    if (Liege != null && Liege != except)
                    {
                        var title = Liege.FindTopmostTitleContainingAllProvinces(provinces, except, dejure);

                        if (title == null)
                            return this;
                        return title;
                    }

                    return this;
                }
            }
            else
            {
                List<ProvinceParser> provinceParsers = null;

                if(dejure)
                    provinceParsers  = GetAllDejureProvinces();
                else
                    provinceParsers = GetAllProvinces();
                float percent = 0;
                float count = 0;
                foreach (var provinceParser in provinceParsers)
                {
                    if (provinces.Contains(provinceParser))
                        count++;
                }

                percent = count / provinceParsers.Count;
                if (provinceParsers.Count == 0)
                    percent = 0;
                if (percent <1f || except == this)
                    return null;

                if (Liege == null)
                    return this;

                TitleParser parent = Liege.FindTopmostTitleContainingAllProvinces(provinces, except, dejure);

                if (parent == null)
                    return this;

                return parent;
            }
            return null;
        }

        public string FindValidCulture()
        {
            if (culture != null)
                return culture;

            var prov = GetAllProvinces();

            var list = prov.Where(a => a.Culture != null).ToList();
            if (list.Count == 0)
                return CultureManager.instance.AllCultures[Rand.Next(CultureManager.instance.AllCultures.Count)].Name;

            return list[Rand.Next(list.Count)].Culture.Name;
        }
        public string FindValidReligion()
        {
            if (religion != null)
                return religion;

            var prov = GetAllProvinces();

            var list = prov.Where(a => a.Religion != null).ToList();
            if (list.Count == 0)
                return ReligionManager.instance.AllReligions[Rand.Next(ReligionManager.instance.AllReligions.Count)].Name;

            return list[Rand.Next(list.Count)].Religion.Name;
        }

        public CharacterParser GetRandomSubtitleHolder()
        {
            var choices = new List<CharacterParser>();
            foreach (var titleParser in SubTitles)
            {
                if(titleParser.Value.Holder != null && titleParser.Value.Holder.PrimaryTitle.Rank < Rank)
                    choices.Add(titleParser.Value.Holder);
            }

            if (choices.Count == 0)
                return null;
            return choices[Rand.Next(choices.Count)];
        }

        public void MakeIndependent()
        {
            if (Liege != null)
            {
                if (Holder != null)
                {
                    if (Holder.PrimaryTitle != this)
                    {
                        Holder.NonRelatedHeir.GiveTitleSoft(this);
                    }
                }
                else
                {
                    CharacterManager.instance.CreateNewCharacter(null, false,
                        SimulationManager.instance.Year - 16, religion, culture).GiveTitleSoft(this);
                }

                DoSetLiegeEvent(null);
            }
        }

        public bool GiveToNeighbour()
        {
            var prov = GetAllProvinces();
            var list = new List<ProvinceParser>();
            prov.ForEach(a => list.AddRange(a.Adjacent.Where(b => b.land && b.title != null && b.Title.TopmostTitle != this.TopmostTitle)));
            var titleChoices = new List<TitleParser>();
            foreach (var provinceParser in list)
            {
                titleChoices.Add(provinceParser.Title.Holder.TopLiegeCharacter.PrimaryTitle);
            }

            titleChoices = titleChoices.Distinct().ToList();

            if (titleChoices.Count > 0)
            {
                titleChoices = titleChoices.OrderBy(a => a.Rank).Reverse().ToList();
                var toGiveTo = titleChoices[0];
                if (Liege != null)
                {
                    if (toGiveTo.Rank <= Rank)
                    {
                        Log("Made independent due to being a seperate island prior to giving to neighbour");
                        DoSetLiegeEvent(null);

                    }
                    else
                    {
                        Log("Made vassal of neighbour (" + toGiveTo.Name+ ") due to being a seperate island");
                        DoSetLiegeEvent(toGiveTo);

                    }
                }

                Log("Given to neighbour holder of " + toGiveTo.Name);
                toGiveTo.Holder.GiveTitleSoft(this);
                
                return true;
            }

            return false;
        }

        public List<TitleParser> GetAllDuchies()
        {
            List<TitleParser> duchies = new List<TitleParser>();

            return GetAllDuchies(duchies);
        }

        private List<TitleParser> GetAllDuchies(List<TitleParser> duchies)
        {
            if(this.Rank == 2)
                duchies.Add(this);

            if (Rank > 2)
            {
                foreach (var subTitlesValue in DejureSub.Values)
                {
                    subTitlesValue.GetAllDuchies(duchies);
                }
            }

            return duchies;
        }

        public List<TitleParser> GetAllCounts()
        {
            List<TitleParser> counts = new List<TitleParser>();

            return GetAllCounts();

        }
        private List<TitleParser> GetAllCounts(List<TitleParser> counts)
        {
            if (this.Rank == 1)
                counts.Add(this);

            if (Rank > 1)
            {
                foreach (var subTitlesValue in DejureSub.Values)
                {
                    subTitlesValue.GetAllCounts(counts);
                }
            }

            return counts;
        }
    }
}
