using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DetuksSharp;
using LeagueSharp;
using LeagueSharp.Common;

namespace StandaloneDeathWalker
{
    class Standalone
    {
        public static Menu Config;

        public Standalone()
        {

            CustomEvents.Game.OnGameLoad += onLoad;
        }

        private static void onLoad(EventArgs args)
        {
            Config = new Menu("Standalone DW", "standDW", true);
            //Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            DeathWalker.AddToMenu(Config);
            Config.AddToMainMenu();
        }
    }
}
