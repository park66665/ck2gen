using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class Dynasty
    {
        public int ID;
        public List<CharacterParser> Members = new List<CharacterParser>();
        public ScriptScope Scope;
        public TitleParser palace;
        private string _name;
        public Color Color { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                (NameScope as ScriptCommand).Value = value;
            }
        }

        public ScriptCommand NameScope { get; set; }
    }
    class DynastyManager
    {
        public static DynastyManager instance = new DynastyManager();
        public int ID = 1;
        public void Init()
        {
            Script s = new Script();
            script = s;
            s.Name = Globals.ModDir + "common\\dynasties\\dynasties.txt";
            s.Root = new ScriptScope();
     
        }
        public Dictionary<int, Dynasty> DynastyMap = new Dictionary<int, Dynasty>();
        public void Save()
        {
            script.Save();
        }

        public Dynasty GetDynasty(int id, string name, string culture, string religion, ScriptScope scope)
        {
            
            ID = id;
            Name = name;
            var nameScope = scope.ChildrenMap["name"] as ScriptCommand;

            script.Root.Add(scope);
            var d = new Dynasty() { ID = ID, Scope = scope, NameScope = nameScope };
            d.Color = Color.FromArgb(255, Rand.Next(200) + 55, Rand.Next(200) + 55, Rand.Next(200) + 55);
            DynastyMap[ID] = d;
            return d;
        }
        public Dynasty GetDynasty(CultureParser culture)
        {
            ScriptScope scope = new ScriptScope();
            scope.Name = ID.ToString();
            ID++;
            do
            {
                Name = culture.dna.GetDynastyName();
            } while (Name == null || Name.Trim().Length==0);

            var nameScope = new ScriptCommand("name", Name, scope);

            scope.Add(nameScope);
            scope.Add(new ScriptCommand("culture", culture.Name, scope));
            script.Root.Add(scope);
            var d = new Dynasty() {ID = ID - 1, Scope = scope, NameScope = nameScope};
            d.Color = Color.FromArgb(255, Rand.Next(200) + 55, Rand.Next(200) + 55, Rand.Next(200) + 55);
            DynastyMap[ID - 1] = d;
            culture.Dynasties.Add(d);
            return d;
        }

        public string Name { get; set; }

        public Script script { get; set; }

        public void LoadVanilla()
        {
            var files = ModManager.instance.GetFileKeys("common\\dynasties");
            foreach (var file in files)
            {
                Script s = ScriptLoader.instance.LoadKey(file);
                foreach (var rootChild in s.Root.Children)
                {
                    var scope = rootChild as ScriptScope;

                    int id = Convert.ToInt32(scope.Name);

                    String name = scope.GetString("name");
                    String culture = scope.GetString("culture");
                    String religion = scope.GetString("religion");
                    Dynasty d = GetDynasty(id, name, culture, religion, scope);
                                    
                }
            }

        }

    }
}
