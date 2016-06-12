using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ARAMDetFull.Champions;
using LeagueSharp;
using LeagueSharp.Common;

namespace ARAMDetFull
{
    class ARAMDetFull
    {
        /* TODO:
         * ##- Tower range higher dives a lot
         * ##- before level 6/7 play safer dont go so close stay behind other players or 800/900 units away from closest enemy champ
         * ##- Target selector based on invincible enemies
         * ##- IF invincible or revive go full in
         * ##- if attacking enemy and it is left 3 or less aa to kill then follow to kill (check movespeed dif)
         *  - bush invis manager player death
         *  - fixx gankplank plays like retard
         *  - this weeks customs
         *  - WPF put to allways take mark
         * ##- nami auto level
         *  - Some skills make aggresivity for time and how much to put in balance ignore minsions on/off
         * ## - LeeSin
         * ## - Nocturn
         *  - Gnar
         *  -Katarina error
         *  - Gangplank error
         *  ##- healing relics
         *  -Make velkoz
         */

        public ARAMDetFull()
        {
            Console.WriteLine("Aram det full started!");
            CustomEvents.Game.OnGameLoad += onLoad;
        }

        public static int gameStart = 0;

        public static Menu Config;

        public static int now
        {
            get { return (int)DateTime.Now.TimeOfDay.TotalMilliseconds; }
        }

        private static void onLoad(EventArgs args)
        {
            gameStart = now;

            Game.PrintChat("ARAm - Sharp by DeTuKs");

            try
            {

                Config = new Menu("ARAM", "Yasuo", true);

                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));

                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));

                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));

                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));


                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "Debug Target")).SetValue(new KeyBind('T', KeyBindType.Press, false));


                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnUpdate += OnGameUpdate;
                Drawing.OnDraw += onDraw;
                CustomEvents.Game.OnGameEnd += OnGameEnd;
                ARAMSimulator.setupARMASimulator();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void OnGameEnd(EventArgs args)
        {
            Utility.DelayAction.Add(10000, closeGame);
        }

        private static void closeGame()
        {
            Game.Quit();
        }

        private static void onDraw(EventArgs args)
        {
            Drawing.DrawText(100, 100, Color.Red, "bal: " + ARAMSimulator.balance + " fear: "+MapControl.fearDistance );
           //return;
           // ((Jayce)ARAMSimulator.champ).drawCD();
            foreach (var hel in ObjectManager.Get<Obj_AI_Base>().Where(r => r.IsValid && !r.IsDead && r.Name.Contains("ealth")))
            {
                var spos = Drawing.WorldToScreen(hel.Position);
                Drawing.DrawText(spos.X, spos.Y, Color.Brown, " : " + hel.Name);
                Drawing.DrawText(spos.X, spos.Y+25, Color.Brown, hel.IsDead + " : " + hel.Type+ " : " + hel.IsValid+ " : " + hel.IsVisible);
            }
            var tar = ARAMTargetSelector.getBestTarget(5100);
            if (tar != null)
                Utility.DrawCircle(tar.Position, 150, Color.Violet);

            foreach (var sec in ARAMSimulator.sectors)
            {
                sec.draw();
            }

            foreach (var ene in MapControl.enemy_champions)
            {
                Utility.DrawCircle(ene.hero.Position, ene.getReach() , Color.Green);
            }
            return;

            foreach (var ene in MapControl.enemy_champions)
            {
                Utility.DrawCircle(ene.hero.Position, ene.reach, Color.Violet);
            }
        }
        public static void getAllBuffs()
        {
            foreach (var aly in HeroManager.Enemies)
            {
                foreach (var buffs in aly.Buffs)
                {
                    Console.WriteLine(aly.ChampionName + " - Buf: " + buffs.Name);
                }
            }
        }

        private static int lastTick = now;

        private static int tickTimeRng = 77;
        private static Random rng = null;
        private static void OnGameUpdate(EventArgs args)
        {
            //if (lastTick + tickTimeRng > now)
            //    return;

            //if(rng == null)
             //   rng = new Random();

            //tickTimeRng = rng.Next(70, 140);
            lastTick = now;
            ARAMSimulator.updateArmaPlay();
        }
    }
}
