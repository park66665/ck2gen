using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    public class EventLogger
    {
        public static EventLogger instance = new EventLogger();

        public List<string> titleLogList = new List<string>();

        public EventLogger()
        {
         //   AddTitle("c_blois");
         //   AddTitle("k_lotharingia");

        }

        Dictionary<string, System.IO.StreamWriter> files = new Dictionary<string, StreamWriter>();

        internal void AddTitle(string title)
        {
            if (files.ContainsKey(title))
                return;

            string filename = Directory.GetCurrentDirectory() + "\\logs\\" + title + ".txt";

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\logs\\"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\logs\\");
            }
            string f = filename;
            int n = 2;
            while (File.Exists(f))
            {
                f = filename.Replace(".txt", n + ".txt");
                n++;
            }

            System.IO.StreamWriter file =
                new System.IO.StreamWriter(f);


            files[title] = file;

        }

        public void Log(string title, string text)
        {
            if (!files.ContainsKey(title))
                return;

            files[title].WriteLine(Simulation.SimulationManager.instance.Year + " - " + text);
        }

        public void Save()
        {
            foreach (var filesValue in files.Values)
            {
                filesValue.Close();
            }
        }
    }
}
