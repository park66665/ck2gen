using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    class Globals
    {
        public static string SrcTraitIconDir =
             "C:\\Users\\MEMAIN\\Documents\\Paradox Interactive\\Crusader Kings II\\mod\\storygen\\_data\\";

        public static string GameDir
        {
            get
            {
                if (!Settings.ContainsKey("GameDir"))
                    Settings["GameDir"] = "";
                return Settings["GameDir"];
            }
            set { Settings["GameDir"] = value; }
        }
        public static string MapDir
        {
            get
            {
                if (!Settings.ContainsKey("MapDir"))
                    Settings["MapDir"] = "";
                return Settings["MapDir"];
            }
            set { Settings["MapDir"] = value; }
        }
        public static string ModDir { get; set; }

       public static string MapOutputDir = null;
        public static string OModDir = "C:\\Users\\ME\\Documents\\Paradox Interactive\\Crusader Kings II\\mod\\storygen\\";
          public static string UserDir = "C:\\Users\\ME\\Documents\\Paradox Interactive\\Crusader Kings II\\storygen\\";

        public static string EUTileDir =
            "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Europa Universalis IV\\map\\random\\tiles\\";
        public static int OneInChanceOfReligionSplinter = 3;
 
        public static float BaseChanceOfRevolt = 1000.0f;
   
        public static int OneInChanceOfCultureSplinter = 1;
        public static int StartProvinceID = 872;
        public static string ModRoot { get; set; }
        public static string ModName { get; set; } = "storygen";
        public static string MapName { get; set; } = "randomMap2";
        public static string MapOutputTotalDir
        {
            get { return MapOutputDir + "\\" + MapName + "\\"; }
        }

        public static int Climate { get; set; } = 0;

        public static string ModRootDir
        {
            get { return ModDir.Substring(0, ModDir.Substring(0, ModDir.Length-1).LastIndexOf('\\')) + "\\"; }
        }
        public static void LoadSettings(StreamReader file, string firstLine)
        {
            String line = firstLine;
            do
            {
                String[] split = line.Split('=');
                String setting = split[0].Trim();
                String val = split[1].Trim();
                Settings[setting] = val;


            } while ((line = file.ReadLine()) != null);
        }
        public static void SaveSettings()
        {

            String filename = ".\\settings.txt";
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, false, Encoding.GetEncoding(1252)))
            {

                var list = Settings.OrderBy(s => s.Key).ToList();

                foreach (var keyValuePair in list)
                {
                    file.WriteLine(keyValuePair.Key + "="+ keyValuePair.Value);
                }
                
             
                file.Close();
            }

        
        }

        public static Dictionary<String, String> Settings = new Dictionary<string, string>();
    }
}
