using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    class TechnologyGroup
    {
        public ScriptScope Scope { get; set; }

        public void Init()
        {
            Scope = new ScriptScope("technology");
            Titles = new ScriptScope("titles");

            Scope.Add(Titles);
        }

        public ScriptScope Titles { get; set; }

        public void AddTitle(String title)
        {
            Titles.Add(title);
        }

        public void AddDated(int date, ScriptCommand command)
        {
            ScriptScope dates = null;
            if (Date.ContainsKey(date))
            {
                dates = Date[date];
            }
            else
            {
                dates = new ScriptScope(date.ToString());
                Date[date] = dates;
                Scope.Add(dates);
            }
            
            dates.Add(command);
        }


        public Dictionary<int, ScriptScope> Date = new Dictionary<int, ScriptScope>();
    }
}
