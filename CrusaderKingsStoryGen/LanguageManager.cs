using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    class LanguageManager
    {
        public static LanguageManager instance = new LanguageManager();
        Dictionary<String, String> english = new Dictionary<string, string>();

        public LanguageManager()
        {
            AddSafe("Emperor");
            AddSafe("King");
            AddSafe("Duke");
            AddSafe("Count");
            AddSafe("Baron");
            AddSafe("Mayor");

        }
        public String Add(String key, String english)
        {
            if (english == null || english.Length == 1)
            {
                english = StarNames.Generate(Rand.Next(45645546));
            }

            this.english[key] = english;
            this.english[key + "_adj"] = english;
            this.english[key + "_desc"] = english;
            return english;
        }
        public String AddDirect(String key, String english)
        {
         
            this.english[key] = english;
         
            return english;
        }

        public void LoadVanilla()
        {
            var files = ModManager.instance.GetFiles("localisation");
            foreach (var file in files)
            {
                using (System.IO.StreamReader load =
                    new System.IO.StreamReader(file, Encoding.GetEncoding(1252)))
                {
                    using (System.IO.StreamWriter file2 =
                        new System.IO.StreamWriter(
                            Globals.ModDir + "localisation\\" + file.Substring(file.LastIndexOf('\\')), false,
                            Encoding.GetEncoding(1252)))
                    {
                        try
                        {
                            while (!load.EndOfStream)
                            {
                                string str = load.ReadLine();
                                var split = str.Split(';');
                                if (split[0] == "e_britannia")
                                {
                                    
                                }
                                LanguageManager.instance.AddDirect(split[0].Trim(), split[1].Trim());
                                
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        public void DoSubstitutes()
        {
            var files = Directory.GetFiles(Globals.ModDir + "localisation\\");
            foreach (var file in files)
            {
                File.Delete(file);
            }

            files = ModManager.instance.GetFiles("localisation");
            foreach (var file in files)
            {
                if (file.Contains("customizable"))
                    continue;

                bool bStripped = false;
                bool bReplaced = false;
                using (System.IO.StreamReader load =
                  new System.IO.StreamReader(file, Encoding.GetEncoding(1252)))
                {
                    using (System.IO.StreamWriter file2 =
                           new System.IO.StreamWriter(Globals.ModDir + "localisation\\" + file.Substring(file.LastIndexOf('\\')), false, Encoding.GetEncoding(1252)))
                    {
                        try
                        {
                            while (!load.EndOfStream)
                            {
                                string str = load.ReadLine();

                                if (str.StartsWith("pagan_group;") || str.StartsWith("pagan;") || str.StartsWith("norse;")  || str.StartsWith("c_") || str.StartsWith("b_") || str.StartsWith("d_") || str.StartsWith("k_") || str.StartsWith("e_"))
                                {
                                    bStripped = true;
                                    continue;
                                }

                                if (trimProvinces && str.StartsWith("PROV"))
                                {
                                    try
                                    {
                                        String small = str.Substring(4);
                                        small = small.Substring(0, small.IndexOf(';'));
                                        Convert.ToInt32(small);
                                        bStripped = true;
                                        continue;
                                    }
                                    catch (Exception)
                                    {
                                                                                
                                    }
                             
                                }
                                foreach (var key in substitutions.Keys)
                                {
                                    if (str.Contains(key))
                                    {                                   
                                        int i = str.IndexOf(';');
                                        string sub = str.Substring(i+1);
                                        if (sub.Contains(key))
                                        {
                                            str = str.Substring(0, i+1) + sub.ReplaceMinusEscape(key, substitutions[key]);
                                            bReplaced = true;
                                        }
                                    }
                                }
                                        
                                file2.Write(str + Environment.NewLine);
                             
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    if (!bStripped && !bReplaced)
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

        }

        public void Save()
        {
            String filename = "localisation\\aaa_genLanguage.csv";

            filename = Globals.ModDir + filename;

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {
                foreach (var entry in english)
                {
                    file.Write(entry.Key + ";" + entry.Value + ";;;;;;;;;;;;;\n");
                }

                file.Close();
            }


            //thing;eng;;;;;;;;;;;;;
        }

        public void Remove(string name)
        {
            this.english.Remove(name);
        }

        public string Get(string name)
        {
            if (!english.ContainsKey(name))
                return "";
            if (english[name] == null)
                english[name] = "";
            return english[name].Trim();
        }

        public string AddSafe(string name)
        {
            String safe = StarNames.SafeName(name);
            Add(safe, name);
            return safe;
        }

        public void CopyToolDir(string dir)
        {
            return;

        }
        Dictionary<String, String> substitutions = new Dictionary<string, string>();
        public bool trimProvinces = true;

    
        public void SetupReligionEventSubsitutions()
        {

            try
            {
                substitutions["Pope"] = Get(ReligionManager.instance.CatholicSub.PopeName);
                substitutions["Jesus"] = ReligionManager.instance.ChristianGroupSub.Religions[0].high_god_name;
                substitutions["Christian"] = Get(ReligionManager.instance.ChristianGroupSub.Name) + "an";
                substitutions["Holy Father"] = "Holy " + Get(ReligionManager.instance.ChristianGroupSub.Religions[0].PopeName);
                substitutions["Catholic"] = Get(ReligionManager.instance.ChristianGroupSub.Name) + "an";
                substitutions["christian"] = Get(ReligionManager.instance.ChristianGroupSub.Name) + "an";
                substitutions["catholic"] = Get(ReligionManager.instance.ChristianGroupSub.Name) + "an";
                substitutions["Bible"] = ReligionManager.instance.CatholicSub.scripture_name;
                substitutions["bible"] = substitutions["Bible"].ToLower();
                substitutions["Crusade"] = ReligionManager.instance.ChristianGroupSub.Religions[0].crusade_name;
                substitutions["crusade"] = ReligionManager.instance.ChristianGroupSub.Religions[0].crusade_name;

                var mainHolyPlace = ReligionManager.instance.MuslimGroupSub.Religions[0].holySites.First();
                substitutions["Sadaqah"] = ReligionManager.instance.MuslimGroupSub.holySites.First().Culture.dna.GetPlaceName();
                substitutions["Ramadan"] = ReligionManager.instance.MuslimGroupSub.holySites.First().Culture.dna.GetPlaceName();
                substitutions["Mecca"] = Get(mainHolyPlace.title);

                substitutions["Allah"] = ReligionManager.instance.MuslimGroupSub.Religions[0].high_god_name;
                substitutions["Muslim"] = Get(ReligionManager.instance.MuslimGroupSub.Name);
                substitutions["Islam"] = Get(ReligionManager.instance.MuslimGroupSub.Name);
                substitutions["Hajj"] = "pilgrimage";
                substitutions["Caliph"] = ReligionManager.instance.MuslimGroupSub.Religions[0].priest;
                substitutions["Sunni"] = Get(ReligionManager.instance.SunniEquiv.Name);
                substitutions["Shiite"] = Get(ReligionManager.instance.ShiiteEquiv.Name);
                substitutions["Ash'ari"] = ReligionManager.instance.MuslimGroupSub.Provinces[0].Culture.dna.ConstructWord(4, 7);
                substitutions["Mu'tazila"] = ReligionManager.instance.MuslimGroupSub.Provinces[0].Culture.dna.ConstructWord(4, 7);
                substitutions["Fatwa"] = ReligionManager.instance.MuslimGroupSub.Provinces[0].Culture.dna.ConstructWord(4, 7);
                substitutions["Kaaba"] = ReligionManager.instance.MuslimGroupSub.Provinces[0].Culture.dna.ConstructWord(4, 7);
                substitutions["Muhammad"] = ReligionManager.instance.MuslimGroupSub.Provinces[0].Culture.dna.ConstructWord(4, 7);
                substitutions["Hajajj"] = ReligionManager.instance.MuslimGroupSub.Provinces[0].Culture.dna.ConstructWord(4, 7);
                substitutions["Furusiyya"] = ReligionManager.instance.MuslimGroupSub.Provinces[0].Culture.dna.ConstructWord(4, 8);

                substitutions["Qur'an"] = ReligionManager.instance.ShiiteEquiv.scripture_name;
                substitutions["Jihad"] = ReligionManager.instance.MuslimGroupSub.Religions[0].crusade_name;
                substitutions["jihad"] = ReligionManager.instance.MuslimGroupSub.Religions[0].crusade_name;
                substitutions["imam"] = ReligionManager.instance.MuslimGroupSub.Religions[0].priest;
                substitutions["Kafir"] = "infidels";


                substitutions["pagan"] = Get(ReligionManager.instance.PaganGroupSub.Name);
                substitutions["Pagan"] = Get(ReligionManager.instance.PaganGroupSub.Name);
                substitutions["Blot"] = ReligionManager.instance.NorseSub.Provinces[0].Culture.dna.ConstructWord(4, 5);
                substitutions["blot"] = substitutions["Blot"];
                substitutions["Odin"] = ReligionManager.instance.NorseSub.gods[0];
                substitutions["Thor"] = ReligionManager.instance.NorseSub.gods[1];
                substitutions["Freyr"] = ReligionManager.instance.NorseSub.gods[2];

                if (ReligionManager.instance.JewGroupSub.Religions[0].holySites.Any())
                {
                    var sHolyPlace = ReligionManager.instance.JewGroupSub.Religions[0].holySites.ToList()[1];//First();
                    substitutions["Jerusalem"] = Get(sHolyPlace.title);
                }
                substitutions["Jew"] = ReligionManager.instance.JewGroupSub.Provinces[0].Culture.dna.ConstructWord(2, 3);
                substitutions["jew"] = substitutions["Jew"].ToLower();
                substitutions["Third Temple"] = ReligionManager.instance.JewGroupSub.Provinces[0].Culture.dna.ConstructWord(2, 4) + " Temple";
                substitutions["Passover"] = ReligionManager.instance.JewGroupSub.Provinces[0].Culture.dna.ConstructWord(5, 8);

                substitutions["Jain"] = ReligionManager.instance.JainEquiv.Name;
                substitutions["Buddhist"] = ReligionManager.instance.BuddhistEquiv.Name;
                substitutions["Hindu"] = ReligionManager.instance.HinduEquiv.Name;
            }
            catch (Exception)
            {
          
            }
         
        }

    }
}
