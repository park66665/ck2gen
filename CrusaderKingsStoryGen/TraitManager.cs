using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrusaderKingsStoryGen.Simulation;

namespace CrusaderKingsStoryGen
{
    class TraitManager
    {
        public static TraitManager instance = new TraitManager();

        public List<ScriptScope> opposites = new List<ScriptScope>();
        public void Init()
        {
            if (!Directory.Exists(Globals.ModDir + "gfx\\traits"))
                Directory.CreateDirectory(Globals.ModDir + "gfx\\traits");
            var files = Directory.GetFiles(Globals.ModDir + "gfx\\traits");
            foreach (var file in files)
            {
                File.Delete(file);
            }

            if (!Directory.Exists(Globals.ModDir + "common\\traits"))
                Directory.CreateDirectory(Globals.ModDir + "common\\traits");

       
            files = Directory.GetFiles(Globals.ModDir + "common\\traits");
            foreach (var file in files)
            {
                File.Delete(file);
            }

            files = Directory.GetFiles(Globals.GameDir + "common\\traits");
            foreach (var file in files)
            {
                if (file.EndsWith(".info"))
                    continue;

                Script s = ScriptLoader.instance.Load(file);

                loaded.Add(s);
                last = s.Root;
            }



        }

        private ScriptScope last;

        List<Script> loaded = new List<Script>();

        public void AddReligiousTraits(ReligionParser religion)
        {
            string relName = religion.Name;
            string relLangName = religion.LanguageName;
            last.Do(@"

                sympathy_" + religion.Name + @" = {
        opposites =  {
	        zealous
        }    
           
        potential = {
	        NOT = { religion_group = " + religion.Group.Name + @" }
        }

	        same_opinion_if_same_religion = 5

	        customizer = no
	        random = no

	        male_insult = INSULT_LOVER_OF_HEATHENS
	        female_insult = INSULT_LOVER_OF_HEATHENS
	        male_insult_adj = INSULT_UNPRINCIPLED
	        female_insult_adj = INSULT_UNPRINCIPLED
	        child_insult_adj = INSULT_STUPID");

            last.Do(@"

           secretly_" + religion.Name + @" = {
	                is_visible = {
		                OR = {
			                character = FROM
			                society_member_of = secret_religious_society_" + religion.Name + @"
			                AND = {
				                is_close_relative = FROM
				                trait = secretly_" + religion.Name + @"
			                }
		                }
	                }

	                same_opinion = 15
	
	                random = no
	                customizer = no
	                ai_zeal = -50 
                }
            }"

);

            // TODO add great holy war-related traits, after adding generated great holy wars.

            ScriptScope c = (ScriptScope) last.Children[last.Children.Count - 1];
            var op = new ScriptScope("opposites");
            c.Add(op);
            opposites.Add(op);
             AddTrait("secretly_" + relName, "bastard.dds");
            AddTrait("sympathy_" + relName, "bastard.dds");
            
        }

        public void Save()
        {
            return;
            foreach (var scriptScope in opposites)
            {
                string name = scriptScope.Parent.Name;
                foreach (var opposite in opposites)
                {
                    string oname = opposite.Parent.Name;
                    if (name != oname)
                    {
                        scriptScope.Add(oname);
                    }
                }
            }

            foreach (var script in this.loaded)
            {
                script.Save();
            }
        }

        public void AddTrait(String name, String srcFilename)
        {
            String srcend = srcFilename.Substring(srcFilename.LastIndexOf('.'));
            //File.Copy(Globals.SrcTraitIconDir+srcFilename, Globals.ModDir + "gfx\\traits\\" + name + srcend);
            SpriteManager.instance.AddTraitSprite(name, "gfx/traits/" + name + srcend);
        }

        public void AddTrait(string safeName)
        {
             var files = Directory.GetFiles(Globals.SrcTraitIconDir);


            String s = files[Rand.Next(files.Length)];

            String srcend = s.Substring(s.LastIndexOf('\\')+1);
            AddTrait(safeName, srcend);
      
        }

        public void FillCharacter(CharacterParser characterParser, CharacterParser.PrimaryTrait trait)
        {
            List<string> exclude = new List<string>();
            List<ScriptScope> choicesL = new List<ScriptScope>();
            Dictionary<string, ScriptScope> choices = new Dictionary<string, ScriptScope>();
            Dictionary<string, ScriptScope> choicesEducation = new Dictionary<string, ScriptScope>();

            foreach (var script in loaded)
            {
                foreach (var rootChild in script.Root.Children)
                {
                    ScriptScope testTrait = rootChild as ScriptScope;

                    if (testTrait.TestBoolean("education"))
                        choicesEducation[testTrait.Name] = (testTrait);
                    else
                    {
                        choices[testTrait.Name] = (testTrait);
                        choicesL.Add(testTrait);
                    }

                }
                break;
            }

            List<ScriptScope> eduChoices = new List<ScriptScope>();

            foreach (var scriptScope in choicesEducation)
            {
                string eduTrait = scriptScope.Value.GetString("attribute");

                string test = trait.ToString();

                if (test == eduTrait)
                {
                    eduChoices.Add(scriptScope.Value);
                }
            }

            var choice = eduChoices[Rand.Next(eduChoices.Count)];
            characterParser.Scope.Add(new ScriptCommand("trait", choice.Name, characterParser.Scope));

            exclude.Add("twin");
            int numTraits = Rand.Next(3, 8);

            for (int x = 0; x < numTraits; x++)
            {
                var ch = choicesL[Rand.Next(choices.Count)];

                if (exclude.Contains(ch.Name) || ch.TestBoolean("is_illness"))
                {
                    x--;
                    continue;
                }

                if (ch.ChildrenMap.ContainsKey("opposites"))
                {
                    var opp = ch.ChildrenMap["opposites"] as ScriptScope;
                    var split = opp.Data.Split(' ');
                    foreach (var s in split)
                    {
                        exclude.Add(s.Trim());
                    }
                }

                characterParser.Scope.Add(new ScriptCommand("trait", ch.Name, characterParser.Scope));
            }
        }
    }
}
