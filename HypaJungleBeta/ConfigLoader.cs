using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    class ConfigLoader
    {
        public static string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                    "\\LeagueSharp\\HypaJungle\\";

        public static StringList getChampionConfigs(string champName)
        {
            string[] files = Directory.GetFiles(path + champName + "\\", "*.hypa", SearchOption.AllDirectories);

            string[] fileNames = new string[files.Count()+1];
            fileNames[0] = "default";
            for (int i = 1; i < files.Count()+1; i++)
            {
                fileNames[i] = Path.GetFileName(files[i-1]);
            }
            Console.WriteLine(files.Count());
            StringList sl = new StringList(fileNames);

            return sl;
        }

        public static void setupFolders(List<string> names)
        {
            foreach (var name in names)
            {
                bool exists = Directory.Exists(path+name+"\\");
                if (!exists)
                    Directory.CreateDirectory(path + name + "\\");
            }
        }

        public static void loadNewConfigHypa(string configName)
        {
            try
            {
                if(configName == "default")
                    return;

                List<Spell> lvlSeq = new List<Spell>();
                List<Jungler.ItemToShop> buyThings = new List<Jungler.ItemToShop>();

                string fPath = path + HypaJungle.player.ChampionName + "\\" + configName;
                Console.WriteLine(fPath);
                var lines = File.ReadLines(fPath);
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                    if(line.StartsWith("--"))
                        continue;
                    //load level seq
                    if (line.StartsWith("LVL"))
                    {
                        lvlSeq.Clear();
                        string[] spells = line.Split(' ');
                        string[] allowSpells = new[] {"Q", "W", "E", "R"};
                        foreach (var spell in spells[1].Split(','))
                        {
                            if(!allowSpells.Contains(spell))
                                continue;
                            SpellSlot ss = (SpellSlot) Enum.Parse(typeof (SpellSlot), spell, false);
                            lvlSeq.Add(new Spell(ss));
                        }
                        Console.WriteLine("Spells found: "+lvlSeq.Count);
                        JungleClearer.jungler.levelUpSeq = lvlSeq.ToArray();
                    }

                    if (line.StartsWith("ITEM"))
                    {
                        string[] things = line.Split(' ');
                        if(things.Count() != 4)
                            continue;

                        int cost = int.Parse(things[1]);
                        List<int> itemIds = new List<int>();
                        if (things[2] != "NONE")
                            foreach (var itemId in things[2].Split(';'))
                            {
                                itemIds.Add(int.Parse(itemId));
                            }

                        List<int> itemsMustHave = new List<int>();
                        if (things[3] != "NONE")
                            foreach (var itemId in things[3].Split(';'))
                            {
                                itemsMustHave.Add(int.Parse(itemId));
                            }
                        Jungler.ItemToShop its = new Jungler.ItemToShop();
                        its.goldReach = cost;
                        its.itemIds = itemIds;
                        its.itemsMustHave = itemsMustHave;

                        buyThings.Add(its);
                    }

                }

                JungleClearer.jungler.buyThings = buyThings;

                Console.WriteLine("Custom config (" + configName + ") loaded!");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

    }
}
