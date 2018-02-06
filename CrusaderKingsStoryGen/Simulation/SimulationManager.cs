using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using CrusaderKingsStoryGen.Story;
using static CrusaderKingsStoryGen.TitleManager;

namespace CrusaderKingsStoryGen.Simulation
{
    class SimulationManager
    {
        public static SimulationManager instance = new SimulationManager();
        public List<CharacterParser> characters = new List<CharacterParser>();
        public int PreYear = 0;
        public int MaxYear = 400;//1337;
        public static int StartYear = 200;
        public int Year = SimulationManager.StartYear;
        public bool Active { get; set; }
        public bool AutoValidateRealm { get; set; } = true;
        public bool AllowCapitalPicking { get; set; } = true;

        public HashSet<TitleParser> DestroyedTitles = new HashSet<TitleParser>();

        public void Init()
        {
            DynastyManager.instance.Init();
            CharacterManager.instance.Init();
            CulturalDnaManager.instance.Init();
            CultureManager.instance.Init();
            EventManager.instance.Load();
            DecisionManager.instance.Load();

            TraitManager.instance.Init();
            SpriteManager.instance.Init();
            foreach (var titleParser in TitleManager.instance.Titles)
            {
                titleParser.DoCapital();
            }
            ReligionManager.instance.Init();
            CharacterParser chr = CharacterManager.instance.GetNewCharacter();
            characters.Add(chr);

            ScriptScope s = new ScriptScope();
            string name = chr.Culture.dna.GetPlaceName();

            s.Name = StarNames.SafeName(name);
            LanguageManager.instance.Add(s.Name, name);
            //872 vanilla

            MapManager.instance.ProvinceIDMap[Globals.StartProvinceID].RenameForCulture(chr.Culture);

            foreach (var provinceParser in MapManager.instance.Provinces)
            {
                if (!provinceParser.land)
                {
                    provinceParser.RenameForCulture(chr.Culture);

                }
            }
            var tit = MapManager.instance.ProvinceIDMap[Globals.StartProvinceID].CreateTitle();
                
            chr.GiveTitle(tit);
            MapManager.instance.ProvinceIDMap[Globals.StartProvinceID].CreateProvinceDetails(chr.Culture);
          
        }

        public CharacterParser AddCharacterForTitle(TitleParser title, String culture, String religion)
        {
            CharacterParser chr = CharacterManager.instance.CreateNewCharacter(culture, religion, Rand.Next(8) == 0);
            title.Log("Given to " + chr.ID + " via AddCharacterForTitle");
            chr.GiveTitle(title);
            if (title.Rank > 0)
                characters.Add(chr);

            return chr;
        }
        public CharacterParser AddCharacter(String culture, String religion)
        {
            CharacterParser chr = CharacterManager.instance.CreateNewCharacter(culture, religion, Rand.Next(8) == 0);

            return chr;
        }

        public CharacterParser AddCharacterForTitle(TitleParser title, bool adult = false, bool doLaws = true, Dynasty dynasty = null)
        {
            CharacterParser chr = CharacterManager.instance.GetNewCharacter();
            chr.Dynasty = dynasty;
            title.Log("Given to " + chr.ID + " via AddCharacterForTitle");
            chr.GiveTitle(title, doLaws);
            if (title.Rank > 0)
                characters.Add(chr);

            return chr;
        }

        private static void FixupDuchies()
        {
            List<TitleParser> orphans = new List<TitleParser>();
            foreach (var titleParser in TitleManager.instance.Titles)
            {
                if (titleParser.Liege != null && titleParser.Liege.Rank == titleParser.Rank)
                {
                    titleParser.Liege.RemoveVassal(titleParser);
                    titleParser.DoSetLiegeEvent(null);
                }

                if (titleParser.Rank == 1 && titleParser.Liege == null)
                {
                 
                    orphans.Add(titleParser);
                }

            }

            for (int index = 0; index < orphans.Count; index++)
            {
                var titleParser = orphans[index];
                int smallest = 10000000;
                TitleParser title = null;
                foreach (var provinceParser in titleParser.Owns[0].Adjacent)
                {
                    if (provinceParser.title == null)
                        continue;

                    if (provinceParser.Title.Liege != null)
                    {
                        var liege = provinceParser.Title.Liege;

                        int c = liege.SubTitles.Count;
                        if (smallest > c)
                        {
                            smallest = c;
                            title = liege;
                        }
                    }
                }

                if (title != null)
                {
                    title.AddSub(titleParser);
                    orphans.Remove(titleParser);
                    index--;
                }
            }
            orphans.Clear();
            foreach (var titleParser in TitleManager.instance.Titles)
            {

                if (titleParser.Rank == 2)
                {
                    List<TitleParser> titles = new List<TitleParser>(titleParser.SubTitles.Values);
                    for (int index = 0; index < titles.Count; index++)
                    {
                        var value = titles[index];
                        for (int i = index + 1; i < titles.Count; i++)
                        {
                            var value2 = titles[i];
                            if (value.Owns.Count == 0)
                                continue;

                            if (!value.Adjacent(value2))
                            {
                                if (!orphans.Contains(value2))
                                    orphans.Add(value2);
                                continue;
                            }
                        }
                    }
                }
            }
            for (int index = 0; index < orphans.Count; index++)
            {
                var titleParser = orphans[index];
                int smallest = 10000000;
                TitleParser title = null;
                foreach (var provinceParser in titleParser.Owns[0].Adjacent)
                {
                    if (provinceParser.title == null)
                        continue;

                    if (provinceParser.Title.Liege != null)
                    {
                        var liege = provinceParser.Title.Liege;

                        int c = liege.SubTitles.Count;
                        if (smallest > c)
                        {
                            smallest = c;
                            title = liege;
                        }
                    }
                }

                if (title != null)
                {
                    title.AddSub(titleParser);
                    orphans.Remove(titleParser);
                    index--;
                }
            }

            orphans.Clear();

            List<TitleParser> newOrphans = new List<TitleParser>();
            foreach (var titleParser in TitleManager.instance.Titles)
            {
                if (titleParser.Rank == 2)
                {
                    if (titleParser.SubTitles.Count > 0)
                    {
                        orphans.Add(titleParser);
                    }
                }
                if (titleParser.Rank == 1)
                {
                    if (titleParser.Liege == null)
                    {
                        newOrphans.Add(titleParser);
                    }
                }
            }

            foreach (var titleParser in newOrphans)
            {
                var duke = TitleManager.instance.CreateDukeScriptScope(titleParser.Owns[0]);
                duke.AddSub(titleParser);
                titleParser.Holder.GiveTitle(duke);
                orphans.Add(duke);
            }

            // now wo do the regions...
            List<TitleParser> forRegion = new List<TitleParser>();
            for (int index = 0; index < orphans.Count; index++)
            {
                var titleParser = orphans[index];
                int kingSize = Rand.Next(20 * 20);
                var results = titleParser.Holder.GetProvinceGroup(kingSize, null);

                foreach (var provinceParser in results)
                {
                    if (provinceParser.Title != null && provinceParser.Title.Liege != null &&
                        provinceParser.Title.Liege.Rank == 2)
                    {
                        if (!forRegion.Contains(provinceParser.Title.Liege))
                        {
                            forRegion.Add(provinceParser.Title.Liege);
                        }
                    }
                }

                foreach (var parser in forRegion)
                {
                    if (orphans.Contains(parser))
                    {
                        int index2 = orphans.IndexOf(parser);

                        if (index2 <= index)
                            index--;
                        orphans.Remove(parser);
                    }

                }

                if (forRegion.Count == 0)
                    continue;

                String name = forRegion[0].Culture.dna.GetPlaceName();

               

                RegionManager.instance.AddRegion(name, forRegion);
                forRegion.Clear();
            }

            foreach (var titleParser in TitleManager.instance.Titles)
            {
                if (titleParser.Active && titleParser.Rank == 2)
                {
                    if (titleParser.Holder == null)
                    {
                        var chr = CharacterManager.instance.CreateNewCharacter(null, false,
                            SimulationManager.instance.Year - 16, titleParser.CapitalProvince.Religion.Name,
                            titleParser.CapitalProvince.Culture.Name);
                        chr.GiveTitle(titleParser);
                    }
                    foreach (var value in titleParser.SubTitles.Values)
                    {
                        titleParser.Holder.GiveTitle(value);
                    }
                    titleParser.DejureSub = new Dictionary<string, TitleParser>(titleParser.SubTitles);
                }
            }
        }

        private void CreateKingdomsFromDuchies()
        {
            List<TitleParser> duchies = new List<TitleParser>();
            List<TitleParser> done = new List<TitleParser>();

            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 2)
                {
                    duchies.Add(titleParser);
                }
            }
            for (int index = 0; index < duchies.Count; index++)
            {
                var duchie = duchies[index];
                int kingSize = Rand.Next(15, 24);
                var results = duchie.Holder.GetProvinceGroup(kingSize, null);
          
                List<TitleParser> kingdomDuchies = new List<TitleParser>();
                foreach (var provinceParser in results)
                {
                    if (!provinceParser.land)
                        continue;
                    if (provinceParser.title == null)
                        continue;

                    if (provinceParser.Title.Liege != null)
                    {
                        var liege = provinceParser.Title.Liege;

                        if (liege.Holder != null && liege.Rank == 2 && liege.Holder.PrimaryTitle.Rank == 2)
                        {
                            bool valid = false;

                            foreach (var kingdomDuchy in kingdomDuchies)
                            {
                                if (liege.IsAdjacent(kingdomDuchy, 2))
                                {
                                    valid = true;
                                    break;
                                }

                            }
                            if (kingdomDuchies.Count == 0)
                                valid = true;

                            if (valid && !kingdomDuchies.Contains(liege) && !done.Contains(liege))
                            {
                                if (liege.Name == "d_kasima")
                                {
                                    
                                }
                                kingdomDuchies.Add(liege);
                            }
                        }
                    }
                }

                if (kingdomDuchies.Count < 3)
                    continue;

                done.AddRange(kingdomDuchies);
                var chr = SimulationManager.instance.AddCharacter(duchie.Holder.culture, duchie.Holder.religion);
                var title = TitleManager.instance.CreateKingScriptScope(duchie.CapitalProvince, chr);
                chr.GiveTitle(title);

                foreach (var kingdomDuchy in kingdomDuchies)
                {
                    title.AddSub(kingdomDuchy);
                }

                var list = title.GetAllProvinces();

                int num = Math.Min(5, list.Count / 2);
              //  chr.GiveTitleSoft(list[0].Title.Liege);
                foreach (var value in list[0].Title.Liege.SubTitles.Values)
                {
                    if (value.Name == "d_kasima")
                    {

                    }
                 //   chr.GiveTitleSoft(value);
                 //   foreach (var titleParser in value.SubTitles)
                    {
                   //     if (titleParser.Value == duchie)
                   //         chr.GiveTitleSoftPlusAllLower(titleParser.Value, null);
                   //     else
                    //        chr.GiveTitleSoft(titleParser.Value);
                   //     break;
                    }

                }

                title.DejureSub = new Dictionary<string, TitleParser>(title.SubTitles);
            }

