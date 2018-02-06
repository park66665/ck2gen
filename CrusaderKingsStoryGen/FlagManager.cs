using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    class FlagManager
    {
        public static FlagManager instance = new FlagManager();

        List<Bitmap> bmpList = new List<Bitmap>();
        public void AssignAndSave()
        {
            if (!Directory.Exists(Globals.ModDir + "gfx\\flags\\"))
                Directory.CreateDirectory(Globals.ModDir + "gfx\\flags\\");
            var files = Directory.GetFiles(Globals.ModDir + "gfx\\flags\\");
            foreach (var file in files)
            {
                File.Delete(file);
            }

            files = Directory.GetFiles(Globals.GameDir + "gfx\\flags\\");

            Bitmap bmp = new Bitmap(2048, 2048);
            bmpList.Add(bmp);
      
            int x = 0;
            int y = 0;
            int w = 76;
            List<String> filenames = new List<string>(files);
            int n = 0;
            int sheets = 0;
            foreach (var titleParser in TitleManager.instance.Titles)
            {
                if (titleParser.Rank < 1)
                    continue;


                SolidBrush b = new SolidBrush(Color.FromArgb(255, Rand.Next(255), Rand.Next(255), Rand.Next(255)));
                Graphics g = Graphics.FromImage(bmp);
                int xx = x * w;
                int yy = y * w;
                int i = Rand.Next(filenames.Count);
                String str = filenames[i];
                if (File.Exists(Globals.ModDir + "gfx\\flags\\" + titleParser.Name + ".tga"))
                    File.Delete(Globals.ModDir + "gfx\\flags\\" + titleParser.Name + ".tga");
                File.Copy(str, Globals.ModDir + "gfx\\flags\\" + titleParser.Name + ".tga");
                b.Dispose();

            }
            foreach (var titleParser in TitleManager.instance.mercs)
            {

                SolidBrush b = new SolidBrush(Color.FromArgb(255, Rand.Next(255), Rand.Next(255), Rand.Next(255)));
                Graphics g = Graphics.FromImage(bmp);
                int xx = x * w;
                int yy = y * w;
                int i = Rand.Next(filenames.Count);
                String str = filenames[i];
                if (File.Exists(Globals.ModDir + "gfx\\flags\\" + titleParser + ".tga"))
                    File.Delete(Globals.ModDir + "gfx\\flags\\" + titleParser + ".tga");
                File.Copy(str, Globals.ModDir + "gfx\\flags\\" + titleParser + ".tga");

                b.Dispose();

            }

        }
    }
}
