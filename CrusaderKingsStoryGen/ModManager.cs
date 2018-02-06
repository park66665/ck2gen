using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    public class ModManager
    {
        public static ModManager instance = new ModManager();

        public Dictionary<String, String> FileMap = new Dictionary<string, string>();
        public List<Mod> Mods = new List<Mod>();
        private string rootDir;

        public void LoadVanilla()
        {
            Mods.Clear();
            Mod mod = new Mod();
            mod.name = "vanilla";
            Mods.Add(mod);
            rootDir = Globals.GameDir;
            LoadDir(Globals.GameDir, mod);
        }
        public void Load(String modFile)
        {
            Mod mod = new Mod();
            mod.name = modFile.Substring(modFile.LastIndexOf("\\")+1).Replace(".mod", "").Trim();
            Mods.Add(mod);
            String usePath = "";
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(modFile, Encoding.GetEncoding(1252)))
            {
                string line = "";

                
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("replace_path"))
                    {
                        String path = line.Substring(line.IndexOf('=') + 1).Replace("\"", "").Replace("/", "\\").Trim();
                        mod.replaceDirs.Add(path);
                        var toRemove = FileMap.Where(o => o.Key.StartsWith(path)).ToList();

                        toRemove.ForEach(p => FileMap.Remove(p.Key));
                    }
                    if (line.Trim().StartsWith("path"))
                    {
                        String path = line.Substring(line.IndexOf('=') + 1).Replace("\"", "").Replace("/", "\\").Trim();
                        usePath = path;
                    }
                    if (line.Trim().StartsWith("name"))
                    {
                        String path = line.Substring(line.IndexOf('=') + 1).Replace("\"", "").Replace("/", "\\").Trim();
                        if (path.Contains("#"))
                        {
                            path = path.Split('#')[0].Trim();
                        }
                        mod.name = path;
                    }
                }
            }
            rootDir = modFile.Substring(0, modFile.LastIndexOf("\\") + 1) + usePath.Replace("mod\\", "")+ "\\";
            LoadDir(rootDir, mod);
        }

        public String[] GetFiles(String path)
        {
            var list = FileMap.Where(f => f.Key.StartsWith(path)).ToList();
            List<String> str = new List<string>();
            list.ForEach(l => str.Add(l.Value));

            return str.ToArray();
        }
        public String[] GetFileKeys(String path)
        {
            var list = FileMap.Where(f => f.Key.StartsWith(path)).ToList();
            List<String> str = new List<string>();
            list.ForEach(l => str.Add(l.Key));

            return str.ToArray();
        }

        public void LoadDir(string dir, Mod mod)
        {
            String[] dirs = Directory.GetDirectories(dir);

            foreach (var s in dirs)
            {
                LoadDir(s, mod);
            }

            String[] files = Directory.GetFiles(dir);

            foreach (var file in files)
            {
                String stripped = file.Replace(rootDir, "");
                mod.FileMap[stripped] = file;
                FileMap[stripped] = file;
            }
        }

   
        public class Mod
        {
            public String name;
            public List<String> replaceDirs = new List<string>();
            public Dictionary<String, String> FileMap = new Dictionary<string, string>();
        }

        public void LoadMods()
        {
            LoadVanilla();
            foreach (var m in ModsToLoad)
            {
                Load(Globals.ModRootDir + m + ".mod");
            }
        }

        public string GetDependencies()
        {
            String depStr = "";

            foreach (var mod in Mods)
            {
                if(mod.name=="vanilla")
                    continue;
                depStr += "\"" + mod.name + "\"" + " ";
            }

            return "dependencies = { " + depStr.Trim() + " }";
        }

        public List<String> ModsToLoad = new List<string>();
        public void AddModsToLoad(string val)
        {
            ModsToLoad.Add(val);
        }

        public void Init()
        {
            foreach (var setting in Globals.Settings.Where(k => k.Key.StartsWith("Mod")).OrderBy(p=>p.Key).ToList())
            {
                ModManager.instance.ModsToLoad.Add(setting.Value);
            }
        }

        public bool IsVanilla(string str)
        {
            if (FileMap.ContainsKey(str))
            {
                if (FileMap[str].StartsWith(Globals.GameDir))
                    return true;

                return false;
            }

            return true;
        }
    }
}
