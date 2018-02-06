using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    class DecisionManager
    {
       public static DecisionManager instance = new DecisionManager();
        public List<Script>  Scripts = new List<Script>();

        private string[] exclude = new[]
        {
            "convert_to_swedish",
            "convert_to_norwegian",
            "convert_to_danish",
            "convert_to_norman",
            "convert_to_spouse_catholic",
            "convert_to_spouse_cathar",
            "convert_to_spouse_fraticelli",
            "convert_to_spouse_waldensian",
            "convert_to_spouse_lollard",
            "convert_to_spouse_bogomilist",
            "convert_to_spouse_nestorian",
            "convert_to_spouse_messalian",
            "convert_to_spouse_monothelite",
            "convert_to_spouse_iconoclast",
            "convert_to_spouse_orthodox",
            "convert_to_spouse_paulician",
            "convert_to_spouse_sunni",
            "convert_to_spouse_zikri",
            "convert_to_spouse_yazidi",
            "convert_to_spouse_ibadi",
             "convert_to_spouse_kharijite",
             "convert_to_spouse_shiite",
             "convert_to_spouse_druze",
             "convert_to_spouse_hurufi",
             "convert_to_spouse_zoroastrian",
             "convert_to_spouse_mazdaki",
             "convert_to_spouse_manichean",
             "convert_to_spouse_miaphysite",
             "convert_to_spouse_monophysite",
             "convert_to_spouse_jewish",
             "convert_to_spouse_samaritan",
             "convert_to_spouse_karaite",
             "convert_to_hinduism",
             "convert_to_buddhism",
             "convert_to_hinduism",
             "convert_to_jainism",
             "convert_to_hinduism",
             "convert_indian_branch",
             "convert_to_spouse_hindu",
             "convert_to_spouse_buddhist",
             "convert_to_spouse_jain",
             "convert_to_french",
             "convert_to_scottish",
             "convert_to_andalusian",
             "convert_to_castillan",
             "convert_to_catalan",
             "convert_to_portuguese",
             "convert_to_dutch",
             "convert_to_italian",
             "convert_to_dutch",
             "convert_to_occitan",
             "convert_to_russian",
             "renounce_iconoclasm",                      
             "convert_to_reformed",
             "form_the_hre_early",
             "become_saoshyant",
             "restore_priesthood",
             "build_third_temple"
        };
        public bool IncludeDecision(String str)
        {
            if (exclude.Contains(str))
                return false;

            return true;
        }
        public void Load()
        {
            return;
            var files = ModManager.instance.GetFileKeys("decisions");

            foreach (var file in files)
            {
                Script s = ScriptLoader.instance.LoadKey(file);
                Scripts.Add(s);
            }
        }
     

        public void Save()
        {
            return;
            var files = Directory.GetFiles(Globals.ModDir + "decisions\\");
            foreach (var file in files)
            {
                File.Delete(file);
            }

            foreach (var script in Scripts)
            {
                if (script.Root.Children.Count > 0)
                {
                    ExporeDecisions(script.Root.Children[0] as ScriptScope);
                    ConvertReligionTests(script.Root);
                    script.Save();

                }
            }
            
            String decisionGovType = Directory.GetCurrentDirectory() + "\\data\\decisiontemplates\\decisions\\storygengovernmenttype_decisions.txt";
            Script s = ScriptLoader.instance.Load(decisionGovType);
            foreach (var government in GovernmentManager.instance.governments)
            {
                if (government.cultureGroupAllow.Count > 0)
                {
                    String name = Globals.ModDir + "decisions\\" + government.name + "_government_change_decisions.txt";
                    String sn = government.name.Replace("_government", "");
                    Dictionary<String, String> conv = new Dictionary<string, string>();
                    String languageRep = LanguageManager.instance.Get(government.name.Replace("_government", "republic_government"));
                    String languageFeu = LanguageManager.instance.Get(government.name.Replace("_government", "feudal_government"));
                    conv["GOVERNMENT_FEUDALISM_REPLACE"] = government.name.Replace("_government", "feudal_government");
                    conv["GOVERNMENT_TRIBAL_REPLACE"] = government.name.Replace("_government", "tribal_government");
                    conv["GOVERNMENT_NOMADIC_REPLACE"] = government.name.Replace("_government", "nomadic_government");
                    conv["GOVERNMENT_MERCHANT_REPUBLIC_REPLACE"] = government.name.Replace("_government", "republic_government");
                    conv["CULTURE_REPLACE"] = government.cultureGroupAllow[0];
                     DecisionManager.instance.CreateFromTemplate(s, sn, name, conv);
                    LanguageManager.instance.Add(
                       sn + "_convert_to_feudalism_indep",
                        "Adopt " + languageFeu + " Feudalism");

                    LanguageManager.instance.Add(
                        sn + "_convert_to_republic_indep",
                        "Adopt " + languageRep + " Republic");

                    LanguageManager.instance.Add(
                            sn + "_convert_to_feudalism_vassal",
                            "Adopt " + languageFeu + " Feudalism");

                    LanguageManager.instance.Add(
                        sn + "_convert_to_republic_vassal",
                        "Adopt " + languageRep + " Republic");
                }
            }
        }

        private void ExporeDecisions(ScriptScope node)
        {
            for (int index = 0; index < node.Children.Count; index++)
            {
                var child = node.Children[index];
                
                if (child is ScriptScope)
                {
                    if(!IncludeDecision((child as ScriptScope).Name))
                    {
                        node.Remove(child);
                        index--;
                        continue;
                    }
             
                }
            }
        }

        private void RemoveAllReligionTests(ScriptScope node)
        {
            for (int index = 0; index < node.Children.Count; index++)
            {
                var child = node.Children[index];

                if (child is ScriptScope)
                {

                    RemoveAllReligionTests(child as ScriptScope);
                }
                if (child is ScriptCommand)
                {
                    ScriptCommand c = (ScriptCommand) child;
                    if (c.Name == "religion" || c.Name == "religion_group")
                    {
                        if (c.Value.ToString().ToUpper() == "FROM" || c.Value.ToString().ToUpper() == "ROOT" || c.Value.ToString().ToUpper().Contains("PREV"))
                        {

                        }
                        else
                        {
                            node.Remove(child);
                            index--;

                        }
                        continue;
                    }
                    if (c.Name == "culture" || c.Name == "culture_group")
                    {
                        if (c.Value.ToString().ToUpper() == "FROM" || c.Value.ToString().ToUpper() == "ROOT" || c.Value.ToString().ToUpper().Contains("PREV"))
                        {

                        }
                        else
                        {
                            node.Remove(child);
                            index--;

                        }
                        continue;
                    }
                }
            }
        }

        public void SearchReplace(ScriptScope node, Dictionary<String, String> searchReplace, String prefix)
        {
            if (node.Name != null && node.Name.StartsWith("PREFIX_DECISION_NAME"))
            {
                node.Parent.ChildrenMap.Remove(node.Name);
                node.NameSearchReplaced = node.Name;
                node.Name = node.Name.Replace("PREFIX_DECISION_NAME", prefix);
           
                node.Parent.ChildrenMap[node.Name] = node.Name;
            }

            for (int index = 0; index < node.Children.Count; index++)
            {
                var child = node.Children[index];

                if (child is ScriptScope)
                {

                    SearchReplace(child as ScriptScope, searchReplace, prefix);
                }
                if (child is ScriptCommand)
                {
                    ScriptCommand c = (ScriptCommand)child;
                    if (searchReplace.ContainsKey(c.Value.ToString().Trim()))
                    {
                        c.Replaced = c.Value;
                        c.Value = searchReplace[c.Value.ToString().Trim()];
                    }
                }
            }
        }
        public void Correct(ScriptScope node, Dictionary<String, String> searchReplace, String prefix)
        {
            if (node.Name != null && node.Name.StartsWith(prefix) && node.NameSearchReplaced !=null)
            {
                node.Parent.ChildrenMap.Remove(node.Name);
                node.Name = node.NameSearchReplaced;
                node.Parent.ChildrenMap[node.Name] = node;
                node.NameSearchReplaced = null;
            
            }

            for (int index = 0; index < node.Children.Count; index++)
            {
                var child = node.Children[index];

                if (child is ScriptScope)
                {

                    Correct(child as ScriptScope, searchReplace, prefix);
                }
                if (child is ScriptCommand)
                {
                    ScriptCommand cc = (ScriptCommand)child;
                    if (searchReplace.ContainsValue(cc.Value.ToString().Trim()))
                    {
                        cc.Value = cc.Replaced;
                    }
                }
            }
        }
        private void ConvertReligionTests(ScriptScope node)
        {
            for (int index = 0; index < node.Children.Count; index++)
            {
                var child = node.Children[index];
                if (child is ScriptScope)
                    ConvertReligionTests(child as ScriptScope);
                if (child is ScriptCommand)
                {
                    ScriptCommand c = (ScriptCommand)child;
                    if (c.Name == "religion" || c.Name == "secret_religion")
                    {
                        if (c.Value.ToString().ToUpper() == "FROM" || c.Value.ToString().ToUpper() == "ROOT" || c.Value.ToString().ToUpper().Contains("PREV"))
                        {

                        }
                        else
                        {
                            if (c.Value.ToString() == "catholic")
                                c.Value = ReligionManager.instance.CatholicSub.Name;
                            else if (c.Value.ToString() == "orthodox")
                                c.Value = ReligionManager.instance.OrthodoxSub.Name;
                            else if (c.Value.ToString() == "sunni")
                                c.Value = ReligionManager.instance.SunniEquiv.Name;
                            else if (c.Value.ToString() == "shiite")
                                c.Value = ReligionManager.instance.ShiiteEquiv.Name;
                            else if (c.Value.ToString() == "norse_pagan")
                                c.Value = ReligionManager.instance.NorseSub.Name;
                            else if (c.Value.ToString() == "norse_pagan_reformed")
                                c.Value = ReligionManager.instance.NorseReformSub.Name;

                        }
                        continue;
                    }
             
                    if (c.Name == "religion_group")
                    {
                        if (c.Value.ToString().ToUpper() == "FROM" || c.Value.ToString().ToUpper() == "ROOT" || c.Value.ToString().ToUpper().Contains("PREV"))
                        {

                        }
                        else
                        {
                            if (c.Value.ToString() == "christian")
                                c.Value = ReligionManager.instance.ChristianGroupSub.Name;
                            else if (c.Value.ToString() == "muslim")
                                c.Value = ReligionManager.instance.MuslimGroupSub.Name;
                            else if (c.Value.ToString() == "indian_group")
                                c.Value = ReligionManager.instance.IndianGroupSub.Name;
                            else if (c.Value.ToString() == "zoroastrian_group")
                                c.Value = ReligionManager.instance.ZoroGroupSub.Name;
                            else if (c.Value.ToString() == "jewish_group")
                                c.Value = ReligionManager.instance.JewGroupSub.Name;
                            else if (c.Value.ToString() == "pagan_group")
                                c.Value = ReligionManager.instance.PaganGroupSub.Name;
                      
                        }
                        continue;
                    }

                }
            }
        }
        public void CreateFromTemplate(Script s, String prefix, String targetFilename,
            Dictionary<String, String> convert)
        {
           
            s.Name = targetFilename;

            SearchReplace(s.Root, convert, prefix);

            s.Save();
            Correct(s.Root, convert, prefix);
        }
    }
}
