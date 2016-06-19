using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace AutoBuyerSharp
{
    public class BuildManager
    {
        public enum ChampType
        {
            Mage,
            Tank,
            Support,
            Carry,
            Bruiser,
            MageTank,
            MageNoMana,
            TankAS,
            MageAS,
            DpsAS
        }

        private static Obj_AI_Hero player = ObjectManager.Player;

        private static string[] mages =
        {
            "Annie", "Ahri", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana",  "FiddleSticks",
             "Gragas", "Heimerdinger", "Karthus",
            "Kassadin",  "Leblanc", "Lissandra", "Lux", "Malzahar",
            "Morgana", "Orianna",   "Swain", "Syndra",  "TwistedFate", "Veigar", "Viktor",
            "Xerath", "Ziggs", "Zyra", "Vel'Koz", "Chogath", "Malphite", "Maokai","Shaco"
        };

        private static string[] supports =
        {
            "Alistar", "Blitzcrank", "Janna", "Karma", "Nami", "Sona", "Soraka", "Taric",
            "Thresh", "Zilean","Lulu",
        };

        private static string[] tanks =
        {
            "Amumu", "DrMundo","Sion", "Galio", "Hecarim", "Rammus", "Sejuani",
            "Shen", "Singed", "Skarner", "Volibear", "Leona",
             "Yorick", "Zac", "Udyr", "Nasus", "Trundle", "Irelia","Braum", "Vi"
        };

        private static string[] ad_carries =
        {
            "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves",  "KogMaw", "MissFortune",
              "Sivir", "Jinx","Jayce", "Gangplank",
            "Talon", "Tristana", "Twitch", "Urgot", "Varus",  "Zed", "Lucian","Yasuo","MasterYi","Quinn","Kalista","Vayne","Kindred"

        };

        private static string[] bruisers =
        {
            "Aatrox", "Darius",  "Fiora", "Garen", "JarvanIV", "Jax", "Khazix", "LeeSin",
            "Nautilus", "Nocturne", "Olaf", "Poppy",
            "Renekton", "Rengar", "Riven", "Shyvana", "Tryndamere", "MonkeyKing", "XinZhao","Pantheon",
            "Rek'Sai","Gnar","Wukong","RekSai"
        };

        private static string[] mageNoMana =
        {
            "Akali", "Katarina", "Vladimir", "Rumble","Mordekaiser", "Kennen"
        };

        private static string[] mageTank =
        {
            "Gragas","Galio","Bard","Singed","Nunu","Evelynn","Elise","Ryze","Ekko","Fizz", "Nidalee"
        };

        private static string[] mageAS =
        {
            "Azir","Kayle","Teemo",
        };

        private static string[] tankAS =
        {
            "Warwick",
        };

        private static string[] dpsAS =
        {

        };
        private static ChampType getType()
        {
            var cName = player.ChampionName;
            if (mages.Contains(cName))
                return ChampType.Mage;
            if (supports.Contains(cName))
                return ChampType.Support;
            if (tanks.Contains(cName))
                return ChampType.Tank;
            if (ad_carries.Contains(cName))
                return ChampType.Carry;
            if (bruisers.Contains(cName))
                return ChampType.Bruiser;
            if (mageNoMana.Contains(cName))
                return ChampType.MageNoMana;
            if (mageTank.Contains(cName))
                return ChampType.MageTank;
            if (mageAS.Contains(cName))
                return ChampType.MageAS;
            if (tankAS.Contains(cName))
                return ChampType.TankAS;
            if (dpsAS.Contains(cName))
                return ChampType.DpsAS;
            return ChampType.Tank;
        }

        public static Build getBestBuild()
        {
            switch (getType())
            {
                case ChampType.Support:
                case ChampType.Mage:
                    return new Build
                    {
                        coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Athenes_Unholy_Grail),
                            new ConditionalItem(ItemId.Sorcerers_Shoes),
                            new ConditionalItem(ItemId.Rabadons_Deathcap),
                            new ConditionalItem(ItemId.Abyssal_Scepter,ItemId.Zhonyas_Hourglass,ItemCondition.ENEMY_AP),
                            new ConditionalItem(ItemId.Void_Staff,ItemId.Liandrys_Torment, ItemCondition.ENEMY_MR),
                            new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                        },
                            startingItems = new List<ItemId>
                        {
                            ItemId.Needlessly_Large_Rod
                        }
                    };
                    case ChampType.TankAS:
                        return new Build
                        {
                            coreItems = new List<ConditionalItem>
                            {
                                new ConditionalItem(ItemId.Mercurys_Treads),
                                new ConditionalItem(ItemId.Frozen_Mallet),
                                new ConditionalItem(ItemId.Wits_End),
                                new ConditionalItem(ItemId.Blade_of_the_Ruined_King),
                                new ConditionalItem(ItemId.Banshees_Veil,ItemId.Sunfire_Cape, ItemCondition.ENEMY_MR),
                                new ConditionalItem(ItemId.Trinity_Force),
                            },
                                startingItems = new List<ItemId>
                            {
                                ItemId.Boots_of_Speed,(ItemId)3751
                            }
                        };
                    case ChampType.DpsAS:
                        return new Build
                        {
                            coreItems = new List<ConditionalItem>
                            {
                                new ConditionalItem(ItemId.Boots_of_Speed),
                                new ConditionalItem(ItemId.Statikk_Shiv),
                                new ConditionalItem(ItemId.The_Bloodthirster),
                                new ConditionalItem(ItemId.Phantom_Dancer),
                                new ConditionalItem(ItemId.Infinity_Edge),
                                new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail, ItemCondition.ENEMY_MR),
                            },
                                startingItems = new List<ItemId>
                            {
                                ItemId.Zeal,
                            }
                        };
                    case ChampType.MageAS:
                        return new Build
                        {
                            coreItems = new List<ConditionalItem>
                            {
                                new ConditionalItem(ItemId.Sorcerers_Shoes),
                                new ConditionalItem(ItemId.Nashors_Tooth),
                                new ConditionalItem(ItemId.Rabadons_Deathcap),
                                new ConditionalItem(ItemId.Ludens_Echo),
                                new ConditionalItem(ItemId.Guinsoos_Rageblade),
                                new ConditionalItem(ItemId.Hextech_Gunblade),
                            },
                                startingItems = new List<ItemId>
                            {
                                ItemId.Stinger,
                            }
                        };
                case ChampType.MageNoMana:
                    return new Build
                    {
                        coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Ludens_Echo),
                            new ConditionalItem(ItemId.Sorcerers_Shoes),
                            new ConditionalItem(ItemId.Abyssal_Scepter,ItemId.Zhonyas_Hourglass,ItemCondition.ENEMY_AP),
                            new ConditionalItem(ItemId.Rabadons_Deathcap),
                            new ConditionalItem(ItemId.Void_Staff,ItemId.Liandrys_Torment, ItemCondition.ENEMY_MR),
                            new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                        },
                            startingItems = new List<ItemId>
                        {
                            ItemId.Needlessly_Large_Rod
                        }
                    };
                case ChampType.MageTank:
                    return new Build
                    {
                        coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Rod_of_Ages),
                            new ConditionalItem(ItemId.Sorcerers_Shoes),
                            new ConditionalItem(ItemId.Abyssal_Scepter,ItemId.Zhonyas_Hourglass,ItemCondition.ENEMY_MR),
                            new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                            new ConditionalItem(ItemId.Rabadons_Deathcap),
                            new ConditionalItem(ItemId.Liandrys_Torment),
                        },
                            startingItems = new List<ItemId>
                        {
                            ItemId.Catalyst_the_Protector,
                        }
                    };
                    case ChampType.Bruiser:
                        return new Build
                        {
                            coreItems = new List<ConditionalItem>
                            {
                                new ConditionalItem(ItemId.Mercurys_Treads),
                                new ConditionalItem(ItemId.Spirit_Visage,ItemId.Sunfire_Cape,ItemCondition.ENEMY_AP),
                                new ConditionalItem(ItemId.Trinity_Force),
                                new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail,ItemCondition.ENEMY_AP),
                                new ConditionalItem(ItemId.Blade_of_the_Ruined_King),
                                new ConditionalItem(ItemId.Maw_of_Malmortius,ItemId.Randuins_Omen,ItemCondition.ENEMY_AP),
                            },
                                startingItems = new List<ItemId>
                            {
                                ItemId.Boots_of_Speed,ItemId.Ruby_Crystal,ItemId.Long_Sword,
                            }
                        };
                default:
                    return new Build
                    {
                        coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Infinity_Edge),
                            new ConditionalItem(ItemId.Berserkers_Greaves),
                            new ConditionalItem(ItemId.Phantom_Dancer),
                            new ConditionalItem(ItemId.Essence_Reaver),
                            new ConditionalItem(ItemId.Maw_of_Malmortius,ItemId.The_Bloodthirster,ItemCondition.ENEMY_AP),
                            new ConditionalItem((ItemId)ItemId.Blade_of_the_Ruined_King),
                           // new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail,ItemCondition.ENEMY_MR),
                        },
                            startingItems = new List<ItemId>
                        {
                            ItemId.Boots_of_Speed,ItemId.Long_Sword,ItemId.Long_Sword,
                        }
                    };
            }

        }
    }
}
