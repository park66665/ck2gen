using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class WarManager
    {
        public static WarManager instance = new WarManager();
    }

    class War
    {
        Script warScript;

        ScriptCommand nameCommand;
        ScriptScope casusBelli;

        ScriptScope startScope;
        ScriptScope endScope;
        private TitleParser targetTitle;
        public List<CharacterParser> Attackers = new List<CharacterParser>();
        public List<CharacterParser> Defenders = new List<CharacterParser>();
        public int startDate;
        public String StartDate { get; set; }
        public String EndDate { get; set; }

        public void Create(String name, String casusBelli, List<CharacterParser> attackers, List<CharacterParser> defenders,
            TitleParser title, int yearStart)
        {
            warScript = new Script();
            
            warScript.Root = new ScriptScope();
            warScript.Root.Do("name = \"" + name + "\"");
            Attackers.AddRange(attackers);
            Defenders.AddRange(defenders);

            startDate = yearStart;

            CreateCasusBelli(casusBelli, attackers[0], defenders[0], title, yearStart);
            CreateStart(attackers, defenders, yearStart);
        }

        private void CreateStart(List<CharacterParser> attackers, List<CharacterParser> defenders, int yearStart)
        {
            var scope = new ScriptScope(yearStart+".1.1");
            foreach (var characterParser in attackers)
            {
                scope.Add(new ScriptCommand("add_attacker", characterParser.ID, scope));

            }
            foreach (var characterParser in defenders)
            {
                scope.Add(new ScriptCommand("add_defender", characterParser.ID, scope));

            }

            warScript.Root.Add(scope);


        }

        private void CreateCasusBelli(string casusBelli, CharacterParser actor, CharacterParser recipient, TitleParser title, int yearStart)
        {
            var scope = new ScriptScope("casus_belli");
            scope.Add(new ScriptCommand("actor", actor.ID, scope));
            scope.Add(new ScriptCommand("recipient", recipient.ID, scope));
            scope.Add(new ScriptCommand("casus_belli", casusBelli, scope));
            scope.Add(new ScriptCommand("landed_title", title, scope));
            scope.Add(new ScriptCommand("date", yearStart.ToString()+".1.1", scope));

            warScript.Root.Add(scope);
        }
    }
}