            duchies.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 2 && titleParser.Liege == null && !titleParser.landless)
                {
                    duchies.Add(titleParser);
                }
            }

            for (int index = 0; index < duchies.Count; index++)
            {
                var dTitle = duchies[index];
                if (dTitle.Name == "d_kasima")
                {

                }
                List<TitleParser> adj = new List<TitleParser>();
                adj.Add(dTitle);
                foreach (var parser in duchies)
                {
                    if (parser.IsAdjacent(dTitle))
                    {
                        if(!adj.Contains(parser))
                            adj.Add(parser);
                    }
                }
                var kingdomDuchies = adj;

                if (kingdomDuchies.Count < 3)
                    continue;

                var duchie = adj[0];

                done.AddRange(kingdomDuchies);
                var chr = SimulationManager.instance.AddCharacter(duchie.Holder.culture, duchie.Holder.religion);
                var title = TitleManager.instance.CreateKingScriptScope(duchie.CapitalProvince, chr);
                chr.GiveTitle(title);

                foreach (var kingdomDuchy in kingdomDuchies)
                {
                    title.AddSub(kingdomDuchy);
                }

                var list = title.GetAllProvinces();

                int num = Math.Min(5, list.Count / 2);
           //     chr.GiveTitleSoft(list[0].Title.Liege);
                foreach (var value in list[0].Title.Liege.SubTitles.Values)
                {

                 //   chr.GiveTitleSoft(value);
                  //  foreach (var titleParser in value.SubTitles)
                    {
                 //       if (titleParser.Value == duchie)
                //            chr.GiveTitleSoftPlusAllLower(titleParser.Value, null);
               //         else
              //              chr.GiveTitleSoft(titleParser.Value);
             //           break;
                    }

                }

                title.DejureSub = new Dictionary<string, TitleParser>(title.SubTitles);
            }

            duchies.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 2 && titleParser.Liege == null && !titleParser.landless)
                {
                    duchies.Add(titleParser);
                }
            }

            var kingdoms = new List<TitleParser>();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 3 && !titleParser.landless)
                {
                    kingdoms.Add(titleParser);
                }
            }
            for (int index = 0; index < duchies.Count; index++)
            {
                var dTitle = duchies[index];
                if (dTitle.Name == "d_kasima")
                {

                }
                foreach (var titleParser in kingdoms)
                {
                    if (titleParser.Adjacent(dTitle))
                    {
                        titleParser.AddSub(dTitle);
                        titleParser.Holder.GiveTitleSoft(dTitle);
                        break;
                    }
                }
            }
            duchies.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 2 && titleParser.Liege == null && !titleParser.landless)
                {
                    duchies.Add(titleParser);
                }
            }

            for (int index = 0; index < duchies.Count; index++)
            {
                var dTitle = duchies[index];
                if (dTitle.Name == "d_kasima")
                {

                }
                foreach (var titleParser in kingdoms)
                {
                    if (titleParser.IsAdjacentSea(dTitle))
                    {
                        titleParser.AddSub(dTitle);
                        titleParser.Holder.GiveTitleSoft(dTitle);
                        break;
                    }
                }
            }
            duchies.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 2 && titleParser.Liege == null && !titleParser.landless)
                {
                    duchies.Add(titleParser);
                }
            }

            for (int index = 0; index < duchies.Count; index++)
            {
                var dTitle = duchies[index];
           
                foreach (var titleParser in kingdoms)
                {
                    if (titleParser.IsAdjacentSea2(dTitle))
                    {
                        titleParser.AddSub(dTitle);
                        titleParser.Holder.GiveTitleSoft(dTitle);
                        break;
                    }
                }
            }

            duchies.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 2 && titleParser.Liege == null && !titleParser.landless)
                {
                    duchies.Add(titleParser);
                }
            }

        }
        private void CreateEmpiresFromKingdoms()
        {
            List<TitleParser> kingdoms = new List<TitleParser>();
            List<TitleParser> done = new List<TitleParser>();

            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 3)
                {
                    kingdoms.Add(titleParser);
                }
            }
            for (int index = 0; index < kingdoms.Count; index++)
            {
                var duchie = kingdoms[index];
                int kingSize = 140;
                var results = duchie.Holder.GetProvinceGroup(kingSize, null);
             

                List<TitleParser> empireKingdoms = new List<TitleParser>();
                foreach (var provinceParser in results)
                {
                    if (!provinceParser.land)
                        continue;
                    if (provinceParser.title == null)
                        continue;

                    if (provinceParser.Title.Liege != null)
                    {
                        var liege = provinceParser.Title.Liege;
                        if (liege != null)
                        {
                            liege = liege.Liege;
                            if (liege == null)
                                continue;
                        }
                        if (liege.Holder != null && liege.Rank == 3 && liege.Holder.PrimaryTitle.Rank == 3)
                        {
                            bool valid = false;

                            foreach (var empireKingdom in empireKingdoms)
                            {
                                if (liege.IsAdjacent(empireKingdom, 2))
                                {
                                    valid = true;
                                    break;
                                }

                            }
                            if (empireKingdoms.Count == 0)
                                valid = true;

                            if (valid && !empireKingdoms.Contains(liege) && !done.Contains(liege))
                                empireKingdoms.Add(liege);
                        }
                    }
                }

                if (empireKingdoms.Count < 3)
                    continue;

                done.AddRange(empireKingdoms);
                  var chr = SimulationManager.instance.AddCharacter(duchie.Holder.culture, duchie.Holder.religion);
                var title = TitleManager.instance.CreateEmpireScriptScope(duchie.CapitalProvince, chr);
                //  chr.GiveTitle(title);

                foreach (var kingdomDuchy in empireKingdoms)
                {
                    title.AddSub(kingdomDuchy);
                }

        
                title.DejureSub = new Dictionary<string, TitleParser>(title.SubTitles);

            }

            kingdoms.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 3 && titleParser.Liege == null && !titleParser.landless)
                {
                    kingdoms.Add(titleParser);
                }
            }
           
            for (int index = 0; index < kingdoms.Count; index++)
            {
                var dTitle = kingdoms[index];
                List<TitleParser> adj = new List<TitleParser>();
                adj.Add(dTitle);
                foreach (var parser in kingdoms)
                {
                    if (parser.IsAdjacent(dTitle))
                    {
                        if (!adj.Contains(parser))
                            adj.Add(parser);
                    }
                }
                var kingdomDuchies = adj;

                if (kingdomDuchies.Count < 3)
                    continue;

                var duchie = adj[0];

                done.AddRange(kingdomDuchies);
                var chr = SimulationManager.instance.AddCharacter(duchie.Holder.culture, duchie.Holder.religion);
                var title = TitleManager.instance.CreateEmpireScriptScope(duchie.CapitalProvince, chr);
              //  chr.GiveTitle(title);

                foreach (var kingdomDuchy in kingdomDuchies)
                {
                    title.AddSub(kingdomDuchy);
                }

                title.DejureSub = new Dictionary<string, TitleParser>(title.SubTitles);
            }

            List<TitleParser> empires = new List<TitleParser>();
            empires.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 4 && !titleParser.landless && titleParser.SubTitles.Count > 0)
                {
                  
                    empires.Add(titleParser);
                    
                }
            }
        
            kingdoms.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 3 && titleParser.Liege == null && !titleParser.landless)
                {
                    kingdoms.Add(titleParser);
                }
            }
            for (int index = 0; index < kingdoms.Count; index++)
            {
                var dTitle = kingdoms[index];

                foreach (var titleParser in empires)
                {
                    titleParser.Active = false;
                    titleParser.Holder = null;

                    if (titleParser.Adjacent(dTitle))
                    {
                        titleParser.AddSub(dTitle);
                       // titleParser.Holder.GiveTitleSoft(dTitle);
                        break;
                    }
                }
            }

            kingdoms.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 3 && titleParser.Liege == null && !titleParser.landless)
                {
                    kingdoms.Add(titleParser);
                }
            }



            for (int index = 0; index < kingdoms.Count; index++)
            {
                var dTitle = kingdoms[index];
              
                foreach (var titleParser in empires)
                {
                    if (titleParser.IsAdjacentSea(dTitle))
                    {
                        if (titleParser.Name == "e_tasima")
                        {

                        }
                        titleParser.AddSub(dTitle);
                      //  titleParser.Holder.GiveTitleSoft(dTitle);
                        break;
                    }
                }
            }
            kingdoms.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 3 && titleParser.Liege == null && !titleParser.landless)
                {
                    kingdoms.Add(titleParser);
                }
            }

            for (int index = 0; index < kingdoms.Count; index++)
            {
                var dTitle = kingdoms[index];
               
                foreach (var titleParser in empires)
                {
                    if (titleParser.IsAdjacentSea2(dTitle))
                    {
                       
                        titleParser.AddSub(dTitle);
                      //  titleParser.Holder.GiveTitleSoft(dTitle);
                        break;
                    }
                }
            }

            empires.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 4 && !titleParser.landless && titleParser.SubTitles.Count > 0)
                {

                    empires.Add(titleParser);

                }
            }

            var empire = empires[Rand.Next(empires.Count)];
            {
                var chr = SimulationManager.instance.AddCharacter(empire.FindValidCulture(), empire.FindValidReligion());
                chr.GiveTitleSoft(empire);

            }
            kingdoms.Clear();
            for (int index = 0; index < TitleManager.instance.Titles.Count; index++)
            {
                var titleParser = TitleManager.instance.Titles[index];

                if (titleParser.Rank == 3 && !titleParser.landless)
                {
                    kingdoms.Add(titleParser);
                    if(titleParser.Liege != empire)
                        titleParser.DoSetLiegeEvent(null);
                    else
                    {
                       
                    }
                }
            }
          
        }

        public bool bDonePreStage1 = false;
        private bool bDonePreStage2 = false;

        public void TickSystem(bool manual = false)
        {
            if (!bDonePreStage2 && MapManager.instance.ToFill.Count == 0)
                bDonePreStage1 = true;

            if (!Active && !manual)
                return;

            bTicked = true;
            if (bDonePreStage1)
            {
                AutoValidateRealm = false;
                {
                    CharacterParser[] a = characters.ToArray();
                    for (int index = 0; index < a.Length; index++)
                    {
                        var characterParser = a[index];


                        if (characterParser.Titles.Count == 0)
                        {
                            continue;
                        }

                        if (characterParser.PrimaryTitle.Rank < 1)
                            continue;

                        characterParser.ConvertCountTitlesToDuchies();
                    }

                }

                FixupDuchies();

                CultureManager.instance.CalculateCulturesProper();

                try
                {
                    CreateKingdomsFromDuchies();
                    CreateEmpiresFromKingdoms();
                    AutoValidateRealm = true;
                    foreach (var title in TitleManager.instance.Titles)
                    {
                        if (title.Holder != null && title.SubTitles.Count > 0 && title.Holder.NumberofCountTitles == 0 && title.Rank > 1)
                        {
                            var sub = title.SubTitles.First().Value;

                            while (sub.Rank > 0)
                            {                                
                                title.Holder.GiveTitleSoft(sub);
                                sub = sub.SubTitles.First().Value;
                            }
                            
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                bDonePreStage1 = false;
                bDonePreStage2 = true;
                
            }
            
            if (bDonePreStage2)
            {     
                if (this.Year >= SimulationManager.instance.MaxYear)
                    return;
            
                if (this.Year%100 == 0 && Active)
                    CharacterManager.instance.DoPurge();
                try
                {
                    DoFullHistoryTick();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
           
            }
            else 
            {
                DoPrehistoryTick();

            }


        }

        private void DoPrehistoryTick()
        {
       
            int toFillCount = MapManager.instance.ToFill.Count;
            for (int n = 0; n < 100; n++)
            {
                int numTicking = 0;
                for (int x = 0; x < characters.Count; x++)
                {

                    CharacterParser character = characters[x];
                    if (character.Liege != null)
                        continue;
                    if (character.PrimaryTitle == null)
                        continue;
                    if (character.TickDisable)
                        continue;

                    if (character.PrimaryTitle.Government != null)
                    {
                        if (!character.PrimaryTitle.Government.cultureGroupAllow.Contains(character.Culture.Group.Name))
                        {
                            character.PrimaryTitle.Government.cultureGroupAllow.Add(character.Culture.Group.Name);
                        }               
                    }

                    character.UpdateCultural();
                    if (character.PrimaryTitle != null && character.PrimaryTitle.Liege == null)
                    {
                        if (!character.TickDisable)
                            numTicking++;

                        if (!character.TickDisable)
                        {
                            int prov = character.Titles.Count;
                            character.Tick();

                            if (prov == character.Titles.Count)
                                numTicking--;
                        }


                    }
                    float chanceOfRevolt = 1.0f;

                    chanceOfRevolt *= Globals.BaseChanceOfRevolt;
                    int i = character.NumberofCountTitles;
                    if (i == 0)
                        i++;
                    chanceOfRevolt /= i;
                    int c = character.NumberofCountTitles;
                
                    if (c == 0)
                        continue;

                    if (c < 10)
                        continue;
                    if (c > 150)
                        chanceOfRevolt /= 100;

                    if (Rand.Next((int)chanceOfRevolt) == 0)
                    {
                        HandleRevolt(character);
                    
                        character.TickDisable = true;

                    }
                    if (character.Titles.Count == 0 || character.bKill)
                    {
                        characters.Remove(character);
                        character.KillTitles();
                        x--;
                    }
                }
                PreYear++;
             

                if (numTicking == 0)
                {
                    for (int x = (int)(characters.Count / 1.5f); x < characters.Count; x++)
                    {
                        CharacterParser character = characters[characters.Count - 1 - x];
                        character.TickDisable = false;
                    }
              
                }
            }

            if (toFillCount == MapManager.instance.ToFill.Count && toFillCount > 0)
            {
                float smallestDist = 100000000;
                ProvinceParser smallest = null;

                var start = MapManager.instance.ProvinceIDMap[Globals.StartProvinceID];

                // first get closest to start place...

                for (int index = 0; index < MapManager.instance.ToFill.Count; index++)
                {
                    var provinceParser = MapManager.instance.ToFill[index];

                    if (provinceParser.title != null)
                    {
                        MapManager.instance.ToFill.Remove(provinceParser);
                        index--;
                        continue;
                    }
                    float dist = provinceParser.DistanceTo(start);

                    if (dist < smallestDist && provinceParser.title == null)
                    {
                        smallest = provinceParser;
                        smallestDist = dist;
                    }
                }
                smallestDist = 100000000;

                var chosen = smallest;
                smallest = null;
                if (chosen != null)
                {
                    foreach (var provinceParser in MapManager.instance.Filled)
                    {
                        float dist = provinceParser.DistanceTo(chosen);

                        if (dist < smallestDist)
                        {
                            smallest = provinceParser;
                            smallestDist = dist;

                        }
                    }
                }

                if (smallest != null)
                {
                    var c = CharacterManager.instance.GetNewCharacter();
                    c.culture = smallest.Title.culture;
                    c.religion = smallest.Title.Holder.religion;
                    characters.Add(c);

                    c.TakeProvinceOverseas(chosen);

                }
             
            }
        }

        public int NumberOfIndependentCounts = 0;
        public int NumberOfIndependentDukes = 0;
        public int NumberOfIndependentKings = 0;
        public int NumberOfIndependentEmpire = 0;
        public int LastNumberOfIndependentCounts = 0;
        public int LastNumberOfIndependentDukes = 0;
        public int LastNumberOfIndependentKings = 0;
        public int LastNumberOfIndependentEmpire = 0;
        public bool bTicked;
        private ProvinceParser testRangeCapital;
        private void DoFullHistoryTick()
        {
            NumberOfIndependentCounts = 0;
            NumberOfIndependentDukes = 0;
            NumberOfIndependentKings = 0;
            NumberOfIndependentEmpire = 0;
        
            var arr = TitleManager.instance.Titles.ToArray();

            List<TitleParser> potentialConquerers = new List<TitleParser>();
            List<TitleParser> potentialCollapsers = new List<TitleParser>();

            int latest = SimulationManager.StartYear;

            if(BookmarkManager.instance.ImportantYears.Count > 0)
                latest = BookmarkManager.instance.ImportantYears[BookmarkManager.instance.ImportantYears.Count - 1];

            for (int i = 0; i < TitleManager.instance.Titles.Count; i++)
            {
                var titlee = TitleManager.instance.Titles[i];
                if (titlee.Holder == null)
                    continue;

                if (titlee.SubTitles.Count == 0)
                {
                    titlee.Log("Destroyed due to flagged problem");
                    titlee.SoftDestroy();
                    continue;
                }

                if (titlee.Rank >= 3 && !titlee.Holder.IsMajorPlayer)
                {
                    var dejure = titlee.GetAllDejureProvinces();
                    var total = titlee.GetAllProvinces();
                    if (total.Count / (float)dejure.Count < 0.05f)
                    {
                        titlee.Holder.GetIslands();
                        dejure = titlee.GetAllDejureProvinces();
                        total = titlee.GetAllProvinces();
                        if (total.Count / (float) dejure.Count < 0.05f)
                        {
                            int num = titlee.Holder.GetAllProvincesReal().Count;

                            titlee.Log("Destroyed due to lost almost entire dejure");
                            titlee.SoftDestroy();
                            
                            continue;
                        }
                            
                    }
                }


           //     HandleDejure(titlee);
                /*
                                for (int index = 0; index < titlee.Holder.Titles.Count; index++)
                                {
                                    var titleParser = titlee.Holder.Titles[index];
                                    if (titleParser.TopmostTitle != titlee.TopmostTitle && titleParser.TopmostTitle.Holder != titlee.TopmostTitle.Holder)
                                    {
                                        var tt = titleParser;

                                        while (tt.Liege != null && tt.Liege != titleParser.TopmostTitle)
                                        {
                                            tt = tt.Liege;
                                        }
                                        var tt2 = titlee;

                                        while (tt2.Liege != null && tt2 != titlee.TopmostTitle)
                                        {
                                            tt2 = tt2.Liege;
                                        }
                                        tt2.TopmostTitle.Holder.GiveTitleSoft(tt);    
                                    }
                                }*/
            }

            if (Year > latest + 60)
            {
                if (Rand.Next(30) == 0)
                {
                    SimulationManager.instance.PickMajorEvent();
                    if(Rand.Next(20)==0)
                        BookmarkManager.instance.AddImportantYear(Year - 1);
                }
            }

          //  if (Year > latest + 5)
            {
                for(int x=0;x<1;x++)
                {                  
                    SimulationManager.instance.PickMinorEvent();
                }
            }


/*
            int useIdealEmpire = GenerationOptions.IdealIndependentEmpireCount;
            if (Year > 950)
                useIdealEmpire--;
            if (Year > 850)
                useIdealEmpire--;
        
            CalculateConquerersAndCollapsers(arr, useIdealEmpire, potentialCollapsers);

            TriggerConquerers(potentialConquerers);

           // if (SimulationManager.instance.AllowSimRevolt)
          //      CollapseTitles(potentialCollapsers);
          */
            foreach (var religiousTitle in TitleManager.instance.ReligiousTitles)
            {
                    CharacterParser chr = religiousTitle.Holder;

                if (chr == null)
                {
                    chr = SimulationManager.instance.AddCharacterForTitle(religiousTitle, true);
                    chr.religion = religiousTitle.religion;
                    chr.UpdateCultural();
                
                }

                TestDeath(chr);

            }

            foreach (var titleParser in arr)
            {


                if (DestroyedTitles.Contains(titleParser))
                {
                    if (titleParser.Holder != null)
                    {
                        titleParser.Holder = null;
                    }
                    continue;
                }


                if (titleParser.Holder != null)
                {
             
                    if (titleParser.heldDates.Count == 0)
                    {
                        titleParser.heldDates.Add(new TitleParser.HeldDate()
                        {
                            chrid = titleParser.Holder.ID,
                            date = SimulationManager.instance.Year
                        });
                    }
                    CharacterParser chr = titleParser.Holder;
                    var provs = titleParser.GetAllProvinces();
                    if (provs.Count == 0)
                    {
                        continue;
                    }

                    if (titleParser.Holder.IsAlive)
                    {
                        if (chr.PrimaryTitle != titleParser)
                            continue;
                        if (titleParser.Holder.ID == 83403)
                        {

                        }
                        if (titleParser.Name == "k_kalatva")
                        {
                            
                        }
                        if (titleParser.Rank == 0)
                            continue;

                        if (titleParser.Rank == 1 && chr.Liege != null)
                            continue;

                        if (titleParser.Religious)
                            continue;


                        if (Year % 20 == 0)
                        {
                            chr.Titles = chr.Titles.OrderBy(a => a.GetAllDejureProvinces().Count).Reverse().ToList();
                            foreach (var chrTitle in chr.Titles)
                            {
                                if (chr.PrimaryTitle.Rank == chrTitle.Rank)
                                {
                                    chr.PrimaryTitle = chrTitle;
                                    break;
                                }
                            }
                        }

                       
                        //                        RepairBrokenTitles(titleParser);

                        CorrectMissingCulture(chr);

                        SimulationCountRank(titleParser);

                      //  CorrectTopmostLiegeMismatch(chr);

                        GiveAwayBaronies(chr);

                        if (SimulationManager.instance.AllowSimConquer)
                            if (TestAssimilationToBigNeighbour(titleParser, chr)) continue;

                        if (chr.PrimaryTitle==null)
                            continue;

                        if (TestPrimaryBaronTitle(chr)) continue;

                           TestPropogateReligion(chr);
                             TestPropogateCulture(chr);
                        chr.Islands = chr.GetIslands();
                        TestCreateEmpire(chr);
    
                     //   DoTech(chr);

                        if (Year%10 == 0)
                        {
                         //   chr.PrimaryTitle.PickNewCapital();

                        }

                        HandleTitles(chr);
                        if (chr.PrimaryTitle == null)
                            continue;
                        chr.Islands = chr.GetIslands();
                        if (SimulationManager.instance.AllowSimRevolt && chr.Liege==null)
                            HandleBreakaways(chr);
                      //  HandleTech(chr);

                        if (chr.PrimaryTitle == null)
                            continue;
                  
                        DoMarriageSimulation(chr);
                        DoImportantCharacterSimulation(chr);
                        DoChildrenSimulation(chr, titleParser);
                        
                    }
                 
                    TestDeath(chr);
                }
            }
    
            Year++;
     
            LastNumberOfIndependentCounts = NumberOfIndependentCounts;
            LastNumberOfIndependentDukes = NumberOfIndependentDukes;
            LastNumberOfIndependentKings = NumberOfIndependentKings;
            LastNumberOfIndependentEmpire = NumberOfIndependentEmpire;

        }

        private void CalculateConquerersAndCollapsers(TitleParser[] arr, int useIdealEmpire, List<TitleParser> potentialCollapsers)
        {
            foreach (var titleParser in arr)
            {
                if (titleParser.Holder == null)
                    continue;

                var list = titleParser.SubTitles.Values.ToList();
                for (int index = 0; index < list.Count; index++)
                {
                    var titleParser2 = titleParser.SubTitles.Values.ToList()[index];
                    if (titleParser2.Rank >= titleParser.Rank)
                    {
                        titleParser2.DoSetLiegeEventDejure(null);
                    }
                }
                if (titleParser.Rank == 4)
                {
                    int chanceOfCollapse = 100;
                    switch (GenerationOptions.EmpireStability)
                    {
                        case 5:
                            chanceOfCollapse = 0;
                            break;
                        case 4:
                            chanceOfCollapse = 100;
                            break;
                        case 3:
                            chanceOfCollapse = 200;
                            break;
                        case 2:
                            chanceOfCollapse = 300;
                            break;
                        case 1:
                            chanceOfCollapse = 400;
                            break;
                        case 0:
                            chanceOfCollapse = 500;
                            break;
                    }
                    chanceOfCollapse *= 3;

                    float delta = titleParser.Holder.RealmSize/400.0f;
                    delta = 1.0f - delta;
                    if (delta < 0.4f)
                        delta = 0.4f;
                    chanceOfCollapse = (int) (chanceOfCollapse*delta);

                    if (Rand.Next(chanceOfCollapse) == 0 && !titleParser.Holder.IsMajorPlayer)
                    {
                        MajorCollapse(titleParser);
                    }
                }

                if (useIdealEmpire < LastNumberOfIndependentEmpire)
                {
                    if (titleParser.Holder != null)
                    {
                        if (titleParser.Liege == null && titleParser.Rank == 4)
                        {
                            if (Rand.Next(5) == 0 && !titleParser.Holder.IsMajorPlayer)
                            {
                                potentialCollapsers.Add(titleParser);
                            }
                        }
                    }
                }
                // Time to split up kingdoms
                if (useIdealEmpire >= LastNumberOfIndependentEmpire &&
                    GenerationOptions.IdealIndependentKingCount > LastNumberOfIndependentKings)
                {
                    if (titleParser.Holder != null)
                    {
                        if (titleParser.Liege == null && titleParser.Rank == 3 && titleParser.Holder.NumberofKingTitles > 1 &&
                            titleParser.Holder.PrimaryTitle != titleParser)
                        {
                            if (Rand.Next(5) == 0 && !titleParser.Holder.IsMajorPlayer)
                            {
                                potentialCollapsers.Add(titleParser);
                            }
                        }
                    }
                }
            }
        }

        private static void TriggerConquerers(List<TitleParser> potentialConquerers)
        {
            if (potentialConquerers.Count > 0)
            {
                var item = potentialConquerers[Rand.Next(potentialConquerers.Count)];
                item.Holder.IsConquerer = true;
            }
        }

        private void CollapseTitles(List<TitleParser> potentialCollapsers)
        {
            if (potentialCollapsers.Count > 0)
            {
                var item = potentialCollapsers[Rand.Next(potentialCollapsers.Count)];
                var chr = item.Holder;

                if (item.Holder != null && Rand.Next(180) == 0)
                {
                    item.Holder.IsConquerer = false;
                    if (item.Rank == 4 && !item.Holder.IsMajorPlayer)
                    {
                        var title = chr.PrimaryTitle;

                        MajorCollapse(title); //title.SoftDestroy();
                    }
                    else
                    {
                        if (item.Rank == 3 && LastNumberOfIndependentDukes < GenerationOptions.IdealIndependentDukeCount &&
                            !item.Holder.IsMajorPlayer)
                        {
                            var title = chr.PrimaryTitle;

                            MajorCollapse(title);
                        }
                        else
                        {
                            foreach (var titleParser in item.SubTitles)
                            {
                                if (titleParser.Value.Holder != null && titleParser.Value.Liege == item && titleParser.Value.Rank == item.Rank - 1)
                                {
                                    titleParser.Value.Holder.GiveTitleSoftPlusAllLower(item, item.Holder);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RepairBrokenTitles(TitleParser titleParser)
        {
            if (titleParser.government == "republic")
            {
                if (titleParser.Holder.Dynasty.palace == null)
                {
                    foreach (var parser in titleParser.palaces)
                    {
                        if (parser.Holder != null &&
                            parser.Holder.IsAlive)
                        {
                            parser.Holder.GiveTitleSoftPlusAllLower(titleParser, null);
                            parser.Holder.GiveTitleSoft(parser.PalaceLocation.Title);
                        }
                    }
                }
                else
                {
                    List<TitleParser> choices = new List<TitleParser>();
                    bool found = false;
                    foreach (var parser in titleParser.palaces)
                    {
                        if (parser.Holder != null &&
                            parser.Holder.IsAlive)
                        {
                            choices.Add(parser);
                            if (parser.Holder == titleParser.Holder)
                                found = true;
                        }
                    }
                    foreach (var parser in titleParser.palaces)
                    {
                        if (parser.Holder != null &&
                            !parser.Holder.IsAlive)
                        {
                            parser.Holder.Heir.GiveTitleSoft(parser);
                            choices.Add(parser);
                        }
                    }

                    if (!found)
                    {
                        var chosen = choices[Rand.Next(choices.Count)];
                        var chr2 = chosen.Holder;
                        chr2.GiveTitleSoftPlusAllLower(titleParser, null);
                        chr2.GiveTitleSoft(chosen.PalaceLocation.Title);
                    }
                }
            }
        }

        private static void CorrectMissingCulture(CharacterParser chr)
        {
            if (!CultureManager.instance.CultureMap.ContainsKey(chr.culture))
            {
                var prov = chr.GetAllProvinces();

                foreach (var provinceParser in prov)
                {
                    if (CultureManager.instance.CultureMap.ContainsKey(provinceParser.Culture.Name))
                    {
                        chr.culture = provinceParser.Culture.Name;
                        break;
                    }
                }
            }
        }

        private void SimulationCountRank(TitleParser titleParser)
        {
            if (titleParser.Liege == null)
            {
                if (titleParser.Rank == 1)
                    NumberOfIndependentCounts++;
                if (titleParser.Rank == 2)
                    NumberOfIndependentDukes++;
                if (titleParser.Rank == 3)
                    NumberOfIndependentKings++;
                if (titleParser.Rank == 4)
                    NumberOfIndependentEmpire++;
            }
        }

        private static void CorrectTopmostLiegeMismatch(CharacterParser chr)
        {
            return;

            for (var index = 0; index < chr.Titles.Count; index++)
            {
                var parser = chr.Titles[index];
                if (parser.TopmostTitle != chr.PrimaryTitle.TopmostTitle)
                {
                    if (chr.PrimaryTitle.Rank > parser.Rank)
                    {
                        parser.DoSetLiegeEvent(chr.PrimaryTitle);
                    }
                }
            }
        }

        private static void GiveAwayBaronies(CharacterParser chr)
        {
            for (var index = 0; index < chr.Titles.ToArray().Length; index++)
            {
                var parser = chr.Titles.ToArray()[index];
                if (parser.Rank == 0 && parser.government != "republicpalace")
                {
                    chr.NonRelatedHeir.GiveTitleSoft(parser);
                }
            }
        }

        private static bool TestAssimilationToBigNeighbour(TitleParser titleParser, CharacterParser chr)
        {
        //    return false;

            if (titleParser.Rank <= 1 && titleParser.Liege == null && titleParser.Holder != null && titleParser.Holder.TopLiegeCharacter.PrimaryTitle.Rank <= 2)
            {
                var provs = new List<ProvinceParser>();
                foreach (var parser in chr.Titles)
                {
                    var prov = parser.GetAllProvinces();
                    provs.AddRange(prov);
                }
                List<TitleParser> biggiesAround = new List<TitleParser>();
                foreach (var provinceParser in provs)
                {
                    foreach (var parser in provinceParser.Adjacent)
                    {
                        if (parser.title != null)
                        {
                            if (parser.Title == null)
                            {
                                continue;
                            }
                            if (parser.Title.TopmostTitle.Rank > titleParser.Holder.TopLiegeCharacter.PrimaryTitle.Rank)
                            {
                                if (!biggiesAround.Contains(parser.Title.TopmostTitle))
                                    biggiesAround.Add(parser.Title.TopmostTitle);
                            }
                        }
                    }
                }

                if (biggiesAround.Count == 1)
                {
                    if (Rand.Next(20) == 0)
                    {
                        var b = biggiesAround[Rand.Next(biggiesAround.Count)];
                        titleParser.Log("Assimilated by " + b);
                        titleParser.DoSetLiegeEvent(b);
                        return true;
                    }
                }
                else if (biggiesAround.Count > 1)
                {
                    if (Rand.Next(50) == 0)
                    {
                        var b = biggiesAround[Rand.Next(biggiesAround.Count)];
                        titleParser.Log("Assimilated by " + b);
                        titleParser.DoSetLiegeEvent(b);
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool TestPrimaryBaronTitle(CharacterParser chr)
        {
            if (chr.PrimaryTitle.Rank == 0)
            {
                chr.PrimaryTitle = null;
                foreach (var parser in chr.Titles)
                {
                    if (chr.PrimaryTitle == null || chr.PrimaryTitle.Rank < parser.Rank)
                        chr.PrimaryTitle = parser;
                }

                if (chr.PrimaryTitle.Rank == 0)
                    return true;
            }
            return false;
        }

        private static void TestPropogateReligion(CharacterParser chr)
        {
            if (!ReligionManager.instance.ReligionMap.ContainsKey(chr.religion))
            {
                if (chr.PrimaryTitle.CapitalProvince != null)
                {
                    if (
                        ReligionManager.instance.ReligionMap.ContainsKey(
                            chr.PrimaryTitle.CapitalProvince.Religion.Name))
                    {
                        chr.religion = chr.PrimaryTitle.CapitalProvince.Religion.Name;
                    }
                }
                List<string> choices = new List<string>();

                var prov = chr.GetAllProvinces();

                foreach (var provinceParser in prov)
                {
                    if (ReligionManager.instance.ReligionMap.ContainsKey(provinceParser.Religion.Name))
                    {
                        choices.Add(provinceParser.Religion.Name);
                    }
                }


                if (choices.Count > 0)
                {
                    chr.religion = choices[Rand.Next(choices.Count)];
                }
                else
                {
                    chr.religion =
                        ReligionManager.instance.AllReligions[
                            Rand.Next(ReligionManager.instance.AllReligions.Count)].Name;
                }
            }

            if(instance.AllowSimReligionSpread)
                if (chr.PrimaryTitle.Rank >= 2)
                {
                    //if (Rand.Next(5) == 0)
                    {
                        var provinces = chr.GetAllProvinces().Where(a=>a.Religion != chr.Religion).ToList();
                        provinces = provinces.OrderBy(a => a.DistanceTo(chr.PrimaryTitle.CapitalProvince)).ToList();

                        int i = 2;
                        if(Rand.Next(30)==0)
                            i = 10;
                        for (var index = 0; index < Math.Min(i, provinces.Count); index++)
                        {
                            var provinceParser = provinces[index];
                            if (Rand.Next(10) == 0)
                            {
                                provinceParser.Religion = chr.Religion;
                            }
                        }
                    }
                }
        }

        private static void TestPropogateCulture(CharacterParser chr)
        {
            if (!instance.AllowSimCultureSpread)
                return;

       
            if (chr.PrimaryTitle.Rank >= 3)
            {
                var provinces = chr.GetAllProvinces().Where(a => a.Culture != chr.Culture).ToList();
                provinces = provinces.OrderBy(a => a.DistanceTo(chr.PrimaryTitle.CapitalProvince)).ToList();
                for (var index = 0; index < Math.Min(2, provinces.Count); index++)
                {
                    var provinceParser = provinces[index];
                    if (Rand.Next(70) == 0)
                    {
                        provinceParser.Culture = chr.Culture;
                    }
                }
            }
          
        }

        private static void TestCreateEmpire(CharacterParser chr)
        {
            var titles = chr.Titles;
            if (chr.PrimaryTitle.Name == "e_spain")
            {
                
            }
            for (var index = 0; index < titles.Count; index++)
            {                
                var titleParser = titles[index];
                if (titleParser.Dejure != null &&
                    (titleParser.Dejure.Holder == null))
                {
                    DoCreateDejureTitle(chr, titleParser, titles);
                   
                }

            }
        }

        private static void DoCreateDejureTitle(CharacterParser chr, TitleParser titleParser, List<TitleParser> titles)
        {
            int c = 0;
            List<TitleParser> has = new List<TitleParser>();
            if (titleParser.Dejure != null && titleParser.Dejure.Name == "e_spain")
            {

            }
            int tot = 0;
            int totBelow = 0;
            int belowc = 0;
            foreach (var parser in titleParser.Dejure.DejureSub)
            {
                var dejureProvs = parser.Value.GetAllDejureProvinces();
                foreach (var provinceParser in dejureProvs)
                {
                    bool b = provinceParser.Title.HasCharacterInChain(chr);
                    c += b ? 1 : 0;
                    tot++;
                    if (b)
                        has.Add(parser.Value);

                }

                if (parser.Value.HasCharacterInChain(chr))
                    belowc++;
                totBelow++;
            }

            float percent = c / (float)tot;

            bool bShouldMakeTitle = chr.PrimaryTitle.Rank < titleParser.Dejure.Rank;

            if (!bShouldMakeTitle)
            {
                // test if character should create title
                if (titleParser.Dejure.Rank == 3 && chr.PrimaryTitle.Rank == 3)
                    bShouldMakeTitle = true;
                if (titleParser.Dejure.Rank == 2 && chr.PrimaryTitle.Rank == 2)
                    bShouldMakeTitle = true;


            }

            float am = 0.51f;
            if (titleParser.Dejure.Rank == 4)
            {
                am = 0.80f;
                if (belowc < 2)
                    bShouldMakeTitle = false;
            }

            if (percent > am && bShouldMakeTitle && Rand.Next(50) == 0)
            {
                if (titleParser.Dejure.Name == "d_gelre")
                {
                }
                titleParser.Dejure.Log("Given to " + chr.ID + " in TestCreateEmpire");
                var prim = chr.PrimaryTitle;
                chr.GiveTitleSoft(titleParser.Dejure, true, true);
                if (chr.Liege != null)
                {
                    if (chr.Liege.PrimaryTitle.Rank > titleParser.Dejure.Rank)
                    {
                        titleParser.Dejure.DoSetLiegeEvent(chr.Liege.PrimaryTitle);
                    }
                }
                if (titleParser.Dejure.Rank > prim.Rank)
                {
                    prim.DoSetLiegeEvent(titleParser.Dejure);
                }


                titles.Add(titleParser.Dejure);
                if (titleParser.Dejure.Rank < chr.PrimaryTitle.Rank)
                {
                    titleParser.Dejure.Log("Usurped in title creation " + titleParser.Dejure + " for " +
                                           chr.PrimaryTitle);

                    titleParser.Dejure.DoSetLiegeEvent(chr.PrimaryTitle);
                }
                else
                {
                    titleParser.Dejure.Log("Made independent for title creation " + titleParser.Dejure + " for " +
                                           chr.PrimaryTitle);

                    titleParser.Dejure.DoSetLiegeEvent(null);
                }
                foreach (var parser in chr.Titles)
                {
                    if (parser.Rank < titleParser.Dejure.Rank)
                    {
                        parser.Log("Set liege on title creation " + titleParser.Dejure);
                        parser.DoSetLiegeEvent(titleParser.Dejure);

                    }
                 
                }
            }

            var de = titleParser.Dejure;

             if (de.Dejure != null &&
                (de.Dejure.Holder == null))
            {
                DoCreateDejureTitle(chr, de, titles);

            }
        }

        private void DoTech(CharacterParser chr)
        {
            return;

            if (Year < 800)
            {
                if (Year%5 == 0)
                {
                    chr.PrimaryTitle.DoTechPointTick();
                }
            }
            else
            {
                chr.PrimaryTitle.DoTechPointTick();
            }
        }

        private static void TestDeath(CharacterParser chr)
        {
// check if dies
            bool dies = false;
            if (chr.Age > 60)
            {
                if (Rand.Next(6) == 0)
                {
                    dies = true;
                }
            }
            else if (chr.Age > 50)
            {
                if (Rand.Next(12) == 0)
                {
                    dies = true;
                }
            }
            else if (chr.Age > 30)
            {
                if (Rand.Next(30) == 0)
                {
                    dies = true;
                }
            }
            else if (chr.Age > 20)
            {
                if (Rand.Next(40) == 0)
                {
                    dies = true;
                }
            }
            else
            {
                if (Rand.Next(30) == 0)
                {
                    dies = true;
                }
            }

            if (dies)
            {
                DoDeath(chr);
            }
        }

        private static void DoImportantCharacterSimulation(CharacterParser chr)
        {
            if (chr.PrimaryTitle == null)
            {
                
            }
            if (chr.IsMinorPlayer)
            {
                chr.DoMinorPlayer();
            }
            if (chr.IsMajorPlayer && Rand.Next(3)==0)
            {
                chr.DoMajorPlayer();
            }
        }

        private void DoChildrenSimulation(CharacterParser chr, TitleParser titleParser)
        {
            if (chr.Age >= 16)
            {
                CharacterParser fem = chr;
                if (!fem.isFemale)
                    fem = chr.CurrentSpouse;

                // check if has kid
                bool hasKid = false;
                if (fem?.Age < 30)
                {
                    if (Rand.Next(4) == 0)
                    {
                        hasKid = true;
                    }
                }
                else if (fem?.Age < 40)
                {
                    if (Rand.Next(10) == 0)
                    {
                        hasKid = true;
                    }
                }

                if (hasKid)
                {
                    if (chr.Kids.Count > 1)
                    {
                        if (Rand.Next(2) == 0)
                            hasKid = false;
                    }
                    if (chr.Kids.Count > 2)
                    {
                        if (Rand.Next(2) == 0)
                            hasKid = false;
                    }
                    if (chr.Kids.Count > 3)
                    {
                        if (Rand.Next(2) == 0)
                            hasKid = false;
                    }
                }


                if (hasKid)
                {
                    CharacterParser kid = CharacterManager.instance.CreateNewCharacter(chr.Dynasty,
                        Rand.Next(2) == 0, SimulationManager.instance.Year, titleParser.CapitalProvince.Religion.Name,
                        titleParser.CapitalProvince.Culture.Name);

                    if (!kid.isFemale)
                    {
                        int n = 60;
                        if (Year < 900)
                            n = 150;
                        if (Year < 800)
                            n = 350;
                        if (Year < 700)
                            n = 450;
                        if (Year < 600)
                            n = 750;

                        if (chr.PrimaryTitle.Rank == 4)
                            n = 4;
                    }
                    chr.Kids.Add(kid);
                    if (!chr.isFemale)
                    {
                        kid.Father = chr;
                        kid.Mother = chr.Spouses[chr.Spouses.Count - 1];
                    }
                    else
                    {
                        kid.Father = chr.Spouses[chr.Spouses.Count - 1];
                        kid.Mother = chr;
                    }
                    chr.CurrentSpouse?.Kids.Add(kid);

                    String name = null;
                    if (Rand.Next(2) == 0)
                    {
                        List<String> choices = new List<string>();

                        int cc = 0;
                        var last = chr;
                        while (chr != null && cc < 10)
                        {
                            if (last.Father != null && last.Father.HadTitle)
                                last = last.Father;
                            else if (last.Mother != null && last.Mother.HadTitle)
                                last = last.Mother;
                            else
                                break;

                            if (last.isFemale == kid.isFemale)
                                choices.Add(last.ChrName);
                            cc++;
                        }

                        if (kid.isFemale)
                        {
                            var m = kid.Father.Mother;
                            int c = 0;
                            while (m != null && c < 3)
                            {
                                if (!choices.Contains(m.ChrName))
                                    choices.Add(m.ChrName);
                                m = m.Mother;
                                c++;
                            }
                            m = kid.Mother.Mother;
                            c = 0;
                            while (m != null && c < 3)
                            {
                                if (!choices.Contains(m.ChrName))
                                    choices.Add(m.ChrName);
                                m = m.Mother;
                                c++;
                            }
                        }
                        else
                        {
                            var m = kid.Father.Father;
                            int c = 0;
                            while (m != null && c < 3)
                            {
                                if (!choices.Contains(m.ChrName) && m.ChrName != null)
                                    choices.Add(m.ChrName);
                                m = m.Father;
                                c++;
                            }
                            m = kid.Mother.Father;
                            c = 0;
                            while (m != null && c < 3)
                            {
                                if (!choices.Contains(m.ChrName) && m.ChrName != null)
                                    choices.Add(m.ChrName);
                                m = m.Father;
                                c++;
                            }
                        }

                        if (Rand.Next(3) == 0)
                        {
                            choices.AddRange(chr.PrimaryTitle.prevLeaderNames);
                        }

                        if (choices.Count > 4)
                        {
                            name = choices[Rand.Next(choices.Count)];
                            kid.ChrName = null;
                        }
                    }
                    kid.UpdateCultural(name);
                }
            }
        }

        private static void DoMarriageSimulation(CharacterParser chr)
        {
            if (chr.Age >= 16 && !chr.IsMarried)
            {
                CharacterParser spouse = null;

                if (Rand.Next(3) == 0)
                {
                    if (!chr.isFemale)
                    {
                        spouse = CharacterManager.instance.FindUnmarriedChildbearingAgeWomen(
                            SimulationManager.instance.Year, chr.religion, chr.culture);
                    }
                    else
                    {
                        spouse = CharacterManager.instance.FindUnlandedMan(
                            SimulationManager.instance.Year, chr.religion, chr.culture);
                    }
                }

                if (spouse == null)
                {
                    spouse = CharacterManager.instance.CreateNewCharacter(null, !chr.isFemale,
                        SimulationManager.instance.Year - 16, chr.religion,
                        chr.culture);
                }
                chr.Spouses.Add(spouse);
                spouse.Spouses.Add(chr);
                chr.AddDated(SimulationManager.instance.Year + ".1.1", "add_spouse=" + spouse.ID);
            }

            if (chr.CurrentSpouse != null)
            {
                if (chr.CurrentSpouse.PrimaryTitle == null)
                {
                    bool d = false;
                    if (chr.CurrentSpouse.Age > 60)
                    {
                        if (Rand.Next(4) == 0)
                        {
                            d = true;
                        }

                    }
                    else if (chr.CurrentSpouse.Age > 50)
                    {
                        if (Rand.Next(8) == 0)
                        {
                            d = true;
                        }

                    }
                    else if (chr.CurrentSpouse.Age > 30)
                    {
                        if (Rand.Next(12) == 0)
                        {
                            d = true;
                        }
                    }
                    else if (chr.CurrentSpouse.Age > 20)
                    {
                        if (Rand.Next(20) == 0)
                        {
                            d = true;
                        }
                    }
                    else
                    {
                        if (Rand.Next(30) == 0)
                        {
                            d = true;
                        }
                    }

                    if (d)
                    {
                        if (chr.CurrentSpouse.IsAlive)
                        {
                            chr.CurrentSpouse.YearOfDeath = SimulationManager.instance.Year;
                            CharacterManager.instance.AliveCharacters.Remove(chr.CurrentSpouse);
                        }
                    }

                }
            }
        }

        private void HandleTech(CharacterParser chr)
        {
            TechnologyManager.instance.HandleTech(chr);
        }

        private void PickMajorEvent()
        {
            switch (Rand.Next(2))
            {
                case 0:
                    MajorConquerer();
                    break;
                case 1:
                    MajorCollapse();

                    break;
            }

        }
        private void PickMinorEvent()
        {

            switch (Rand.Next(1))
            {
                case 0:
                    MinorWar();
                    break;
             
            }

        }

        private void MinorWar()
        {
            if (!AllowSimConquer)
                return;

            var list =
                    TitleManager.instance.Titles.Where(
                        t => t.Liege == null && t.Holder != null && t.Rank >= 1 && !t.Holder.IsMinorPlayer && !t.Holder.IsMajorPlayer && !t.Holder.IsMajorPlayer && t.Holder.PrimaryTitle == t)                        
                        .ToList();

            if (Rand.Next(4) != 0)
            {
                list =
                   TitleManager.instance.Titles.Where(
                       t => t.Liege == null && t.Holder != null && t.Rank >= 1 && t.Rank <= 2 && !t.Holder.IsMinorPlayer && !t.Holder.IsMajorPlayer && !t.Holder.IsMajorPlayer && t.Holder.PrimaryTitle == t)
                       .ToList();
            }
            if (list.Any())
            {
                var toWar = list[Rand.Next(list.Count)];

                if (toWar.Name == "d_northumberland")
                {
                    
                }
                List<TitleParser> choices = new List<TitleParser>();
                toWar.GetAllProvinces().ForEach(o=> o.Adjacent.Where(p => p.title != null && p.Title.TopmostTitle != toWar.TopmostTitle).ToList().ForEach(q=> choices.Add(q.Title.TopmostTitle)));

                choices = choices.Distinct().Where(p=>!toWar.Wars.Contains(p)).ToList();
                if (choices.Count == 0)
                    return;
                toWar.Wars.Add(choices[Rand.Next(choices.Count)]);
                toWar.Holder.IsMinorPlayer = true;
                toWar.Holder.Behaviour = "war";

            }
        }

        void MajorCollapse(TitleParser toDestroy=null)
        {
            if (!AllowSimRevolt)
                return;

            if (Rand.Next(5) != 0)
                return;

            if (toDestroy == null)
            {
                var list =
                    TitleManager.instance.Titles.Where(
                        t => t.Liege == null && t.Holder != null && t.Rank > 2 && t.GetAllProvinces().Count > 14 && !t.Holder.IsMinorPlayer && !t.Holder.IsMajorPlayer)
                        .OrderBy(p => p.GetAllProvinces().Count)
                        .Reverse()
                        .ToList();
                if (list.Any())
                {
                    toDestroy = list[Rand.Next(Math.Min(3, list.Count))];

                }
                else
                {
                    return;
                }
            }

            if (Rand.Next(5) == 0)
            {
                {

                    StoryManager.instance.CreateEvent(SimulationManager.instance.Year, "after the death of " + toDestroy.Holder.ChrName + ", the " + toDestroy.LangTitleName + "the once great " + toDestroy.LangRealmName + " finally collapsed after years of decline", toDestroy);
                    CharacterParser chr = toDestroy.Holder;
                    chr.RemoveTitleAddEvent(toDestroy);
                    toDestroy.Log("Destroyed in collapse");
                    toDestroy.SoftDestroy();
                    DoDeath(chr);
                    toDestroy.Holder = null;
                    if (toDestroy.Rank == 4 && Rand.Next(7) == 0)
                        BookmarkManager.instance.AddImportantYear(SimulationManager.instance.Year + 1);
                }
            }
            else
            {
                var choices = toDestroy.SubTitles.Where(a => a.Value.Rank == toDestroy.Rank - 1 && a.Value.Dejure != toDestroy).ToList();
                var choices2 = toDestroy.SubTitles.Where(a => a.Value.Rank == toDestroy.Rank - 1).ToList();

                var toMakeIndependent = 1;
                choices = choices.OrderBy(a => a.Value.CapitalProvince.DistanceTo(toDestroy.CapitalProvince)).Reverse().ToList();
                choices2 = choices2.OrderBy(a => a.Value.CapitalProvince.DistanceTo(toDestroy.CapitalProvince)).Reverse().ToList();
                if(choices2.Count > 0)
                    choices.Add(choices2[0]);
                toMakeIndependent = Math.Max(toMakeIndependent, 1);
                if (toDestroy.Rank > 3)
                {
                    
                }
                if (choices.Count > 0)
                {
                    for (int i = 0; i < toMakeIndependent; i++)
                    {
                        var ch = choices[i];
                        choices.RemoveAt(i);

                        ch.Value.DoSetLiegeEvent(null);
                        toMakeIndependent = Math.Min(choices.Count, toMakeIndependent);
                        i--;
                    }
                }

            }
        }
        void MajorConquerer()
        {
            var list =
                TitleManager.instance.Titles.Where(t => t.Holder!=null && t.Holder.PrimaryTitle == t && t.Holder.PrimaryTitle.Rank >= 1 && t.Holder.PrimaryTitle.Rank < 3  && !t.Holder.IsMinorPlayer)
                    .OrderBy(t => Rand.Next(10000000)).ToList();

            if (list.Any())
            {
                TitleParser title = list[Rand.Next(list.Count)];
                title.Holder.IsMajorPlayer = true;
                StoryManager.instance.CreateEvent(SimulationManager.instance.Year, title.Holder.ChrName + " the " + title.LangTitleName + " began " + (title.Holder.isFemale ? "her" : "his") + " great conquests", title);
                title.Holder.Behaviour = "conquerer";

                title.Log("Independent as conquerer");

                title.DoSetLiegeEvent(null);

            }
        }

        private void HandleBreakaways(CharacterParser chr)
        {
            /*       var titleParser = chr.PrimaryTitle;
                   for (int index = 0; index < chr.Titles.Count; index++)
                   {
                       var parser = chr.Titles[index];
                       if (parser.Rank == 2 && parser.government != "republic")
                       {
                           var provinces = parser.GetAllProvinces();
                           var surroundingTitles = new List<TitleParser>();

                           int sea = 0;
                           TitleParser highest = null;

                           var adj = new List<ProvinceParser>();
                           var adjsea = new List<ProvinceParser>();

                           provinces.ForEach(
                               p =>
                                   adj.AddRange(
                                       p.Adjacent.Where(
                                           pp =>
                                               pp.title != null && pp.Title.Holder != null &&
                                               pp.Title.Holder.TopLiegeCharacter != chr.TopLiegeCharacter)));

                           provinces.ForEach(p => adjsea.AddRange(p.Adjacent.Where(pp => !pp.land)));
                           foreach (var provinceParser in adj)
                           {
                               if (provinceParser.title != null &&
                                   !surroundingTitles.Contains(provinceParser.Title.TopmostTitle))
                               {
                                   surroundingTitles.Add(provinceParser.Title.TopmostTitle);
                                   if (highest == null || highest.Rank < provinceParser.Title.TopmostTitle.Rank)
                                       highest = provinceParser.Title.TopmostTitle;
                               }
                           }
                           //}

                           if (highest != null && highest.Rank > parser.Rank && surroundingTitles.Count == 1 &&
                               adjsea.Count==0)
                           {
                               var liegeTitle = highest.Holder.GetMostAppropriateTitleForLiege(titleParser);
                               if (parser.Holder.PrimaryTitle.Rank <= liegeTitle.Rank &&
                                   (parser.Liege == null || parser.Liege.Holder == null) && parser.Rank < liegeTitle.Rank)
                                   parser.DoSetLiegeEvent(liegeTitle);
                               else
                                   liegeTitle.Holder.GiveTitleSoftPlusAllLower(parser, parser.Holder);
                               return;
                           }
                       }
                       if (parser.Rank == 3 && parser.government != "republic")
                       {
                           var provinces = parser.GetAllProvinces();
                           var surroundingTitles = new List<TitleParser>();
                           var adjme = new List<ProvinceParser>();
                           int sea = 0;
                           TitleParser highest = null;

                           var adj = new List<ProvinceParser>();
                           var adjsea = new List<ProvinceParser>();

                           provinces.ForEach(
                               p =>
                                   adj.AddRange(
                                       p.Adjacent.Where(
                                           pp =>
                                               pp.title != null && pp.Title.Holder != null &&
                                               pp.Title.Holder.TopLiegeCharacter != chr.TopLiegeCharacter)));

                           provinces.ForEach(p => adjsea.AddRange(p.Adjacent.Where(pp => !pp.land)));

                           provinces.ForEach(p => adjme.AddRange(p.Adjacent.Where(pp => pp.Title != null && pp.Title.Holder != null && pp.Title.Holder.TopLiegeCharacter == chr.TopLiegeCharacter && !pp.Title.HasLiegeInChain(parser))));
                           foreach (var provinceParser in adj)
                           {
                               if (provinceParser.title != null &&
                                   !surroundingTitles.Contains(provinceParser.Title.TopmostTitle))
                               {
                                   surroundingTitles.Add(provinceParser.Title.TopmostTitle);
                                   if (highest == null || highest.Rank < provinceParser.Title.TopmostTitle.Rank)
                                       highest = provinceParser.Title.TopmostTitle;
                               }
                           }
                           //}

                           if (highest != null && highest.Rank > parser.Rank && surroundingTitles.Count <=2 &&
                               adjsea.Count == 0 && adjme.Count==0)
                           {
                               var liegeTitle = highest.Holder.GetMostAppropriateTitleForLiege(titleParser);
                               if (parser.Holder.PrimaryTitle.Rank <= liegeTitle.Rank &&
                                   (parser.Liege == null || parser.Liege.Holder == null) && parser.Rank < liegeTitle.Rank)
                                   parser.DoSetLiegeEvent(liegeTitle);
                               else
                                   liegeTitle.Holder.GiveTitleSoftPlusAllLower(parser, parser.Holder);
                               return;
                           }
                       }
                   }*/

            //return;
            if (chr.PrimaryTitle.Name == "e_spain")
            {

            }
            if (chr.Islands.Count > 1)
            {
                chr.GetIslands();

                if (chr.Islands.Count > 1 && chr.PrimaryTitle.Name == "e_spain")
                {
                    
                }
                List<CharacterParser.ProvinceIsland> seperateIslands = new List<CharacterParser.ProvinceIsland>();
                foreach (var provinceIsland in chr.Islands)
                {
                    if(!provinceIsland.Provinces.Contains(chr.PrimaryTitle.DejureCapitalProvince))
                        seperateIslands.Add(provinceIsland);
                }

                if (seperateIslands.Count == chr.Islands.Count)
                {
                    seperateIslands.Clear();
                    foreach (var provinceIsland in chr.Islands)
                    {
                        if (!provinceIsland.Provinces.Contains(chr.PrimaryTitle.CapitalProvince))
                            seperateIslands.Add(provinceIsland);
                    }
                }

                if (seperateIslands.Count > 0)
                {
                    //var seperateIsland = seperateIslands[Rand.Next(seperateIslands.Count)];

                    foreach (var seperateIsland in seperateIslands)
                    {
                     //   if (seperateIsland.Provinces.Count < chr.MainIsland.Provinces.Count)
                        {
                            // allow a single province gap...

                            bool oneAway = false;
                            if(!chr.IsMajorPlayer && ((chr.PrimaryTitle.Rank > 2 && seperateIsland.Provinces.Count >= 20) || (chr.PrimaryTitle.Rank > 3 && seperateIsland.Provinces.Count >= 30)))
                            foreach (var p in seperateIsland.Provinces)
                            {
                              
                                 
                                    if (p.Adjacent.Count(a => !a.land && !a.river) > 0 && chr.MainIsland.Provinces.Any(a=>a.Adjacent.Any(b=> !b.land && !b.river)))
                                        oneAway = true;
                            }

                            
                            if(!oneAway && !chr.IsMajorPlayer)
                            {

                                for (var index = 0; index < seperateIsland.Provinces.Count; index++)
                                {   
                                    var seperateIslandProvince = seperateIsland.Provinces[index];
                                    if (seperateIslandProvince.Title.Holder.TopLiegeCharacter == chr.TopLiegeCharacter)
                                    {
                                        var title = seperateIslandProvince.Title.FindTopmostTitleContainingAllProvinces(seperateIsland.Provinces, null, false);
                                       
                                        if (!title.GiveToNeighbour())
                                        {
                                           
                                            title.Log("Made independent due to being a seperate island");
                                            title.MakeIndependent();
                                        }

                                    }
                                }
                            }

                        }
                     
                    }
                }
            }
       
        }

#if DEBUG
        private void DebugBreak(TitleParser breakTest)
        {
            if (Form1.instance.SelectedTitleLast == breakTest)
            {
                Debugger.Break();
                Form1.instance.SelectedTitleLast = null;
            }
        }
#endif
        public void HandleDejure(TitleParser title)
        {
            debugtitle = "k_italy";
            debugtitle = "e_byzantium";
            // check dejure...

            foreach (var titleParser in title.DejureSub.Values)
            {
                var chr = titleParser.Holder;
                if (chr == null)
                    continue;

                if (debugtitle == titleParser.Name)
                {

                }
                if (titleParser.Rank > 1)
                {

                    if (titleParser.Rank > 2)
                    {
                    }

                    if (Rand.Next(4) == 0)
                    {
                        if (titleParser.Liege != null && titleParser.Dejure != titleParser.Liege)
                        {
                            if (titleParser.Holder != null)
                            {
                                if (titleParser.Holder.PrimaryTitle.Rank >= titleParser.Dejure.Rank)
                                {
                                    var toGiveTo = titleParser.GetRandomSubtitleHolder();
                                    if (toGiveTo != null)
                                    {
                                        titleParser.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                        toGiveTo.GiveTitleSoft(titleParser);
                                    }
                                    else
                                    {
                                        toGiveTo = titleParser.Holder.NonRelatedHeir;
                                        titleParser.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                        toGiveTo.GiveTitleSoft(titleParser);
                                    }
                                }
                            }


                            titleParser.Log("Set to dejure: " + titleParser.Dejure);
                            titleParser.DoSetLiegeEvent(titleParser.Dejure);

                        }
                    }

                    foreach (var subTitlesValue in titleParser.DejureSub.Values)
                    {
                        if (subTitlesValue.Holder != chr)
                        {
                            // if its in the same realm as me...
                            if (subTitlesValue.Holder != null &&
                                subTitlesValue.Holder.TopLiegeCharacter == chr.TopLiegeCharacter)
                            {
                                // if I'm not the liege of subtitle or don't hold it myself...
                                if (subTitlesValue.Dejure != null && subTitlesValue.Liege != subTitlesValue.Dejure)
                                {
                    //                if (Rand.Next(20) == 0)
                                    {
                                     /*   if (subTitlesValue.Holder.PrimaryTitle.Rank >= subTitlesValue.Dejure.Rank)
                                        {
                                            var toGiveTo = subTitlesValue.GetRandomSubtitleHolder();
                                            if (toGiveTo != null)
                                            {
                                                subTitlesValue.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                                toGiveTo.GiveTitleSoft(subTitlesValue);
                                            }
                                            else
                                            {
                                                toGiveTo = subTitlesValue.Holder.NonRelatedHeir;
                                                subTitlesValue.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                                toGiveTo.GiveTitleSoft(subTitlesValue);
                                            }
                                        }*/
                                        subTitlesValue.Log("Set to dejure: " + subTitlesValue.Dejure);
                                        subTitlesValue.DoSetLiegeEvent(subTitlesValue.Dejure);
                                    }
                                }
                            }
                            else if (subTitlesValue.Holder != null)
                            {
                                if (subTitlesValue.Liege == null)
                                {
                                  //  if (Rand.Next(20) == 0)
                                    {
                                        if (subTitlesValue.Holder.PrimaryTitle.Rank >= subTitlesValue.Dejure.Rank)
                                        {
                                            var toGiveTo = subTitlesValue.GetRandomSubtitleHolder();
                                            if (toGiveTo != null)
                                            {
                                                toGiveTo = subTitlesValue.Holder.NonRelatedHeir;
                                                subTitlesValue.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                                toGiveTo.GiveTitleSoft(subTitlesValue);
                                            }
                                            else
                                            {
                                                toGiveTo = subTitlesValue.Holder.NonRelatedHeir;
                                                subTitlesValue.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                                toGiveTo.GiveTitleSoft(subTitlesValue);
                                            }
                                        }
                                        {
                                            subTitlesValue.Log("Set to dejure: " + subTitlesValue.Dejure);
                                            subTitlesValue.DoSetLiegeEvent(subTitlesValue.Dejure);
                                        }
                                    }
                                }
                                if (subTitlesValue.Liege != null &&
                                    subTitlesValue.Holder.TopLiegeCharacter != chr.TopLiegeCharacter)
                                {
                                //    if (Rand.Next(20) == 0)
                                    {
                                        if (subTitlesValue.Holder.PrimaryTitle.Rank >= subTitlesValue.Dejure.Rank)
                                        {
                                            var toGiveTo = subTitlesValue.GetRandomSubtitleHolder();
                                            if (toGiveTo != null)
                                            {
                                                subTitlesValue.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                                toGiveTo.GiveTitleSoft(subTitlesValue);
                                            }
                                            else
                                            {
                                                toGiveTo = subTitlesValue.Holder.NonRelatedHeir;
                                                subTitlesValue.Log("Given to " + toGiveTo.ID + " in HandleDejure");
                                                toGiveTo.GiveTitleSoft(subTitlesValue);
                                            }
                                        }
                                        {
                                            subTitlesValue.Log("Set to dejure: " + subTitlesValue.Dejure);
                                            subTitlesValue.DoSetLiegeEvent(subTitlesValue.Dejure);
                                        }
                                    }
                                }

                            }
                        }

                    }

                }
            }
        }

        public bool AllowSimConquer = true;
        public bool AllowSimRevolt = true;
        public bool AllowSimBookmarks = true;
        public bool AllowSimReligionSpread = true;
        public bool AllowSimCultureSpread = true;
        public bool AllowCustomTitles = false;
        private static string debugtitle = "k_finland";
        private void HandleTitles(CharacterParser chr)
        {
            chr.Titles = chr.Titles.Distinct().ToList();

            if (chr.PrimaryTitle.Liege != null)
            {
                for (var index = 0; index < chr.Titles.Count; index++)
                {
                    var titleParser = chr.Titles[index];
                    if (titleParser.Liege != null && titleParser != chr.PrimaryTitle &&
                        titleParser.Rank <= chr.PrimaryTitle.Rank && titleParser.TopmostTitle != chr.PrimaryTitle.TopmostTitle)
                    {
                        if (titleParser.Name == "c_blois")
                        {
                            
                        }
                        titleParser.Log("Setting independent due to primary title being different liege");
                        titleParser.DoSetLiegeEvent(null);
                    }
                    else if (titleParser.Liege != null && titleParser != chr.PrimaryTitle &&
                             titleParser.Rank > chr.PrimaryTitle.Rank)
                    {
                        chr.PrimaryTitle = titleParser;
                        index = -1;
                    }
                }
            }
            if (chr.NumberofEmpireTitles > 1)
            {
                List<TitleParser> emps = chr.Titles.Where(titleParser => titleParser.Rank == 4).ToList();

                int biggestN = -1;
                TitleParser biggest = null;
                foreach (var titleParser in emps)
                {
                    var num = titleParser.GetAllDirectProvinces().Count;

                    if (num > biggestN)
                    {
                        biggest = titleParser;
                        biggestN = num;
                    }
                }

                emps.Remove(biggest);
                foreach (var titleParser in emps)
                {
                    var l = titleParser.SubTitles.Values.ToList();
                    for (var index = 0; index < l.Count; index++)
                    {
                        var titleParserSubTitle = l[index];
                        titleParserSubTitle.DoSetLiegeEvent(biggest);
                    }
                    titleParser.Log("Destroyed in handle titles owner owns two empires");
                    titleParser.SoftDestroy();
                }
            }
            for (var index = 0; index < chr.Titles.Count; index++)
            {
                var titleParser = chr.Titles[index];

                if (titleParser.FlagProblem)
                {
                    titleParser.Log("Destroyed due to no capital's obtainable");
                    titleParser.SoftDestroy();
                }
                if (titleParser != chr.PrimaryTitle)
                {
               
                    if (titleParser.Liege != null)
                    {
                        if (chr.PrimaryTitle.Liege == null && titleParser.Rank == chr.PrimaryTitle.Rank)
                        {
                            index = 0;
                            chr.PrimaryTitle = titleParser;
                            continue;
                        }
                        if (titleParser.Liege.Holder.TopLiegeCharacter != chr.TopLiegeCharacter)
                        {
                            if (titleParser.Rank < chr.PrimaryTitle.Rank)
                            {
                               
                                titleParser.Log("Assigned in handle titles: " + chr.PrimaryTitle);

                                titleParser.DoSetLiegeEvent(chr.PrimaryTitle);
                            }
                            else
                            {
                                if (chr.PrimaryTitle.Name == "c_gelre")
                                {

                                }
                                titleParser.Log("Independent in handle titles: " + chr.PrimaryTitle);
                                titleParser.DoSetLiegeEvent(null);
                            }
                        }
                    }
                }
            }
       
            int timeout = 20;
            while (chr.NumberofKingTitles >= 3 && chr.PrimaryTitle.Rank > 3 && !chr.IsMajorPlayer)
            {
                int was = chr.NumberofKingTitles;
                var choices = new List<TitleParser>();
                var capitalDuchy = chr.PrimaryTitle.CapitalProvince.Title.Liege;
                if (capitalDuchy != null && capitalDuchy.Liege != null)
                    capitalDuchy = capitalDuchy.Liege;

                choices.AddRange(chr.Titles.Where(t => t.Rank == 3 && t != capitalDuchy));

                var giveAway = choices[Rand.Next(choices.Count)];

                if (giveAway.CapitalProvince.Title.Liege != null && giveAway.CapitalProvince.Title.Liege.Holder != chr &&
                    giveAway.CapitalProvince.Title.Liege.Holder.PrimaryTitle.Rank == 2)
                {
                    giveAway.Log("Given to " + giveAway.CapitalProvince.Title.Liege.Holder.ID + " in HandleTitles - Too many king titles");

                    giveAway.CapitalProvince.Title.Liege.Holder.GiveTitleSoft(giveAway);
                }
                else
                {
                    var h = chr.NonRelatedHeir;
                    giveAway.CapitalProvince.Title.Log("Given to " + h.ID + " in HandleTitles - Too many king titles");
                    if(giveAway.CapitalProvince.Title.Liege != null)
                        giveAway.CapitalProvince.Title.Liege.Log("Given to " + h.ID + " in HandleTitles - Too many king titles");
                    giveAway.Log("Given to " + giveAway.CapitalProvince.Title.Holder + " in HandleTitles - Too many king titles");
                    h.GiveTitleSoft(giveAway.CapitalProvince.Title);
                    h.GiveTitleSoft(giveAway.CapitalProvince.Title.Liege);
                    giveAway.CapitalProvince.Title.Holder.GiveTitleSoft(giveAway);
                }

                foreach (var titleParser in giveAway.SubTitles)
                {
                    if (titleParser.Value.Holder == chr)
                    {
                        titleParser.Value.Log("Given to " + giveAway.Holder + " in HandleTitles - Too many king titles");
                        giveAway.Holder.GiveTitleSoft(titleParser.Value);
                    }
                }

                if (was == chr.NumberofKingTitles)
                {
                    timeout--;
                    if(timeout < 0)
                        break;
                }
                timeout--;
            }

            while (chr.NumberofDukeTitles >= 3 && chr.PrimaryTitle.Rank >= 3 && !chr.IsMajorPlayer)
            {
                // give away a duchy to a vassal...

                var choices = new List<TitleParser>();
                var capitalDuchy = chr.PrimaryTitle.CapitalProvince.Title.Liege;
                
                choices.AddRange(chr.Titles.Where(t => t.Rank == 2 && t != capitalDuchy && !t.FlagProblem));

                if (choices.Count == 0)
                {
                    break;
                }

                var giveAway = choices[Rand.Next(choices.Count)];
                {
                    var h = chr.NonRelatedHeir;
                    var cap = giveAway.CapitalProvince;
                    if (giveAway.FlagProblem)
                    {
                        giveAway.Log("Destroyed due to flagged problem");
                        giveAway.SoftDestroy();
                    }
                    else
                    {
                        giveAway.CapitalProvince.Title.Log("Given to " + h.ID + " in HandleTitles - Too many duke titles");
                        h.GiveTitleSoft(giveAway.CapitalProvince.Title);
                        if (!h.GiveTitleSoft(giveAway))
                        {
                            giveAway.CapitalProvince.Title.Log("Given to " + h.ID + " in HandleTitles - Too many duke titles");
                            h.GiveTitleSoft(giveAway);
                        }
                        if (giveAway.Dejure != null &&
                            (chr.Titles.Contains(giveAway.Dejure) || chr.Titles.Contains(giveAway.Dejure.Liege)))
                        {
                            giveAway.Log("Too many dukes, given to dejure: " + giveAway.Dejure);

                            giveAway.DoSetLiegeEvent(giveAway.Dejure);
                        }
                        else
                        {
                            giveAway.Log("Too many dukes, made direct title into vassal of " + chr.PrimaryTitle);

                            giveAway.DoSetLiegeEvent(chr.PrimaryTitle);
                        }
                    }
                 

       
                }
               
                foreach (var titleParser in giveAway.SubTitles)
                {
                    if (titleParser.Value.Holder == chr)
                    {
                        if (giveAway.Holder == null)
                        {
                            titleParser.Value.Holder.NonRelatedHeir.GiveTitleSoft(giveAway);
                            titleParser.Value.Log("Given to " + giveAway.Holder + " in HandleTitles - Too many duke titles");
                            
                            giveAway.Holder.GiveTitleSoft(titleParser.Value);

                        }
                        else
                        {
                            titleParser.Value.Log("Given to " + giveAway.Holder + " in HandleTitles - Too many duke titles");
                            giveAway.Holder.GiveTitleSoft(titleParser.Value);

                        }
                    }
                }
            }
            while (chr.NumberofDukeTitles >= 3 && chr.PrimaryTitle.Rank ==2 && chr.Titles.Any(t=>t.Liege != null && t.Rank == 2) && !chr.IsMajorPlayer)
            {
                // give away a duchy to a vassal...

                var choices = new List<TitleParser>();
                var capitalDuchy = chr.PrimaryTitle.CapitalProvince.Title.Liege;

                choices.AddRange(chr.Titles.Where(t => t.Rank == 2 && t != capitalDuchy));


                var giveAway = choices[Rand.Next(choices.Count)];
                {
                    var l = chr.Titles.Where(t => t.Liege != null && t.Rank == 2).ToList();
                    if (l.Count == 0)
                        break;
                    var liege = l[0].Liege;
                    var h = chr.NonRelatedHeir;
                    giveAway.CapitalProvince.Title.Log("Given to " + h.ID + " in HandleTitles - Too many duke titles");

                    h.GiveTitleSoft(giveAway.CapitalProvince.Title);
                    if (giveAway.CapitalProvince.Title.Holder != chr)
                    {
                        if (!liege.Holder.GiveTitleSoft(giveAway, false))
                        {
                            if (liege.Rank > giveAway.Rank)
                            {
                                giveAway.Log("Too many dukes, made direct title into vassal of " + liege);
                                giveAway.DoSetLiegeEvent(liege);
                                var h2 = chr.NonRelatedHeir;
                                giveAway.CapitalProvince.Title.Log("Given to " + h2.ID + " in HandleTitles - Too many duke titles");
                                h2.GiveTitleSoft(giveAway);

                            }
                        }
                        else
                        {
                            giveAway.Log("Too many dukes, made direct title into vassal of " + liege);
                            giveAway.DoSetLiegeEvent(liege);
                            chr.NonRelatedHeir.GiveTitleSoft(giveAway);

                        }
                    }
                    else 
                    {
                        if (!liege.Holder.GiveTitleSoft(giveAway, false))
                        {
                            if (liege.Rank > giveAway.Rank)
                            {
                                giveAway.Log("Too many dukes, made direct title into vassal of " + liege);
                                giveAway.DoSetLiegeEvent(liege);
                                chr.NonRelatedHeir.GiveTitleSoft(giveAway);
                            }

                        }
                        else
                        {
                            giveAway.Log("Too many dukes, made direct title into vassal of " + liege);
                            giveAway.DoSetLiegeEvent(liege);

                        }
                    }

                }
            
                foreach (var titleParser in giveAway.SubTitles)
                {
                    if (titleParser.Value.Holder == chr)
                    {
                        giveAway.Holder.GiveTitleSoft(titleParser.Value);
                    }
                }
            }
            while (chr.NumberofCountTitles > 5 && chr.PrimaryTitle.Rank > 1)
            {
                // give away a duchy to a vassal...

                var choices = new List<TitleParser>();
                var capitalDuchy = chr.PrimaryTitle.CapitalProvince.Title.Liege;

                choices.AddRange(chr.Titles.Where(t => t.Rank == 1 && t.Liege != capitalDuchy));
                if(choices.Count==0)
                    choices.AddRange(chr.Titles.Where(t => t.Rank == 1 && t.Owns.Count > 0 && (capitalDuchy == null || t.Owns[0] != capitalDuchy.CapitalProvince)));
                var choiceProvinces = new List<TitleParser>(choices);

                if (choiceProvinces.Count > 0)
                {
                    var giveAway = choiceProvinces[Rand.Next(choices.Count)];

                    if (chr.PrimaryTitle.Rank > 2 && giveAway.CapitalProvince.Title.Liege != null &&
                        giveAway.CapitalProvince.Title.Liege.Holder != chr)
                    {
                        if (!giveAway.CapitalProvince.Title.Liege.Holder.GiveTitleSoft(giveAway))
                        {
                            chr.NonRelatedHeir.GiveTitleSoftPlusOneLower(giveAway, null);
                        }
                    }
                    else
                    {
                        if (!giveAway.Holder.HasLiegeInChain(chr))
                        {
                            chr.NonRelatedHeir.GiveTitleSoft(giveAway, true);
                            giveAway.Log("Too many dukes, made direct title into vassal of " + chr.PrimaryTitle);
                            giveAway.DoSetLiegeEvent(chr.PrimaryTitle);
                        }
                        else
                        {

                            chr.NonRelatedHeir.GiveTitleSoft(giveAway);
                            if (!giveAway.Holder.HasLiegeInChain(chr))
                            {
                                giveAway.Log("Too many dukes, made direct title into vassal of " + chr.PrimaryTitle);

                                giveAway.DoSetLiegeEvent(chr.PrimaryTitle);
                            }
                        }
                    }


                }
             
            }

            for (var index = 0; index < chr.Titles.Count; index++)
            {
                var titleParser = chr.Titles[index];
                if (titleParser.Liege == null)
                {
                    if (titleParser.Name == "e_france")
                    {

                    }
                    SortVassals(titleParser, titleParser.Holder);
                }
            }
        }

        private void SortVassals(TitleParser title, CharacterParser holder)
        {
            if (title.Name == "c_maine")
            {

            }
            var ownedSubs = new List<TitleParser>();
            foreach (var titleParser in title.DejureSub)
            {
                if (titleParser.Value.HasCharacterInChain(holder))
                {
                    ownedSubs.Add(titleParser.Value);
                }
            }

            foreach (var titleParser in title.DejureSub)
            {
                if (ownedSubs.Contains(titleParser.Value))
                {
                //    if(titleParser.Value.Liege != title)
               //         titleParser.Value.DoSetLiegeEvent(title);

                    SortVassals(titleParser.Value, holder);
                }
            }
        }


        public static void DoDeath(CharacterParser chr)
        {
            CharacterManager.instance.AliveCharacters.Remove(chr);

            if (chr.IsAlive)
            {
                chr.YearOfDeath = SimulationManager.instance.Year;
                chr.UpdateCultural();
            }
            if (chr.PrimaryTitle == null)
                return;
            
            if(chr.Age > 35)
            {
                if (Rand.Next(2) == 0)
                {
                    chr.GiveNickname();
                }
            }
      
            var heir = chr.Heir;
            
            if (Rand.Next(60) == 0)
                heir = chr.NonRelatedHeir;

            if (chr.PrimaryTitle.Religious)
                heir = chr.NonRelatedHeir;

            if (chr.PrimaryTitle.government == "republic")
            {
                if (heir.Dynasty.palace != null)
                {
                    heir.GiveTitleSoft(heir.Dynasty.palace);
                }
            }
            var prim = chr.PrimaryTitle;
            var h = heir;
            foreach (var parser in chr.Titles.ToArray())
            {
                h.GiveTitleSoft(parser);
            }

            if(h.PrimaryTitle.Rank == prim.Rank)
                h.PrimaryTitle = prim;

        }

        private void HandleRevolt(CharacterParser character)
        {
           
            var pr = new List<ProvinceParser>();
            
            foreach (var title in character.Titles)
            {
                title.AddChildProvinces(pr);
                if (title.Owns.Count > 0)
                {
                    bool bDo = false;
                    foreach (var provinceParser in title.Owns[0].Adjacent)
                    {
                        if (provinceParser.land && (provinceParser.title == null || provinceParser.Title.Holder == null))
                        {
                            bDo = true;
                            break;
                        }
                    }
                    if (bDo)
                        pr.Add(title.Owns[0]);
                }
            }

            pr.Remove(character.PrimaryTitle.CapitalProvince);
            for (int index = 0; index < pr.Count; index++)
            {
                var provinceParser = pr[index];
                bool keep = false;
                foreach (var parser in provinceParser.Adjacent)
                {
                    if (parser.title == null)
                        continue;
                    ;
                    if (parser.Title.Holder == null)
                    {
                        keep = true;
                        break;
                    }
                }
                if (!keep)
                {
                    pr.Remove(provinceParser);
                    index--;
                }

            }
            if (pr.Count == 0)
                return;

            var p = pr[Rand.Next(pr.Count)];
            List<ProvinceParser> provinces = new List<ProvinceParser>();
            provinces.Add(p);
            while (provinces.Count < character.Titles.Count / 3)
            {
                MapManager.instance.FindAdjacent(provinces, provinces.Count / 3);
            }
            var ch = CharacterManager.instance.GetNewCharacter();
            characters.Add(ch);
            foreach (var provinceParser in provinces)
            {
                if (provinceParser.title != null)
                    ch.GiveTitle(TitleManager.instance.TitleMap[provinceParser.title]);
            }

            int nn = Globals.OneInChanceOfReligionSplinter;
            if (ReligionManager.instance.ReligionMap[character.religion].Provinces.Count > 100)
                nn--;
            if (ReligionManager.instance.ReligionMap[character.religion].Provinces.Count > 300)
                nn--;
            if (character.religion == "pagan")
                nn = 0;
          
        
            if (Rand.Next(Globals.OneInChanceOfCultureSplinter) == 0)
            {
                var oldGroup = character.Culture.Group;
                ch.culture = CultureManager.instance.BranchCulture(character.culture).Name;
                ReligionParser r = ReligionManager.instance.BranchReligion(character.religion, ch.culture);
                ch.Culture.Group.ReligionGroup = r.Group;
                ch.religion = r.Name;
                character.religion = ch.religion;
            
            }
        }


        public void DebugTest(TitleParser titleParser)
        {
            // test topmost title's provinces all share topmost title
            var title = titleParser.TopmostTitle;

            var prov = title.GetAllProvinces();

            foreach (var provinceParser in prov)
            {
                if (provinceParser.Title.TopmostTitle != title)
                {
                    var topmost = provinceParser.Title.TopmostTitle;
                }
            }
        }

        public void RunSimulationFromHistoryFiles(int endDate)
        {
           // Rand.SetSeed();
            AllowCapitalPicking = false;
            bDonePreStage2 = true;
            PreYear = 1;
            Year = endDate;
            AutoValidateRealm = false;
            LanguageManager.instance.trimProvinces = false;
            LanguageManager.instance.LoadVanilla();
            ReligionManager.instance = new ReligionManager();
            CultureManager.instance = new CultureManager();
            TitleManager.instance = new TitleManager();
            ReligionManager.instance.LoadVanilla();
            CultureManager.instance.LoadVanilla();
            TitleManager.instance.LoadVanilla();
            foreach (var instanceTitle in TitleManager.instance.Titles)
            {
                if(instanceTitle.Rank > 1)
                    instanceTitle.LiegeDirect = null;
            }
            DynastyManager.instance.LoadVanilla();
            var files = ModManager.instance.GetFileKeys("history\\provinces");
            foreach (var file in files)
            {
                string f = file.Substring(file.IndexOf("provinces\\") + "provinces\\".Length);

                int id = Convert.ToInt32(f.Split('-')[0]);

                var province = MapManager.instance.ProvinceIDMap[id];

                var script = ScriptLoader.instance.LoadKey(file);
                province.loadingFromHistoryFiles = true;
                foreach (var rootChild in script.Root.Children)
                {
                    if (rootChild is ScriptCommand)
                    {
                        var com = rootChild as ScriptCommand;
                        if (com.Name == "culture")
                        {
                            province.Culture = CultureManager.instance.CultureMap[com.Value.ToString()];
                            province.initialCulture = province.Culture.Name;
                        }
                        if (com.Name == "religion")
                        {
                            province.Religion = ReligionManager.instance.ReligionMap[com.Value.ToString()];
                            province.initialReligion = province.Religion.Name;
                        }
                        if (com.Name == "title")
                        {
                            string title = com.Value.ToString();
                          
                            province.RenameSafe(title);
                            if (province.Title != null)
                            {
                                province.Title.Owns.Add(province);
                                province.Title.CapitalProvince = province;
                                foreach (var titleSubTitle in province.Title.SubTitles)
                                {
                                    province.AddBarony(titleSubTitle.Value.Name, titleSubTitle.Value);

                                }
                            }                        
                        }

                        if (com.Name.StartsWith("b_"))
                        {               
                            province.ActivateBarony(com.Name, com.Value.ToString());
                        }
                        if (com.Name == "terrain")
                        {
                            province.terrain = com.Value.ToString();
                        }


                    }
                    
                    if (rootChild is ScriptScope)
                    {
                        var com = rootChild as ScriptScope;

                        if (com.Name.IsDate())
                        {
                            int year = com.Name.YearFromDate();

                            if (year <= endDate)
                            { foreach (var comChild in com.Children)
                                {
                                    if (comChild is ScriptCommand)
                                    {
                                        var e = comChild as ScriptCommand;
                                        if (e.Name == "culture")
                                        {
                                            province.Culture = CultureManager.instance.CultureMap[e.Value.ToString()];
                             
                                        }
                                        if (e.Name == "religion")
                                        {
                                            province.Religion = ReligionManager.instance.ReligionMap[e.Value.ToString()];
                                        }
                                        if (e.Name.StartsWith("b_"))
                                        {
                                            province.ActivateBarony(e.Name, e.Value.ToString());

                                        }
                                    }

                                }

                                province.dateScripts.Add(com);
                            }
                        }
                    }
                }

                province.loadingFromHistoryFiles = false;
            }

            CharacterManager.instance.LoadVanilla(endDate);
            TitleManager.instance.LoadToDate(endDate);
            AutoValidateRealm = true;
            AllowCapitalPicking = true;
        }

        public void PlaceVanillaOnMap()
        {
            AllowCapitalPicking = false;
            bDonePreStage2 = true;
            PreYear = 1;
            Year = 769;
            AutoValidateRealm = false;
            LanguageManager.instance.trimProvinces = false;
            LanguageManager.instance.LoadVanilla();
            ReligionManager.instance = new ReligionManager();
            CultureManager.instance = new CultureManager();
            TitleManager.instance = new TitleManager();
            ReligionManager.instance.LoadVanilla();
            CultureManager.instance.LoadVanilla();
            TitleManager.instance.LoadVanilla();
            foreach (var instanceTitle in TitleManager.instance.Titles)
            {
                if (instanceTitle.Rank > 1)
                    instanceTitle.LiegeDirect = null;
            }
            DynastyManager.instance.LoadVanilla();

            foreach (var title in TitleManager.instance.Titles)
            {
                if (title.Rank == 4)
                {
                    if (title.DejureSub.Count > 0)
                    {
                        // Place empire

                        PlaceEmpireRandomlyOnMap(title);
                    }
                }
            }

            CharacterManager.instance.LoadVanilla(769);

            AutoValidateRealm = true;
            AllowCapitalPicking = true;
        }

        private void PlaceEmpireRandomlyOnMap(TitleParser title)
        {
            List<TitleParser> allCounts = title.GetAllCounts();

            List<ProvinceParser> provinces = MapManager.instance.GetProvinceBlock(allCounts.Count);
            foreach (var subTitlesValue in title.DejureSub.Values)
            {
                // get all kingdom duchies...
                List<TitleParser> duchies = subTitlesValue.GetAllDuchies();
            }
        }
    }
}
