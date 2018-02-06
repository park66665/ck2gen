using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrusaderKingsStoryGen.MapGen;

namespace CrusaderKingsStoryGen
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //    GeneratedTerrainMap map = new GeneratedTerrainMap();
            //   map.Init(3072, 2048);
            //      return;
         /*   Rand.SetSeed();
            Globals.GameDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Crusader Kings II\\";

            CulturalDnaManager.instance.Init();
            CultureManager.instance.Init();

            for (int x = 0; x < 1000; x++)
            {
                String str = CultureManager.instance.AllCultures[0].dna.GetPlaceName();
                System.Console.Out.WriteLine(str);
            }
            */
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
         //   Application.Run(new ScriptBlueprint());
        }
    }
}
