using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DetuksSharp;
using LeagueSharp.Common;

namespace StandaloneDeathWalker
{
    class Standalone
    {
        public static Menu Config;

        public Standalone()
        {
            Config = new Menu("Standalone DW", "standDW", true);
            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            DeathWalker.AddToMenu(Config.SubMenu("Orbwalker"));

            Config.AddToMainMenu();
        }
    }
}
