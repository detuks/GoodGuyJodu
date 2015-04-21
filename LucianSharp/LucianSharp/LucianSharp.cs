using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
/*
 * ToDo:
 * 
 * */
using SharpDX;
using Color = System.Drawing.Color;


namespace LucianSharp
{
    internal class LucianSharp
    {

        public const string CharName = "Lucian";
        public static Menu Config;

        public LucianSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("Lucian - Sharp by DeTuKs");

            try
            {

                Config = new Menu("LucianSharp", "Lucian", true);
                var orbwalkerMenu = new Menu("Lucian Orbwalker", "my_Orbwalker");
                LXOrbwalker.AddToMenu(orbwalkerMenu);
                Config.AddSubMenu(orbwalkerMenu);
                //TS
                var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(TargetSelectorMenu);
                Config.AddSubMenu(TargetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E from melee")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));
               
                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));
               
                //Extra
                Config.AddSubMenu(new Menu("Draw Sharp", "draw"));
                Config.SubMenu("draw").AddItem(new MenuItem("drawQ", "draw Q")).SetValue(true);
                Config.SubMenu("draw").AddItem(new MenuItem("drawW", "draw W")).SetValue(true);
                Config.SubMenu("draw").AddItem(new MenuItem("drawE", "draw E")).SetValue(true);
                

                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug").AddItem(new MenuItem("debugOn", "Debug stuff")).SetValue(new KeyBind('A', KeyBindType.Press));


                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

                LXOrbwalker.AfterAttack += AfterAttack;

                Lucian.setSkillShots();

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Game.PrintChat("Oops. Something went wrong with LucianSharp");
            }
        }

        private static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            Lucian.onAfterAttack(target);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("debugOn").GetValue<KeyBind>().Active) //fullDMG
            {
                Console.WriteLine(DtsHealthPrediction.ActiveAttacksTower.Count);
                foreach (var buf in Lucian.player.Buffs)
                {
                    Console.WriteLine(buf.Name);
                }
                Console.WriteLine(Lucian.gotPassiveRdy());
            }

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
                Lucian.doCombo(target);
            }

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass || LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear)
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
                Lucian.doHarass(target);
            }

        }

        private static void onDraw(EventArgs args)
        {
          /*  foreach (var target in ObjectManager.Get<Obj_AI_Base>().Where(ob =>Lucian.targValidForQ(ob)))
            {
                Lucian.getPolygonOn(target,target.BoundingRadius).Draw(Color.Red,2);
            }*/

            Drawing.DrawCircle(Lucian.player.Position, 1000, LXOrbwalker.CanAttack()?Color.Blue:Color.Red);
            if (Config.Item("drawQ").GetValue<bool>())
            {
                Drawing.DrawCircle(Lucian.player.Position, Lucian.Q.Range + 100, Color.Blue);
                Drawing.DrawCircle(Lucian.player.Position, 1100, Color.Blue);
            }
            if (Config.Item("drawW").GetValue<bool>())
                Drawing.DrawCircle(Lucian.player.Position, Lucian.W.Range, Color.Yellow);
            if (Config.Item("drawE").GetValue<bool>())
                Drawing.DrawCircle(Lucian.player.Position, Lucian.E.Range, Color.Green);

        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile)
            {
                var mis = (Obj_SpellMissile)sender;
                if (mis.SpellCaster.IsMe && mis.SData.Name.Contains("Attack"))
                {
                    LXOrbwalker._lastAATick = 0;
                    //Lucian.onAfterAttack((Obj_AI_Base)mis.Target);
                }
            }
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
          
        }

        public static void OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {

        }
    }
}
