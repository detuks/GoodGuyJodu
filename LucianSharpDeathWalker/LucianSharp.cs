using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using DetuksSharp;
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

                Config = new Menu("LucianSharp [DeathWalker]", "Lucian", true);
                var orbwalkerMenu = new Menu("Lucian Orbwalker", "my_Orbwalker");
                DeathWalker.AddToMenu(orbwalkerMenu);
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
                Config.SubMenu("combo").AddItem(new MenuItem("Wvisib", "W to get vision")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));
               
                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));

                //KillSteal
                Config.AddSubMenu(new Menu("KillSteal Sharp", "killsteal"));
                Config.SubMenu("killsteal").AddItem(new MenuItem("ksOn", "do KillSteal")).SetValue(true);
                Config.SubMenu("killsteal").AddItem(new MenuItem("ksOnQ", "use Q")).SetValue(true);
                Config.SubMenu("killsteal").AddItem(new MenuItem("ksOnW", "use W")).SetValue(true);
                Config.SubMenu("killsteal").AddItem(new MenuItem("ksOnE", "use E")).SetValue(true);
               
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
                Obj_AI_Base.OnDoCast += onDoCast;
                DeathWalker.AfterAttack += AfterAttack;

                DeathWalker.OnUnkillable += onUnkillable;
                Obj_AI_Hero.OnLeaveLocalVisiblityClient += onLeaveVisibility;

                Lucian.setSkillShots();

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Game.PrintChat("Oops. Something went wrong with LucianSharp");
            }
        }

        private static void onLeaveVisibility(AttackableUnit sender, EventArgs args)
        {
            if (sender.IsEnemy && sender is Obj_AI_Hero && Config.Item("Wvisib").GetValue<bool>())
            {
                var sen = sender as Obj_AI_Hero;
                if (Lucian.W.CanCast(sen))
                {
                    Lucian.W.Cast(sen.Position);
                }
            }
        }

        private static void onDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;
            if (Lucian.gotPassiveRdy())
            {
                if(!args.SData.IsAutoAttack())
                    Console.WriteLine("Basaadaa");
            }
        }


        private static void onUnkillable(AttackableUnit unit, AttackableUnit target, int msTillDead)
        {
            if (target.Health < 30)
                return;
            if (target is Obj_AI_Base && !target.MagicImmune && msTillDead-200>(int)(Lucian.Qdata.SData.OverrideCastTime*1000))
            {
                Lucian.useQonTarg((Obj_AI_Base) target, Lucian.QhitChance.medium);
            }

        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            try
            {

                if(unit.IsMe)
                    Lucian.onAfterAttack(target);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("debugOn").GetValue<KeyBind>().Active) //fullDMG
            {
                /*Console.WriteLine(DtsHealthPrediction.ActiveAttacksTower.Count);
                foreach (var buf in Lucian.player.Buffs)
                {
                    Console.WriteLine(buf.Name);
                }
                Console.WriteLine(Lucian.gotPassiveRdy());*/
            }

            if (DeathWalker.CurrentMode == DeathWalker.Mode.Combo)
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
                Lucian.doCombo(target);
            }

            if (DeathWalker.CurrentMode == DeathWalker.Mode.Harass || DeathWalker.CurrentMode == DeathWalker.Mode.LaneClear)
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
                Lucian.doHarass(target);
            }

            if (Config.Item("ksOn").GetValue<bool>()) //fullDMG
                Lucian.doKillSteal();

        }

        private static void onDraw(EventArgs args)
        {
          /*  foreach (var target in ObjectManager.Get<Obj_AI_Base>().Where(ob =>Lucian.targValidForQ(ob)))
            {
                Lucian.getPolygonOn(target,target.BoundingRadius).Draw(Color.Red,2);
            }*/

            if (Config.Item("drawQ").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Lucian.player.Position, Lucian.Q.Range + 100, Color.Blue);
                Render.Circle.DrawCircle(Lucian.player.Position, 1100, Color.Blue);
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
                   // DeathWalker.lastAATick = 0;
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
