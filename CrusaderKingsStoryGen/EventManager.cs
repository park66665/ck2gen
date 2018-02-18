using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This is one of the most important parts of this program.
// Should be fixed in order to update this program to CK2 2.8.1.1.

// TODO Fix this to be more compatible to CK2 2.8.1.1?

namespace CrusaderKingsStoryGen
{
    class EventNamespace
    {
        public String name;
        public List<EventParser> events = new List<EventParser>();
        public Dictionary<String, EventParser> eventmap = new Dictionary<String, EventParser>();

        public EventNamespace(String name)
        {
            this.name = name;
        }
    }
    class EventManager
    {
        
        public static EventManager instance = new EventManager();
        public List<Script>  Scripts = new List<Script>();
        public void AddToNamespace(String names, EventParser ev)
        {
            EventNamespace e = null;
            if (!NamespaceMap.ContainsKey(names))
            {
                e = new EventNamespace(names);
                NamespaceMap[names] = e;
            }
            else
            {
                e = NamespaceMap[names];
            }

            e.events.Add(ev);
            e.eventmap[ev.GetProperty("id").Value.ToString().Replace(names + ".", "")] = ev;
        }
        public void Load()
        {
            return;

            // This code is already dead.

            var files = ModManager.instance.GetFileKeys("events");
            
            foreach (var file in files)
            {
                Script s = ScriptLoader.instance.LoadKey(file);

                if (TestRemove(file))
                {                    
                    continue;
                }

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

                    if (TestRemove(sc))
                    {
                        s.Root.Remove(child);
                        continue;
                    }

                    EventParser ee = new EventParser(sc);
                    Events.Add(ee);

                    EventMap[(sc.Find("id") as ScriptCommand).Value.ToString()] = ee;
                    if (names != null)
                        AddToNamespace(names, ee);
                }
                Scripts.Add(s);
            }

            foreach (var scriptScope in Events)
            {
                scriptScope.FindLinks();
            }        

        }

        public void Save()
        {
            return;

            // Necrocode.

            var files = Directory.GetFiles(Globals.ModDir + "events\\");

            foreach (var file in files)
            {
                File.Delete(file);
            }

            for (int index = 0; index < Scripts.Count; index++)
            {
                var script = Scripts[index];
                if(script.Name.Contains("culture_conversion_events"))
                    continue;
                
                currentScript = script;
                var toDelete = new List<object>();
                var toRemove = ConvertReligionTests(script.Root, toDelete);
                if (toRemove != null)
                {
                    script.Root.Remove(toRemove);
                }
                foreach (var scriptScope in toDelete)
                {
                    var r = scriptScope;


                    ScriptScope root = null;

                    if (r is ScriptCommand)
                        root = ((ScriptCommand) r).Parent;

                    while (root.Parent != null && root.Parent != script.Root)
                    {
                        root = root.Parent;
                    }
                 
                    script.Root.Remove(root);
                }
                script.Save();
            }
        }

        public void SaveReligionEvents()
        {
         }

        public List<EventParser> Events = new List<EventParser>();
        public Dictionary<string, EventParser> EventMap = new Dictionary<string, EventParser>();
        public Dictionary<string, EventNamespace> NamespaceMap = new Dictionary<string, EventNamespace>();
        private Script currentScript;

        private ScriptScope ConvertReligionTests(ScriptScope node, List<object> toDelete )
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
                                if (o is ScriptCommand && ((ScriptCommand) o).Name.Trim() == "where")
                                {
                                    try
                                    {
                                        if (((ScriptCommand) o).Value is ScriptReference)
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
                        if (c.Value.ToString().ToUpper() == "THIS" || c.Value.ToString().ToUpper() == "FROM" || c.Value.ToString().ToUpper() == "ROOT" || c.Value.ToString().ToUpper().Contains("PREV"))
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
                        if (c.Value.ToString().ToUpper() == "THIS" )
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
            return null;
        }

        private bool TestRemove(string filename)
        {
            foreach (var str in ExcludeFile)
            {
                if (filename.Contains(str))
                    return true;
            }
            return false;
        }
        private bool TestRemove(ScriptScope scriptScope)
        {
            var c = (scriptScope.Find("id") as ScriptCommand);
            if (c != null)
            {
                ScriptReference val = c.Value as ScriptReference;
                if (c.Value is ScriptReference)
                {
                    if(val != null && Exclude.Contains(val.Referenced))
                        return true;
                }
                else if (c.Value is int)
                {
                    int val2 = (int)c.Value;
                    if (val2 != null && ExcludeID.Contains(val2))
                        return true;
                }
                    
            }

            return false;
        }

        private List<String> ExcludeFile = new List<string>()
        {
            "TOG.400",
        };
        private List<String> Exclude = new List<string>()
        {
            "TOG.400",
            "RIP.10012"
        };

        private List<int> ExcludeID = new List<int>()
        {
            10000,

        };

        public EventParser GetEvent(string id)
        {
            if (!EventMap.ContainsKey(id))
                return null;

            return EventMap[id];
        }

        public Script GetNewScript(string name)
        {
            Script script = new Script();
            script.Root = new ScriptScope();
            script.filenameIsKey = true;
            String n = "events\\" + name + ".txt";

            script.Name = n;
            EventManager.instance.Scripts.Add(script);

            return script;
        }
    }
}
