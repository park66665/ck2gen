using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen.Story
{
    class StoryManager
    {
        public static StoryManager instance = new StoryManager();
        public List<String> events = new List<string>();
        public int lastYear = -1;
        private object lastSubject;

        public void CreateEvent(int year, String text, object primarySubject)
        {
            String yearStr = "";
            if (lastYear != -1 && lastYear < year && primarySubject == lastSubject)
            {
                if (year - lastYear == 1)
                {
                    switch (Rand.Next(3))
                    {
                        case 0:
                            yearStr = "A year later, ";
                            break;
                        case 1:
                            yearStr = "One year later, ";
                            break;
                        case 2:
                            yearStr = "It was a year later when ";
                            break;
                    }

                }
                else
                {
                    int n = year - lastYear;
                    switch (Rand.Next(2))
                    {
                        case 0:
                            yearStr = n + " years later, ";
                            break;
                        case 1:
                            yearStr = "After " + n + " long years, ";
                            break;
                      
                    }
                    
                    
                }

            }
            else
            {
                switch (Rand.Next(3))
                {
                    case 0:
                        yearStr = "In " + year + ", ";
                        break;
                    case 1:
                        yearStr = "In the year of " + year + ", ";
                        break;
                    case 2:
                        yearStr = "It was in " + year + " when ";
                        break;
                }
            }
            lastYear = year;
          
            events.Add(yearStr + text);
            Form1.instance.Log(yearStr + text);
            lastSubject = primarySubject;
        }
    }
}
