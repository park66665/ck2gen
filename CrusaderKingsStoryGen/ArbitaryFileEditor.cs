using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    public class ArbitaryFileEditor
    {
        public static ArbitaryFileEditor instance = new ArbitaryFileEditor();

        public void CopyAndSubstitute(String filename, Dictionary<String, String> subList, bool bMapDir = false)
        {
            String file = Globals.GameDir + filename;
            if(bMapDir)
                file = Globals.MapDir + filename;

            if (File.Exists(Globals.ModDir + filename))
            {
                File.Delete(Globals.ModDir + filename);
            }
            bool bStripped = false;
            using (System.IO.StreamReader load =
                new System.IO.StreamReader(file, Encoding.GetEncoding(1252)))
            {
                using (System.IO.StreamWriter file2 =
                    new System.IO.StreamWriter(
                        Globals.ModDir + filename, false,
                        Encoding.GetEncoding(1252)))
                {
                    try
                    {
                        while (!load.EndOfStream)
                        {
                            string str = load.ReadLine();
                            bool done = false;

                            foreach (var item in subList)
                            {
                                bool i = str.Trim().StartsWith(item.Key);
                                if (i)
                                {
                                    done = true;
                                    file2.Write(str.Substring(0, str.IndexOf(item.Key)) + item.Value + Environment.NewLine);

                                }
                                if (done)
                                    break;
                            }
                            if(done)
                                continue;
                                                     
                            file2.Write(str + Environment.NewLine);
                         
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
             



            }
        }
    }
}
