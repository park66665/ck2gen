using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class TechnologyManager
    {
        public static TechnologyManager instance = new TechnologyManager();
        private Script script;
        public List<ProvinceParser> TechOrderedProvinceList = new List<ProvinceParser>();
        public List<TitleParser> TechOrderedTitleList = new List<TitleParser>();
        public List<TitleParser> TechOrderedDukeTitleList = new List<TitleParser>();
        public Dictionary<float, TechnologyGroup> Groups = new Dictionary<float, TechnologyGroup>();
        public List<ProvinceParser> CentresOfTech = new List<ProvinceParser>();
        private float minMilitary = 0f;
        private float minEconomy = 0f;
        private float minCulture = 0f;

        private float minDate = 200;
        private float maxDate = 1070;

        private float maxMilitary = 4f;
        private float maxEconomy = 4f;
        private float maxCulture = 4f;
        public Script Script
        {
            get { return script; }
            set { script = value; }
        }

        public int numMerchantRepublics = 0;
        public void Init()
        {
            Script = new Script();

            Script.Name = Globals.ModDir + "history\\technology\\techs.txt";
            Script.Root = new ScriptScope();
        }
        List<ReligionParser> religionsTheocracy = new List<ReligionParser>();


      
        private int useYear = 0;
        private int lastYear = -1;
        public void SaveOutTech(int year = -1)
        {
            // CJS
            return;
            if (year == -1)
            {
                year = SimulationManager.instance.Year-3;
                useYear = year;
            }
            else
            {
                useYear = year;
                if (lastYear == -1)
                {
                    SaveOutTech(useYear-2);
                }

            }

            lastYear = useYear;


            if (TechOrderedDukeTitleList.Count == 0)
            {
                foreach (var titleParser in TitleManager.instance.Titles)
                {
                    if (titleParser.Rank == 2)
                    {
                        TechOrderedDukeTitleList.Add(titleParser);
                    }
                    if (titleParser.Liege == null)
                        TechOrderedTitleList.Add(titleParser);
                }
                TechOrderedProvinceList.AddRange(MapManager.instance.Provinces);
                TechOrderedTitleList.Sort(SortDuchyByTotalTech);
                TechOrderedDukeTitleList.Sort(SortDuchyByTotalTech);

            }

            float dateDelta = (year - minDate) / (float)(maxDate - minDate);

            if (dateDelta < 0)
                dateDelta = 0;
            if (dateDelta > 1)
                dateDelta = 1;
            float lastDelta = 0;
            float orderDelta = 1.0f;
            int ToLower = TechOrderedDukeTitleList.Count;
            ToLower = (int)(ToLower * 0.005f);
            float rate = 1.0f - (GenerationOptions.TechSpreadRate / 5.0f);
            ToLower = (int)(ToLower * rate);
            int cccc = 0;
            float lowerAmount = 0;

            foreach (var titleParser in TechOrderedDukeTitleList)
            {

                float delSpeed = (5 - GenerationOptions.TechAdvanceRate) / 5.0f;
                delSpeed *= 2.0f;
                float mil = lerp(minMilitary, maxCulture * delSpeed, dateDelta - lowerAmount);
                float cul = lerp(minCulture, maxCulture * delSpeed, dateDelta - lowerAmount);
                float eco = lerp(minEconomy, maxCulture * delSpeed, dateDelta - lowerAmount);

                Add(titleParser, mil, cul, eco);

                cccc++;
                if (cccc >= ToLower)
                {
                    lowerAmount += 1.0f / 400.0f;
                    cccc = 0;
                }
            }
        }
        private void Add(TitleParser title, float mil, float cul, float eco)
        {
            String name = title.Name;
            int y = useYear;
            if (y > SimulationManager.instance.MaxYear)
                y = SimulationManager.instance.MaxYear;
            var group = title.TechGroup;
            if (title.TechGroup != null)
            {
                if (title.TechGroup.HasNamed(y.ToString()))
                    return;

                var date = new ScriptScope((y).ToString());
                date.Add(new ScriptCommand("military", mil, date));
                date.Add(new ScriptCommand("economy", eco, date));
                date.Add(new ScriptCommand("culture", cul, date));
                title.TechGroup.Add(date);
                return;
            }
            if (!Groups.ContainsKey(mil))
            {
                Groups[mil] = new TechnologyGroup();
                Groups[mil].Date = new Dictionary<int, ScriptScope>();
                Groups[mil].Date[y] = new ScriptScope((y).ToString());
                Groups[mil].Date[y].Add(new ScriptCommand("military", mil, Groups[mil].Date[y]));
                Groups[mil].Date[y].Add(new ScriptCommand("economy", eco, Groups[mil].Date[y]));
                Groups[mil].Date[y].Add(new ScriptCommand("culture", cul, Groups[mil].Date[y]));
                Groups[mil].Titles = new ScriptScope("titles");
                Groups[mil].Titles.Add("\t\t"+name);
                Script.Root.AllowDuplicates = true;
                var tech = new ScriptScope("technology");
                Script.Root.Add(tech);
                tech.Add(Groups[mil].Titles);
                tech.Add(Groups[mil].Date[y]);
                title.TechGroup = Groups[mil].Titles.Parent;

            }
            else
            {
                if (!Groups[mil].Titles.Children.Contains("\t\t" + name))
                {
                    Groups[mil].Titles.Add("\t\t" + name);
                    title.TechGroup = Groups[mil].Titles.Parent;
                }
            }
        }

        private float lerp(float min, float max, float delta)
        {
            if (delta < 0)
                delta = 0;
            float d = (max - min)*delta;
            d += min;

            d *= 10.0f;
            d = (int) d;
            d /= 10.0f;
           
            return d;
        }

        private int SortDuchyByTotalTech(TitleParser x, TitleParser y)
        {
            if (x.AverageTech > y.AverageTech)
                return -1;
            if (x.AverageTech < y.AverageTech)
                return 1;

            return 0;
        }


        public void Save()
        {
            if (!Directory.Exists(Globals.ModDir + "history\\technology\\"))
            {
                Directory.CreateDirectory(Globals.ModDir + "history\\technology\\");
            }
            if (File.Exists(Globals.ModDir + "history\\technology\\techs.txt"))
            {
                File.Delete(Globals.ModDir + "history\\technology\\techs.txt");
            }

            Script.Save();
        }

        private int holdingCost = 400;
        private int maxMerchantRepublics = 7;
        public void HandleTech(CharacterParser chr)
        {
            return;
            var provinces = chr.GetAllProvinces();
          
            foreach (var provinceParser in provinces)
            {
                if (provinceParser.economicTechPoints > holdingCost)
                {
                    bool bTribal = false;
                    if (provinceParser.government=="tribal")
                    {
                        bTribal = true;
                    }
                    if (bTribal)
                    {
                        // work toward feudalism...
                        provinceParser.economicTechPoints -= holdingCost;

                        provinceParser.baronies[0].level++;
                        if (provinceParser.baronies[0].level >= 5)
                        {
                            ScriptScope thing = new ScriptScope();
                            String barName = provinceParser.baronies[0].title;
                            thing.Name = SimulationManager.instance.Year + ".4.1";
                            bool done = false;
                            if (numMerchantRepublics < maxMerchantRepublics)
                            {
                                if (provinceParser.Adjacent.Where(p => !p.land && p.Range.Y - p.Range.X > 10).Any() && provinceParser.Title.Liege != null && provinceParser.Title.Liege.Rank==2 && provinceParser.Title.Liege.CapitalProvince == provinceParser )
                                {
                                    if (Rand.Next(4) == 0)
                                    {
                                        provinceParser.republic = true;
                                        thing.Add(new ScriptCommand() { Name = barName, Value = "city" });
                                        done = true;
                                        provinceParser.government = "republic";
                                        numMerchantRepublics++;

                                        {
                                            var chosen = provinceParser.Title.Liege;
                                            chosen.Holder.GiveTitleSoft(provinceParser.Title);
                                            chosen.CapitalProvince = provinceParser;
                                            //chosen.Holder.GiveTitleSoft(provinceParser.Title);
                                            //if (provinceParser.Title.Liege != chosen)
                                            //  chosen.Holder.GiveTitleSoft(provinceParser.Title.Liege);
                                            chosen.DoSetLiegeEvent(null);
                                            chosen.government = "republic";
                                            provinceParser.Title.government = "republic";
                                            chosen.Scope.Do(@"
		                                dignity = 200 # Never want the Republic of Venice to change primary title

                                        allow = {
			                                is_republic = yes
		                                }
                                ");

                                            {
                                                ScriptScope thingTit = new ScriptScope();

                                                thingTit.Name = SimulationManager.instance.Year + ".4.1";
                                                ScriptScope thingTit3 = new ScriptScope();

                                                thingTit3.Name = SimulationManager.instance.Year + ".4.1";

                                                chosen.titleScripts.Add(thingTit);
                                                provinceParser.Title.titleScripts.Add(thingTit3);

                                                thingTit.Add(new ScriptCommand() { Name = "law", Value = "succ_patrician_elective" });
                                                thingTit3.Add(new ScriptCommand() { Name = "law", Value = "succ_patrician_elective" });
                                                // if (title.Liege != null && title.Rank == 0 && title.TopmostTitle.government == "republic")
                                                {
                                                    // if (title.Liege.Owns[0].baronies[0].title == title.Name)
                                                    {
                                                        var name = chosen.Culture.dna.GetPlaceName();
                                                        var sname = StarNames.SafeName(name);
                                                        while (LanguageManager.instance.Get(sname) != null)
                                                        {
                                                            name = chosen.Culture.dna.GetPlaceName();
                                                            sname = StarNames.SafeName(name);
                                                        }
                                                        chosen.republicdynasties.Add(chosen.Holder.Dynasty.ID);
                                                        
                                                        for (int x = 0; x < 4; x++)
                                                        {
                                                            Dynasty d = DynastyManager.instance.GetDynasty(chosen.Culture);
                                                            if (!TitleManager.instance.dynastiesWithPalaces.Contains(d.ID))
                                                            {
                                                                var barony = TitleManager.instance.CreateBaronyScriptScope(null, chosen.Culture);
                                                                TitleManager.instance.Titles.Add(barony);
                                                                barony.government = "republicpalace";
                                                                barony.republicdynasty = d.ID;
                                                                barony.culture = chosen.culture;
                                                                barony.PalaceLocation = provinceParser;
                                                                barony.republicreligion = chosen.Holder.religion;
                                                                barony.DoSetLiegeEvent(chosen);
                                                                var cr = SimulationManager.instance.AddCharacterForTitle(barony,
                                                                    true, false, d);
                                                                
                                                                //chosen.Holder.GiveTitleSoft(barony);
                                                                chosen.Holder.Dynasty.palace = barony;
                                                                TitleManager.instance.dynastiesWithPalaces.Add(d.ID);
                                                                chosen.palaces.Add(barony);
                                                                if (barony.Rank == 0 && barony.government == "republicpalace")
                                                                {

                                                                    ScriptScope thingTit2 = new ScriptScope();

                                                                    thingTit2.Name = SimulationManager.instance.Year + ".4.1";
                                                                    thingTit2.Add(new ScriptCommand() { Name = "holding_dynasty", Value = barony.republicdynasty });
                                                                    //thing.Add(new ScriptCommand() { Name = "liege", Value = title.republicdynasty });
                                                                    if (barony.Scope.Find("culture") == null)
                                                                    {
                                                                        barony.Scope.Add(new ScriptCommand() { Name = "culture", Value = chosen.culture });
                                                                        barony.Scope.Add(new ScriptCommand() { Name = "religion", Value = chosen.Holder.religion });

                                                                    }
                                                                    barony.titleScripts.Add(thingTit2);
                                                                }
                                                            }
                                                        }
                                                        if (!TitleManager.instance.dynastiesWithPalaces.Contains(chosen.Holder.Dynasty.ID))
                                                        {
                                                            var barony = TitleManager.instance.CreateBaronyScriptScope(null, chosen.Culture);
                                                            TitleManager.instance.Titles.Add(barony);
                                                            barony.government = "republicpalace";
                                                            barony.republicdynasty = chosen.Holder.Dynasty.ID;
                                                            barony.culture = chosen.culture;
                                                            barony.republicreligion = chosen.Holder.religion;
                                                            barony.DoSetLiegeEvent(chosen);
                                                            barony.PalaceLocation = provinceParser;
                                                            chosen.Holder.GiveTitleSoft(barony);
                                                            chosen.Holder.Dynasty.palace = barony;
                                                            chosen.palaces.Add(barony);
                                                            TitleManager.instance.dynastiesWithPalaces.Add(chosen.Holder.Dynasty.ID);
                                                            if (barony.Rank == 0 && barony.government == "republicpalace")
                                                            {

                                                                ScriptScope thingTit2 = new ScriptScope();

                                                                thingTit2.Name = SimulationManager.instance.Year + ".4.1";
                                                                thingTit2.Add(new ScriptCommand() { Name = "holding_dynasty", Value = barony.republicdynasty });
                                                                //thing.Add(new ScriptCommand() { Name = "liege", Value = title.republicdynasty });
                                                                if (barony.Scope.Find("culture") == null)
                                                                {
                                                                    barony.Scope.Add(new ScriptCommand() { Name = "culture", Value = chosen.culture });
                                                                    barony.Scope.Add(new ScriptCommand() { Name = "religion", Value = chosen.Holder.religion });

                                                                }
                                                                barony.titleScripts.Add(thingTit2);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (chosen.Liege != null && chosen.Liege.Rank > chosen.Rank)
                                                {
                                                    thingTit.Add(new ScriptCommand() { Name = "liege", Value = chosen.Liege.Name });
                                                }
                                             
                                            }
                                        }
                                    }
                                }
                            }

                            if (!done)
                            {
                                thing.Add(new ScriptCommand() {Name = barName, Value = "castle"});
                                provinceParser.government = "feudalism";
                            }
                          
                            provinceParser.militaryTechPoints = 0;
                            provinceParser.dateScripts.Add(thing);
                            if (provinceParser.Title.Holder == chr)
                                chr.PrimaryTitle.government = "feudalism";
                        }
                    }
                    else if(provinceParser.militaryTechPoints > holdingCost)
                    {
                        if (provinceParser.ActiveBaronies < provinceParser.max_settlements)
                        {
                            provinceParser.militaryTechPoints -= holdingCost;
                            ProvinceParser.Barony b = provinceParser.GetLastEnabledBarony();
                            if (b.level >= 2)
                            {
                                List<String> choices = new List<string>();

                                b = provinceParser.GetNextBarony();
                                if (provinceParser.CastleCount == 0)
                                {
                                    choices.Add("castle");
                                }
                                if (provinceParser.TownCount == 0)
                                {
                                    choices.Add("city");
                                }
                                if (provinceParser.TempleCount == 0)
                                {
                                    choices.Add("temple");
                                }

                                if (choices.Count == 0)
                                {
                                    choices.Add("castle");
                                    choices.Add("city");
                                    choices.Add("temple");

                                }

                                {
                                    b.enabled = true;
                                    b.type = choices[Rand.Next(choices.Count)];
                                    ScriptScope thing = new ScriptScope();
                                    String barName = b.title;
                                    thing.Name = SimulationManager.instance.Year + ".4.1";
                                    thing.Add(new ScriptCommand() {Name = barName, Value = b.type});

                                    provinceParser.dateScripts.Add(thing);

                                }

                            }
                            else
                            {
                                b.level++;
                            }
                        }
                    }

                }
            }
        }
    }
}
