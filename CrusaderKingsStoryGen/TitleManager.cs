using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class TitleManager : ISerializeXml
    {
        public static TitleManager instance = new TitleManager();

        public Dictionary<string, TitleParser> TieredTitles = new Dictionary<string, TitleParser>();
        public Dictionary<string, TitleParser> TitleMap = new Dictionary<string, TitleParser>();
        public List<TitleParser> Titles = new List<TitleParser>();
        public Script LandedTitlesScript { get; set; }
        public List<TitleParser> ReligiousTitles = new List<TitleParser>();


        public void LoadVanilla()
        {

            LandedTitlesScript = ScriptLoader.instance.Load(
                Globals.GameDir + "common\\landed_titles\\landed_titles.txt");

            foreach (var child in LandedTitlesScript.Root.Children)
            {
             
                DoVanillaTitle(child as ScriptScope, null);

                // do this proper
                //   Titles.Add(empire);
            }
            foreach (var titleParser in Titles)
            {
                titleParser.DejureSub = new Dictionary<string, TitleParser>(titleParser.SubTitles);
            }


        }

        private void DoVanillaTitle(ScriptScope scope, TitleParser liege)
        {

            if (scope.Name == "gain_effect")
                return;

            var empire = new TitleParser(scope as ScriptScope, true);
            
            if (empire.Name == "e_rebels")
            {
                Titles.Remove(empire);
                return;
            }
            if (empire.Name == "e_pirates")
            {
                Titles.Remove(empire);
                return;
            }

          
        }

        public void Load()
        {
            LandedTitlesScript = ScriptLoader.instance.Load(
             Globals.GameDir + "common\\landed_titles\\landed_titles.txt");

            LandedTitlesScript.Root.Clear();
            /*
            foreach (var child in LandedTitlesScript.Root.Children)
            {
                var empire = new TitleParser(child as ScriptScope);
                TieredTitles[empire.Name] = empire;
                TitleMap[empire.Name] = empire;
             //   Titles.Add(empire);
            }

            PromoteAllToTop();
            RemoveKingdomAndAbove();
       //     RemoveDukeAndAbove();
            Save();*/
        }

        private void RemoveDukeAndAbove()
        {
            var array = TieredTitles.Values.ToArray();
        }

     

        List<TitleParser> toTop = new List<TitleParser>(); 
        private void PromoteAllToTop()
        {
            toTop.Clear();
            var array = TieredTitles.Values.ToArray();

            foreach (var titleParser in array)
            {
                foreach (var subTitle in titleParser.SubTitles)
                {
                    if (subTitle.Value.Rank == 0)
                        continue;
                    PromoteToTop(subTitle.Value);
                }

            }

            foreach (var titleParser in toTop)
            {
                LandedTitlesScript.Root.Add(titleParser.Scope);
                titleParser.Scope.Parent = LandedTitlesScript.Root;
                TieredTitles[titleParser.Name] = titleParser;
            }
        }
        private bool PromoteToTop(TitleParser title)
        {
            var array = title.SubTitles.ToArray();
            if (title.Rank == 0)
                return false;
            foreach (var titleParser in array)
            {
                if(PromoteToTop(titleParser.Value))
                    titleParser.Value.SubTitles.Clear();
            }

            title.Scope.Parent.Remove(title.Scope);            
            title.SubTitles.Clear();
            toTop.Add(title);
            return true;
        }

        public void Save()
        {
            LandedTitlesScript.Root.Strip(new string[]
            {
                "layer",
                "pagan_coa"
            });

            CreateMercs();

            LandedTitlesScript.Save();
        }

        public float maxDist = 0;

        private static String[] compositions = new[]
        {
            "muslim_turkic_company_composition",
            "bedouin_company_composition",
            "berber_company_composition",
            "muslim_cuman_company_composition",
            "zun_warriors_composition",
            "bulls_rishabha_composition",
            "white_company_composition",
            "great_company_composition",
            "company_of_st_george_composition",
            "star_company_composition",
            "little_hat_company_composition",
            "rose_company_composition",
            "catalan_company_composition",
            "navarrese_company_composition",
            "swiss_company_composition",
            "breton_company_composition",
           "victual_brothers_composition",
           "varangian_guard_composition",
           "cuman_company_composition",
           "rus_company_composition",
           "pecheneg_company_composition",
           "bulgarian_company_composition",
           "lombard_band_composition",
           "breton_band_composition",
           "catalan_band_composition",
           "saxon_band_composition",
           "cuman_band_composition",
           "rus_band_composition",
           "finnish_band_composition",
           "lithuanian_band_composition",
           "abyssinian_band_composition",
           "nubian_band_composition",
           "scottish_band_composition",
           "irish_band_composition",
           "alan_band_composition",
          "pecheneg_band_composition",
          "bulgarian_band_composition",
          "turkic_band_composition",
          "mamluks_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
          "naval_merc_composition",
       
        };
        public List<string> mercs = new List<string>();
        public void CreateMercs()
        {
            // do mercs...
            foreach (var cultureParser in CultureManager.instance.AllCultures)
            {
                String name = "";
                String namesafe = null;

                do
                {
                    name = cultureParser.dna.GetMaleName();
                    namesafe = "d_" + StarNames.SafeName(name);

                } while (TitleMap.ContainsKey(namesafe) || LanguageManager.instance.Get(namesafe)!="");
                
                LanguageManager.instance.Add(namesafe, name);
                String composition = compositions[Rand.Next(compositions.Length)];
                LandedTitlesScript.Root.Do(@"

                    " + namesafe + @" = {

	                   	color = { 135 170 60 }
	                    color2 = { 255 255 255 }

	                    capital = " + MapManager.instance.GetInhabited().id + @" 
	
	                    # Parent Religion 
	                    culture =  " + cultureParser.Name + @" 
	
	                    mercenary = yes

	                    title = ""CAPTAIN""
	                    foa = ""CAPTAIN_FOA""

	                    # Always exists
	                    landless = yes
	
	                    # Cannot be held as a secondary title
	                    primary = yes
	
	                    # Cannot be vassalized
	                    independent = yes
	
	                    strength_growth_per_century = 1.00
	
	                    mercenary_type = " + composition + @"
                    }


                ");
            }
        }


        public CharacterParser PromoteNewRuler(TitleParser title)
        {
    
            {
                var chara = CharacterManager.instance.GetNewCharacter();

                chara.GiveTitle(title);
                SimulationManager.instance.characters.Add(chara);
                return chara;
            }


        }

        public TitleParser CreateDukeScriptScope(ProvinceParser capital, String name = null)
        {
            if (capital.title == "c_keysay")
            {
                
            }
            var rand = Rand.Get();
            Color col = Color.FromArgb(255, rand.Next(200) + 55, rand.Next(200) + 55, rand.Next(200) + 55);
            ScriptScope scope = new ScriptScope();
            scope.Parent = LandedTitlesScript.Root;

            if (name == null)
            {
                String place = capital.Title.Holder.Culture.dna.GetPlaceName();
                String text = place;
                place = StarNames.SafeName(place);


                if (TitleManager.instance.TitleMap.ContainsKey("d_" + place))
                {
                    return CreateDukeScriptScope(capital, name);
                }

                LanguageManager.instance.Add(place, text);
                scope.Name = "d_" + place;
                LanguageManager.instance.Add(scope.Name, text);
            }
            else
            {
                scope.Name = "d_" + name;
                if (TitleManager.instance.TitleMap.ContainsKey(scope.Name))
                    return CreateDukeScriptScope(capital);
            }

            //  scope.Kids.Add(new ScriptCommand() { Name = "rebel", Value = false });
            scope.Add(new ScriptCommand() { Name = "color", Value = col });
            scope.Add(new ScriptCommand() { Name = "color2", Value = col });
            
            scope.Add(new ScriptCommand() { Name = "capital", Value = capital.id });
          
            TitleParser title = new TitleParser(scope);
            
  //          if (capital.Title.Culture.dna.horde)
//                title.Scope.Do("historical_nomad = yes");
            
            AddTitle(title);
            if (title.capital != 0)
                title.CapitalProvince = MapManager.instance.ProvinceIDMap[title.capital];
            // now place the counties into it...

            return title;
        }

        public TitleParser CreateDukeScriptScope(ProvinceParser capital, CharacterParser chr)
        {
            var rand = Rand.Get();
            Color col = Color.FromArgb(255, rand.Next(200) + 55, rand.Next(200) + 55, rand.Next(200) + 55);
            ScriptScope scope = new ScriptScope();
            scope.Parent = LandedTitlesScript.Root;

            
            {
                String place = chr.Culture.dna.GetPlaceName();
                String text = place;
                place = StarNames.SafeName(place);


                if (TitleManager.instance.TitleMap.ContainsKey("d_" + place))
                {
                    return CreateDukeScriptScope(capital, chr);
                }

                LanguageManager.instance.Add(place, text);
                scope.Name = "d_" + place;
                LanguageManager.instance.Add(scope.Name, text);
            }
           
            //  scope.Kids.Add(new ScriptCommand() { Name = "rebel", Value = false });
            scope.Add(new ScriptCommand() { Name = "color", Value = col });
            scope.Add(new ScriptCommand() { Name = "color2", Value = col });

            scope.Add(new ScriptCommand() { Name = "capital", Value = capital.id });

            TitleParser title = new TitleParser(scope);
  //          if (chr.Culture.dna.horde)
//                title.Scope.Do("historical_nomad = yes");
            AddTitle(title);
            if (title.capital != 0)
                title.CapitalProvince = MapManager.instance.ProvinceIDMap[title.capital];
            // now place the counties into it...

            return title;
        }
        public TitleParser CreateBaronyScriptScope(ProvinceParser capital, CultureParser culture)
        {
            var place = culture.dna.GetPlaceName();
            var text = place;
            place = StarNames.SafeName(text);            

            if (TitleManager.instance.TitleMap.ContainsKey("b_" + place))
            {
                return CreateBaronyScriptScope(capital, culture);
            }
            var rand = Rand.Get();
            Color col = Color.FromArgb(255, rand.Next(200) + 55, rand.Next(200) + 55, rand.Next(200) + 55);
            ScriptScope scope = new ScriptScope();
            if(capital != null)
                scope.Parent = capital.Title.Scope;
          

                scope.Name = "b_" + place;
                LanguageManager.instance.Add("b_" + place, text);
            //  scope.Kids.Add(new ScriptCommand() { Name = "rebel", Value = false });
         
            TitleParser title = new TitleParser(scope);
            TieredTitles[title.Name] = title;
            if(capital != null)
                capital.Title.Scope.Add(title.Scope);
            else
                LandedTitlesScript.Root.Add(title.Scope);

  //          if (culture.dna.horde)
//                title.Scope.Do("historical_nomad = yes");
         //   AddTitle(title);
         
            return title;
        }
        public TitleParser CreateEmpireScriptScope(ProvinceParser capital, String name = null)
        {
            var rand = Rand.Get();
            Color col = Color.FromArgb(255, rand.Next(200) + 55, rand.Next(200) + 55, rand.Next(200) + 55);
            ScriptScope scope = new ScriptScope();
            scope.Parent = LandedTitlesScript.Root;

            if (name == null)
                scope.Name = "e_" + capital.Title.Name.Substring(2);
            else
                scope.Name = "e_" + name;

            //  scope.Kids.Add(new ScriptCommand() { Name = "rebel", Value = false });
            scope.Add(new ScriptCommand() { Name = "color", Value = col });
            scope.Add(new ScriptCommand() { Name = "color2", Value = col });

            scope.Add(new ScriptCommand() { Name = "capital", Value = capital.id });

            TitleParser title = new TitleParser(scope);
            AddTitle(title);
            if (title.capital != 0)
                title.CapitalProvince = MapManager.instance.ProvinceIDMap[title.capital];
            // now place the counties into it...

            return title;
        }
        public TitleParser CreateKingScriptScope(ProvinceParser capital, CharacterParser chr)
        {
            var rand = Rand.Get();
            Color col = Color.FromArgb(255, rand.Next(200) + 55, rand.Next(200) + 55, rand.Next(200) + 55);
            ScriptScope scope = new ScriptScope();
            scope.Parent = LandedTitlesScript.Root;



            {
                String place = chr.Culture.dna.GetPlaceName();
                String text = place;
                place = StarNames.SafeName(place);

                if (TitleManager.instance.TitleMap.ContainsKey("k_" + place))
                {
                    return CreateKingScriptScope(capital, chr.culture);
                }

                LanguageManager.instance.Add(place, text);
                scope.Name = "k_" + place;
                LanguageManager.instance.Add(scope.Name, text);
            }


            //scope.Kids.Add(new ScriptCommand() { Name = "rebel", Value = false });
            scope.Add(new ScriptCommand() { Name = "color", Value = col });
            scope.Add(new ScriptCommand() { Name = "color2", Value = col });

            scope.Add(new ScriptCommand() { Name = "capital", Value = capital.id });

            TitleParser title = new TitleParser(scope);
            AddTitle(title);

            // now place the counties into it...
            if (title.capital != 0)
                title.CapitalProvince = MapManager.instance.ProvinceIDMap[title.capital];

            return title;
        }
        public TitleParser CreateEmpireScriptScope(ProvinceParser capital, CharacterParser chr)
        {
            var rand = Rand.Get();
            Color col = Color.FromArgb(255, rand.Next(200) + 55, rand.Next(200) + 55, rand.Next(200) + 55);
            ScriptScope scope = new ScriptScope();
            scope.Parent = LandedTitlesScript.Root;



            {
                String place = chr.Culture.dna.GetPlaceName();
                String text = place;
                place = StarNames.SafeName(place);
                LanguageManager.instance.Add(place, text);
                scope.Name = "e_" + place;
                LanguageManager.instance.Add(scope.Name, text);
            }


            //scope.Kids.Add(new ScriptCommand() { Name = "rebel", Value = false });
            scope.Add(new ScriptCommand() { Name = "color", Value = col });
            scope.Add(new ScriptCommand() { Name = "color2", Value = col });

            scope.Add(new ScriptCommand() { Name = "capital", Value = capital.id });

            TitleParser title = new TitleParser(scope);
            AddTitle(title);

            // now place the counties into it...
            if (title.capital != 0)
                title.CapitalProvince = MapManager.instance.ProvinceIDMap[title.capital];

            return title;
        }
        
        public TitleParser CreateKingScriptScope(ProvinceParser capital, String name = null)
        {
            var rand = Rand.Get();
            Color col = Color.FromArgb(255, rand.Next(200) + 55, rand.Next(200) + 55, rand.Next(200) + 55);
            ScriptScope scope = new ScriptScope();
            scope.Parent = LandedTitlesScript.Root;


            if (name == null)
                scope.Name = "k_" + capital.Title.Name.Substring(2);
            else
                scope.Name = "k_" + name;


            //scope.Kids.Add(new ScriptCommand() { Name = "rebel", Value = false });
            scope.Add(new ScriptCommand() { Name = "color", Value = col });
            scope.Add(new ScriptCommand() { Name = "color2", Value = col });
        
            scope.Add(new ScriptCommand() { Name = "capital", Value = capital.id });

            TitleParser title = new TitleParser(scope);
            AddTitle(title);

            // now place the counties into it...
            if (title.capital != 0)
                title.CapitalProvince = MapManager.instance.ProvinceIDMap[title.capital];

            return title;
        }

       
     
        public void AddTitle(TitleParser title)
        {
            if(!Titles.Contains(title))
                Titles.Add(title);
            TieredTitles[title.Name] = title;
            LandedTitlesScript.Root.Add(title.Scope);
            title.Scope.Parent = LandedTitlesScript.Root;

            title.color = Color.FromArgb(255, Rand.Next(255), Rand.Next(255), Rand.Next(255));
            title.SetProperty("color", title.color);
            title.SetProperty("color2", title.color);
        }
        public HashSet<int> dynastiesWithPalaces = new HashSet<int>(); 
        public void SaveTitles()
        {

            foreach (var titleParser in Titles)
            {
                if (titleParser.Name == "b_sasaka")
                {
                    int gfdg = 0;
                }
              
                if (titleParser.culture == null)
                {
                    if (titleParser.Dejure != null)
                    {
                        titleParser.culture = titleParser.Dejure.culture;          
                    }
                    else
                        titleParser.culture = "norse";
                }                
                if(titleParser.Rank > 1 && titleParser.Scope.Find("capital")==null)
                {
                    
                }
                var p = titleParser.GetAllProvinces();
                bool bFound = false;
                foreach (var provinceParser in p)
                {
                    if (provinceParser.id == titleParser.capital)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound && p.Count > 0)
                {
                    var ca = p[Rand.Next(p.Count)];
                    titleParser.capital = ca.id;
                    var cap = (titleParser.Scope.Find("capital") as ScriptCommand);
                    if (cap == null)
                    {
                        titleParser.Scope.Add(new ScriptCommand("capital", ca.id, titleParser.Scope));
                    }
                    else
                        cap.Value = ca.id;
                    titleParser.CapitalProvince = ca;
                    if(titleParser.Holder!=null)
                        titleParser.Holder.GiveTitleSoftPlusIntermediate(titleParser.CapitalProvince.Title);

                }
                int h = 0;
                foreach (var child in titleParser.Scope.Children)
                {
                    ScriptScope scope = child as ScriptScope;

                }

                String tit = titleParser.Culture.dna.kingTitle;
           
                switch (titleParser.Rank)
                {
                    case 0:
                        tit = titleParser.Culture.dna.baronTitle;
                        break;
                    case 1:
                        tit = titleParser.Culture.dna.countTitle;
                        break;
                    case 2:
                        tit = titleParser.Culture.dna.dukeTitle;
                        break;
                    case 3:
                        tit = titleParser.Culture.dna.kingTitle;
                        break;
                    case 4:
                        tit = titleParser.Culture.dna.empTitle;
                        break;
                }

                if(titleParser.Rank > 0)
                if(!titleParser.Scope.HasNamed("culture"))
                titleParser.Scope.Add(new ScriptCommand() { Name = "culture", Value = titleParser.culture });

                if (tit == null)
                {
                    
                }
                if(tit != null && tit.Trim().Length > 0)
                if (!titleParser.Scope.HasNamed("title_female") && !titleParser.Scope.HasNamed("title"))
                    titleParser.Scope.Do(
                    @"
                        title=" + tit + @"
                        title_female=" + tit + @"
"
                    );
            }

           

            if (!Directory.Exists(Globals.ModDir + "history\\titles\\"))
                Directory.CreateDirectory(Globals.ModDir + "history\\titles\\");
            var files = Directory.GetFiles(Globals.ModDir + "history\\titles\\");
            foreach (var file in files)
            {
                File.Delete(file);
            }
            for (int index = 0; index < Titles.Count; index++)
            {

                var title = Titles[index];
                if (title.Name == "b_sasaka")
                {
                  
                }
     
                if (!title.Active && title.Rank > 0)
                    continue;


                if (title.Religious)
                {
                }
                Script titleScript =
                    ScriptLoader.instance.Load(Globals.GameDir + "history\\titles\\" + title.Name + ".txt");
                titleScript.Root.Clear();
             /*   ScriptScope thing = new ScriptScope();
                thing.Name = "1.1.1";
                titleScript.Root.SetChild(thing);*/

                /*
                  if (title.government == "republic")
                  {
                      thing.Add(new ScriptCommand() {Name = "law", Value = "succ_patrician_elective"});
                      // if (title.Liege != null && title.Rank == 0 && title.TopmostTitle.government == "republic")
                      {
                          // if (title.Liege.Owns[0].baronies[0].title == title.Name)
                          {
                              var name = title.Culture.dna.GetPlaceName();
                              var sname = StarNames.SafeName(name);
                              while (LanguageManager.instance.Get(sname) != null)
                              {
                                  name = title.Culture.dna.GetPlaceName();
                                  sname = StarNames.SafeName(name);
                              }
                              if (!dynastiesWithPalaces.Contains(title.Holder.Dynasty.ID))
                              {
                                  var barony = TitleManager.instance.CreateBaronyScriptScope(null, title.Culture);
                                  Titles.Add(barony);
                                  barony.government = "republicpalace";
                                  barony.republicdynasty = title.Holder.Dynasty.ID;
                                  barony.culture = title.culture;
                                  barony.republicreligion = title.Holder.religion;
                                  barony.Liege = title;
                                  title.Holder.GiveTitleSoft(barony);
                                  dynastiesWithPalaces.Add(title.Holder.Dynasty.ID);

                              }
                          }
                      }
                  }
                  */
                if (title.Liege != null && title.Liege.Rank > title.Rank)
                {
                //    thing.Add(new ScriptCommand() {Name = "liege", Value = title.Liege.Name});
                }
               /* if (title.Rank == 0 && title.government == "republicpalace")
                {

                    thing.Add(new ScriptCommand() { Name = "holding_dynasty", Value = title.republicdynasty });
                    //thing.Add(new ScriptCommand() { Name = "liege", Value = title.republicdynasty });
                    if (title.Scope.Find("culture") == null)
                    {
                        title.Scope.Add(new ScriptCommand() { Name = "culture", Value = title.culture });
                        title.Scope.Add(new ScriptCommand() { Name = "religion", Value = title.republicreligion });

                    }
                }*/

           //     titleScript.Root.SetChild(thing);

                foreach (var scriptScope in title.titleScripts)
                {
                    int v = 2;
                    if (!scriptScope.FromVanilla)
                    {
                        while (titleScript.Root.HasNamed(scriptScope.Name))
                        {
                            var indexOf = scriptScope.Name.LastIndexOf(".");//(v - 1));
                            scriptScope.Name = scriptScope.Name.Substring(0, indexOf) + "." + v;
                            v++;
                        }

                    }
                    titleScript.Root.SetChild(scriptScope);
                }
                /*    TitleParser lastLiege = null;
                foreach (var heldDate in title.heldDates)
                {
                    ScriptScope thing = new ScriptScope();
                    thing.Name = heldDate.date+".1.1";
                    titleScript.Root.SetChild(thing);
                    if (title.Culture.dna.horde)
                        thing.Add(new ScriptCommand() { Name = "historical_nomad", Value = true });

                    if (title.Liege != null && title.Liege.Rank > title.Rank && title.Liege != lastLiege)
                    {
                        thing.Add(new ScriptCommand() { Name = "liege", Value = title.Liege.Name });
                        lastLiege = title.Liege;
                    }

                  //  if (title.Holder != null)
                    {
                        thing.Add(new ScriptCommand() { Name = "holder", Value = heldDate.chrid });

                        //     title.Holder.MakeAlive();
                    }
                 
                }
                /*      {

                          {
                              ScriptScope thing = new ScriptScope();
                              thing.Name = "1.1.1";
                              titleScript.Root.SetChild(thing);
                              if (title.Culture.dna.horde)
                                  thing.Add(new ScriptCommand() { Name = "historical_nomad", Value = true });

                              if (title.Liege != null)
                              {
                                  thing.Add(new ScriptCommand() { Name = "liege", Value = title.Liege.Name });

                              }

                              if (title.Holder != null)
                              {
                                  thing.Add(new ScriptCommand() { Name = "holder", Value = title.Holder.ID });

                                  //     title.Holder.MakeAlive();
                              }
                              else if (title.SubTitles.Count > 0 && title.Rank >= 2 && title.Holder == null)
                              {

                                  thing.Add(new ScriptCommand() { Name = "holder", Value = title.SubTitles.Values.ToArray()[0].Holder.ID });
                                  //    title.SubTitles.Values.ToArray()[0].Holder.MakeAlive();
                              }
                              thing = new ScriptScope();
                              thing.Name = SimulationManager.instance.Year + ".1.1";
                              titleScript.Root.SetChild(thing);
                              if (title.CurrentHolder != null)
                              {
                                  thing.Add(new ScriptCommand() { Name = "holder", Value = title.CurrentHolder.ID });

                              }

         
            }


        }
         */

                titleScript.Save();

       
            }
        }

        public TitleParser CreateEmperor(ProvinceParser capital)
        {
            TitleParser s = CreateEmpireScriptScope(capital);

            return s;
        }

       

        public ProvinceParser GetRandomCount()
        {
            var list = new List<TitleParser>();
            foreach (var titleParser in Titles)
            {
                if(titleParser.Rank==1)
                    list.Add(titleParser);
            }

            return list[Rand.Next(list.Count)].CapitalProvince;
        }

        public void FixupFinal()
        {

            /*
            for (int i = 0; i < LandedTitlesScript.Root.Children.Count; i++)
            {
                var child = LandedTitlesScript.Root.Children[i];
                var scope = child as ScriptScope;

                if (scope.Name.StartsWith("c_"))
                {
                    if (TitleMap.ContainsKey(scope.Name.Replace("c_", "d_")))
                        continue;
                    var title = TitleMap[scope.Name];
            
                    LandedTitlesScript.Root.Remove(scope);
                    ScriptScope king = new ScriptScope(scope.Name.Replace("c_", "d_"));
                    LanguageManager.instance.Add(scope.Name.Replace("c_", "d_"),
                        LanguageManager.instance.Get(scope.Name));

                    king.SetChild(scope);

                    king.Add(new ScriptCommand() { Name = "color", Value = Color.FromArgb(255, 255, 255, 255) });
                    king.Add(new ScriptCommand() { Name = "color2", Value = Color.FromArgb(255, 255, 255, 255) });

                    king.Add(new ScriptCommand() { Name = "capital", Value = TitleMap[scope.Name].Owns[0].id });


                    //
                    var t = new TitleParser(king);
                    t.Active = true;
                    //      AddTitle(t);
                    title.Liege = t;
                    LandedTitlesScript.Root.SetChild(king);
                    i--;
                }
            }

            for (int i = 0; i < LandedTitlesScript.Root.Children.Count; i++)
            {
                var child = LandedTitlesScript.Root.Children[i];
                var scope = child as ScriptScope;

                if (scope.Name.StartsWith("d_") && scope.Find("controls_religion") == null &&
                    scope.Find("controls_religion") == null)
                {
                    if (TitleMap.ContainsKey(scope.Name.Replace("d_", "k_")))
                        continue;

                    var title = TitleMap[scope.Name];
                  
                    LandedTitlesScript.Root.Remove(scope);
                    ScriptScope king = new ScriptScope(scope.Name.Replace("d_", "k_"));
                    LanguageManager.instance.Add(scope.Name.Replace("d_", "k_"),
                        LanguageManager.instance.Get(scope.Name));

                    king.Add(new ScriptCommand() { Name = "color", Value = Color.FromArgb(255, 255, 255, 255) });
                    king.Add(new ScriptCommand() { Name = "color2", Value = Color.FromArgb(255, 255, 255, 255) });
                    king.Add(new ScriptCommand() { Name = "capital", Value = (scope.Find("capital") as ScriptCommand).Value });

                    king.SetChild(scope);
                    //    LandedTitlesScript.Root.SetChild(king);
                    var t = new TitleParser(king);
                    t.Active = true;
                    //     AddTitle(t);
                    title.Liege = t;
                    LandedTitlesScript.Root.SetChild(king);
                    i--;
                }
            }
            */



        }

        public void SaveProject(XmlWriter writer)
        {
          
            writer.WriteStartElement("titles");
            foreach (var titleParser in Titles)
            {
                writer.WriteStartElement("title");

                writer.WriteElementString("name", titleParser.Name);
                writer.WriteElementString("active", titleParser.Active.ToString());
                writer.WriteElementString("culture", titleParser.culture);
                writer.WriteElementString("religion", titleParser.religion);

                writer.WriteStartElement("color");
                writer.WriteElementString("r", titleParser.color.R.ToString());
                writer.WriteElementString("g", titleParser.color.G.ToString());
                writer.WriteElementString("b", titleParser.color.B.ToString());

                writer.WriteEndElement();

                writer.WriteStartElement("scripts");
                foreach (var titleParserTitleScript in titleParser.titleScripts)
                {
                    titleParserTitleScript.SaveXml(writer);
                }
                
                writer.WriteEndElement();

                writer.WriteEndElement();

            }
            writer.WriteEndElement();
            writer.WriteStartElement("titlehierarchy");
            foreach (var rootChild in LandedTitlesScript.Root.Children)
            {
                var scope = rootChild as ScriptScope;

                writer.WriteStartElement(scope.Name);

                scope.WriteHierarchy(writer);

                writer.WriteEndElement();

            }
            writer.WriteEndElement();
        }

        public void LoadProject(XmlReader reader)
        {
       
        }

        public void LoadToDate(int endDate)
        {
            for (var index = 0; index < Titles.Count; index++)
            {
                var titleParser = Titles[index];

                titleParser.Active = false;
                SimulationManager.instance.DestroyedTitles.Add(titleParser);
                if (titleParser.Holder != null)
                {
                    titleParser.Holder.Titles.Remove(titleParser);
                    titleParser.Holder = null;
                }
                var list = titleParser.SubTitles.Values.ToList();

         
            }
            for (var index = 0; index < Titles.Count; index++)
            {
                var titleParser = Titles[index];
                if (!ModManager.instance.FileMap.ContainsKey("history\\titles\\" + titleParser.Name + ".txt"))
                {
                    continue;
                }
                if (titleParser.Name == "c_bari")
                {

                }

                if (titleParser.Scope.HasNamed("capital"))
                {
                    int cap = titleParser.Scope.GetInt("capital");
                    titleParser.capital = cap;
                    titleParser.CapitalProvince = MapManager.instance.ProvinceIDMap[cap];
                }
                var script = ScriptLoader.instance.LoadKey("history\\titles\\" + titleParser.Name + ".txt");
                foreach (var rootChild in script.Root.Children)
                {
                    var scope = rootChild as ScriptScope;

                    int year = Convert.ToInt32(scope.Name.Split('.')[0]);
               
                    if (year <= endDate)
                    {
                        titleParser.titleScripts.Add(scope);
                        scope.FromVanilla = true;

                        foreach (var scopeChild in scope.Children)
                        {
                            var com = scopeChild as ScriptCommand;
                            if (com == null)
                                continue;

                            if (com.Name == "liege")
                            {
                                if (com.Value.ToString() != "0")
                                {
                                    if (com.Value.ToString() == "144999")
                                    {

                                    }
                                    if(com.Value.ToString()=="-")
                                    {
                                        titleParser.LiegeDirect = null;
                                    }
                                    else
                                    {
                                        titleParser.LiegeDirect = TitleMap[com.Value.ToString()];
                                    }
                                    
                                }
                                else
                                {
                         
                                    titleParser.LiegeDirect = null;
                                }

                            }
                            if (com.Name == "holder")
                            {
                                int i = com.GetInt();
                                if (i != 0)
                                {
                                    var chr = CharacterManager.instance.CharacterMap[com.GetInt()];
                                    if (chr.ID == 144999)
                                    {
                                        
                                    }
                                    chr.GiveTitleSoft(titleParser, true, false);
                                    SimulationManager.instance.DestroyedTitles.Remove(titleParser);
                                    titleParser.Active = true;
                                }
                                else
                                {
                                    titleParser.Holder = null;
                                    if(!SimulationManager.instance.DestroyedTitles.Contains(titleParser))
                                        SimulationManager.instance.DestroyedTitles.Add(titleParser);
                                    titleParser.Active = false;
                                }


                            }
                        }
                    }
                }
            }

            for (var index = 0; index < Titles.Count; index++)
            {
                var titleParser = Titles[index];
                if (titleParser.Name == "c_troyes")
                {

                }

                if (titleParser.Liege != null)
                {
                    if (!titleParser.Liege.Active || titleParser.Liege.Holder == null)
                    {
                        titleParser.LiegeDirect = null;
                    }
                }
            }
        }
    }
}
