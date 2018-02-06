using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    partial class SocietyManager
    {
        public enum Template
        {
 
            the_satanists,
            the_cold_ones,
            the_plaguebringers,
            the_cult_of_kali,
            the_trollcrafters,
            monastic_order_orthodox,
            monastic_order_benedictine,
            monastic_order_dominican,
            monastic_order_nestorian,
            monastic_order_monophysite,
            monastic_order_hindu,
            monastic_order_buddhist,
            monastic_order_jain,
            the_assassins,
            hermetics

        }
        public List<Template> Monastic = new List<Template>() {
            Template.monastic_order_orthodox,
            Template.monastic_order_benedictine,
            Template.monastic_order_dominican,
            Template.monastic_order_nestorian,
            Template.monastic_order_monophysite,
            Template.monastic_order_hindu,
            Template.monastic_order_buddhist,
            Template.monastic_order_jain,};

        public List<Template> Evil = new List<Template>() {
            Template.the_satanists,
            Template.the_cold_ones,
            Template.the_plaguebringers,
            Template.the_cult_of_kali,
            Template.the_trollcrafters};

        public List<Template> Muslim = new List<Template>() {
            Template.the_assassins,
         };

        public List<Template> General = new List<Template>() {
            Template.hermetics,
         };
        public List<String> secretSocieties = new List<string>();
        public List<String> monasticSocieties = new List<string>();
        public static SocietyManager instance = new SocietyManager();
        public List<Script> Scripts = new List<Script>();
        public Dictionary<String, ScriptScope> Societies = new Dictionary<String, ScriptScope>();
        private Script currentScript;
        private Template contextTemplate;
        private string contextName;
        public Dictionary<Template, List<String>> TestContainer = new Dictionary<Template, List<string>>();
        private ReligionParser contextReligion;

        public void CreateSocietyForReligion(ReligionParser r, string c, ScriptScope scope, Template template)
        {
            var lang = StarNames.Generate(c);

            switch (Rand.Next(5))
            {
                case 0:
                    lang = "The " + lang + " Order";
                    break;
                case 1:
                    lang = "Children of " + lang;
                    break;
                case 2:
                    lang = "Way of " + lang;
                    break;
                case 3:
                    lang = "Society of " + lang;
                    break;
                case 4:
                    lang = "Sons of " + lang;
                    break;
            }

            var name = lang.AddSafe();
            ScriptScope newSociety = new ScriptScope(name);
            var temp = Societies[template.ToString()];
            Societies[name] = newSociety;
            contextName = name;
            contextReligion = r;

            CreateAssassinTemplate(Scripts[0].Root, ScripterTriggerManager.instance.script.Root, name, r);

            (Scripts[0].Root.Children[Scripts[0].Root.Children.Count-1] as ScriptScope).Tag1 = r;

            //   newSociety.FillFrom(temp, new ScriptScope.CopyDelegate(SocietyModify));
        }
/*
        private void SocietyModify(object o)
        {
            if (o is ScriptScope)
            {
                ScriptScope scope = o as ScriptScope;
                
            }
            else
            {
                ScriptCommand command = o as ScriptCommand;

                if (command.Name == "religion")
                {
                    command.Value = contextReligion.Name;
                }
                if (command.Name == "religion_group")
                {
                    command.Value = contextReligion.Group.Name;
                }
            }
        }
        */

        public void Load()
        {
    
            var files = ModManager.instance.GetFileKeys("common\\societies");

            foreach (var file in files)
            {
                Script s = ScriptLoader.instance.LoadKey(file);


                string names = null;
                for (int index = 0; index < s.Root.Children.Count; index++)
                {
                    var child = s.Root.Children[index];
                    if (child is ScriptCommand)
                    {
                        if (child.ToString().Contains("namespace"))
                        {
                            names = (child as ScriptCommand).Value.ToString();
                        }
                        continue;
                    }
                    var sc = (child as ScriptScope);

                    Societies[(sc as ScriptScope).Name] = sc as ScriptScope;
                    



                }
                s.Root.Children.Clear();
                s.Root.ChildrenMap.Clear();
                s.Root.ChildrenHas.Clear();
                Scripts.Add(s);
            }


        }
        private ScriptScope ConvertReligionTests(ScriptScope node, List<object> toDelete)
        {
            for (int index = 0; index < node.Children.Count; index++)
            {
                var child = node.Children[index];
                if (child is ScriptScope)
                {
                    var sc = child as ScriptScope;

                    if (sc.Name == "narrative_event")
                    {
                        //     node.Children.Remove(sc);
                        //    continue;
                    }
                }
                if (child is ScriptScope)
                {

                    var sc = child as ScriptScope;
                    if (sc.Name == "distance")
                    {
                        foreach (var o in sc.Children)
                        {
                            if (o is ScriptCommand && ((ScriptCommand)o).Name.Trim() == "where")
                            {
                                try
                                {
                                    if (((ScriptCommand)o).Value is ScriptReference)
                                    {
                                        int prov = Convert.ToInt32(((ScriptReference)((ScriptCommand)o).Value).Referenced);
                                        toDelete.Add((ScriptCommand)o);
                                        return null;
                                    }

                                    int prov2 = Convert.ToInt32((((ScriptCommand)o).Value));
                                    toDelete.Add(Convert.ToInt32(((ScriptCommand)o)));
                                    return null;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }

                    if (sc.Name == "regional_percentage")
                    {
                        node.Remove(child);
                        index--;
                        continue;
                    }
                    ConvertReligionTests(child as ScriptScope, toDelete);
                    if ((child as ScriptScope).Children.Count == 0)
                    {
                        node.Remove(child);
                        index--;
                    }
                    if ((child as ScriptScope).Children.Count == 1 && (child as ScriptScope).Name == "modifier")
                    {
                        node.Remove(child);
                        index--;
                    }
                }
                if (child is ScriptCommand)
                {
                    ScriptCommand c = (ScriptCommand)child;

                    if (c.Name == "completely_controls")
                    {

                        toDelete.Add(c);
                        return null;
                    }

                    if (c.Name == "culture_group" || c.Name == "culture")
                    {
                        if (c.Value.ToString().ToUpper() == "FROM" || c.Value.ToString().ToUpper() == "ROOT" ||
                            c.Value.ToString().ToUpper().Contains("PREV"))
                        {

                        }
                        else
                        {
                            toDelete.Add(c);
                            return null;
                        }
                    }
                    if (c.Name == "religion" || c.Name == "secret_religion")
                    {
                        if (c.Value.ToString().ToUpper() == "FROM" || c.Value.ToString().ToUpper() == "ROOT" || c.Value.ToString().ToUpper().Contains("PREV"))
                        {

                        }
                        else
                        {
                            if (c.Value.ToString() == "hindu")
                                c.Value = ReligionManager.instance.HinduEquiv.Name;
                            else if (c.Value.ToString() == "buddhist")
                                c.Value = ReligionManager.instance.BuddhistEquiv.Name;
                            else if (c.Value.ToString() == "jain")
                                c.Value = ReligionManager.instance.JainEquiv.Name;
                            else if (c.Value.ToString() == "catholic")
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
                            else
                            {
                                toDelete.Add(c);
                            }
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
                            else
                            {
                                toDelete.Add(c);
                            }

                        }
                        continue;
                    }

                }
            }
            return null;
        }

        public void Save()
        {
            return;

            var files = Directory.GetFiles(Globals.ModDir + "common\\societies\\");
            foreach (var file in files)
            {
                File.Delete(file);
            }

            for (int index = 0; index < Scripts.Count; index++)
            {
                var script = Scripts[index];
                if (script.Name.Contains("culture_conversion_events"))
                    continue;

                currentScript = script;
                var toDelete = new List<object>();
                //   var toRemove = ConvertReligionTests(script.Root, toDelete);
                //  if (toRemove != null)
                //    {
                //       script.Root.Remove(toRemove);
                //    }

                foreach (var c in Scripts[0].Root.Children)
                {
                    var scope = (c as ScriptScope);
                    if (scope != null)
                    {
                        if (scope.Tag1 != null)
                            if ((scope.Tag1 as ReligionParser).Provinces.Count == 0)
                            {
                                toDelete.Add(scope);
                            }

                    }
                }

                foreach (var scriptScope in toDelete)
                {
                     script.Root.Remove(scriptScope);
                }

                script.Save();
            }
        }

    }

    class Society
    {
        public ScriptScope Scope;
        

    }
/*
    class Society : ScopeProxy
    {

        public string primary_attribute
        {
            get {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                return GetString(propertyName);
            }
            set
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                SetValue(propertyName, value);
            }
        }

        public bool is_religious
        {
            get
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                return GetBool(propertyName);
            }
            set
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                SetValue(propertyName, value);
            }
        }
        public int opinion_to_other_members
        {
            get
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                return GetInt(propertyName);
            }
            set
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                SetValue(propertyName, value);
            }
        }
        public int opinion_per_rank_above
        {
            get
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                return GetInt(propertyName);
            }
            set
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                SetValue(propertyName, value);
            }
        }
        public string sound
        {
            get
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                return GetString(propertyName);
            }
            set
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                SetValue(propertyName, value);
            }
        }
        public string society_ranks_gfx
        {
            get
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                return GetString(propertyName);
            }
            set
            {
                var propertyName = MethodBase.GetCurrentMethod().Name.Substring(4);
                SetValue(propertyName, value);
            }
        }


    }*/
}
