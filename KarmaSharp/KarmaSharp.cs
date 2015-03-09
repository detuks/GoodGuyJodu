using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using System.Net;
/*
 * ToDo:
 * 
 * */


namespace KarmaSharp
{
    internal class KarmaSharp
    {

        public const string CharName = "Karma";

        public static Menu Config;

        public static Obj_AI_Hero target;

        public KarmaSharp()
        {
            if (ObjectManager.Player.BaseSkinName != CharName)
                return;
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("Karma - Sharp by DeTuKs");

            try
            {

				  Config = new Menu("Karma - Sharp by DeTuKs Donate if you love my assams :)", "Karma", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                Karma.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                //TS
                var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
                SimpleTs.AddToMenu(TargetSelectorMenu);
                Config.AddSubMenu(TargetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E on Myself")).SetValue(false);
                Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R on Q(Harass too)")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));
               
                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));
                Config.SubMenu("harass").AddItem(new MenuItem("harP", "Harass Enemy press")).SetValue(new KeyBind('T', KeyBindType.Press, false));
                Config.SubMenu("harass").AddItem(new MenuItem("harT", "Harass Enemy toggle")).SetValue(new KeyBind('H', KeyBindType.Toggle, false));
                Config.SubMenu("harass").AddItem(new MenuItem("useQHar", "Use Q with R")).SetValue(true);
                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                Config.SubMenu("extra").AddItem(new MenuItem("useMinions", "Use minions on Q")).SetValue(true);
				//Donate
                Config.AddSubMenu(new Menu("Donate", "Donate"));
                Config.SubMenu("Donate").AddItem(new MenuItem("domateMe", "PayPal:")).SetValue(true);
                Config.SubMenu("Donate").AddItem(new MenuItem("domateMe2", "dtk600@gmail.com")).SetValue(true);
                Config.SubMenu("Donate").AddItem(new MenuItem("domateMe3", "Tnx ^.^")).SetValue(true);

                //Debug
              //  Config.AddSubMenu(new Menu("Debug", "debug"));
              //  Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "Debug Target")).SetValue(new KeyBind('T', KeyBindType.Press, false));


                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

                Karma.setSkillShots();
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Yasuo- Sharpino");
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Karma.orbwalker.ActiveMode.ToString() == "Combo")
            {
                target = SimpleTs.GetTarget(1150, SimpleTs.DamageType.Magical);
                    Karma.doCombo(target);
            }

            if (Karma.orbwalker.ActiveMode.ToString() == "Mixed")
            {
               
            }

            if (Karma.orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                
            }


            if (Config.Item("harP").GetValue<KeyBind>().Active || Config.Item("harT").GetValue<KeyBind>().Active)
            {
                target = SimpleTs.GetTarget(1150, SimpleTs.DamageType.Magical);
                    Karma.doHarass(target);
            }
        }

        private static void onDraw(EventArgs args)
        {
            Drawing.DrawCircle(Karma.Player.Position, 950, Color.Blue);
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
          

        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
          
        }

        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {


           
        }




    }
}
