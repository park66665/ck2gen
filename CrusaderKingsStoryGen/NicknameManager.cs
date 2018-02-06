using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class NicknameManager
    {
        public static NicknameManager instance = new NicknameManager();

        private bool init = false;
        List<String> nicks = new List<string>(); 
        public void Init()
        {
            Script s = ScriptLoader.instance.Load(Globals.GameDir + "common\\nicknames\\00_nicknames.txt");
        
            foreach (var o in s.Root.ChildrenMap)
            {
                String name = o.Key;
                nicks.Add(name);
                init = true;
            }

            nicks.Remove("nick_the_master_of_hungary");
            nicks.Remove("nick_the_master_of_hungary");
        }

        public String getNick(CharacterParser chr)
        {
            if (!init)
                Init();

            return nicks[Rand.Next(nicks.Count)];
        }
    }
}
