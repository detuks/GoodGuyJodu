#region

using System;
using System.Drawing;
using System.Linq;
using DetuksSharp;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;

#endregion

namespace Marksman.Champions
{
    using System.Threading;

    using Utils = LeagueSharp.Common.Utils;

    internal interface IKindred
    {
        void DeathWalker_AfterAttack(AttackableUnit unit, AttackableUnit target);
        void Drawing_OnDraw(EventArgs args);
        void Game_OnGameUpdate(EventArgs args);
        bool ComboMenu(Menu config);
        bool HarassMenu(Menu config);
        bool MiscMenu(Menu config);
        bool DrawingMenu(Menu config);
        bool ExtrasMenu(Menu config);
        bool LaneClearMenu(Menu config);
        //bool JungleClearMenu(Menu config);
    }

    internal class Kindred : Champion, IKindred
    {
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;

        public Kindred()
        {
            Q = new Spell(SpellSlot.Q, 340);
            W = new Spell(SpellSlot.W, 593);
            E = new Spell(SpellSlot.E, 640);
            R = new Spell(SpellSlot.R, 1100);
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotCircle);

            Marksman.Utils.Utils.PrintMessage("Kindred loaded.");
        }

        public override void DeathWalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
        }
        public override void Drawing_OnDraw(EventArgs args)
        {
            Spell[] spellList = { Q, E, R };
            foreach (var spell in spellList)
            {
                var menuItem = this.GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }

            if (Program.Config.Item("UseRC").GetValue<bool>())
            {
                var drawRMin = Program.Config.Item("DrawRMin").GetValue<Circle>();
                if (drawRMin.Active)
                {
                    var minRRange = Program.Config.Item("UseRCMinRange").GetValue<Slider>().Value;
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, minRRange, drawRMin.Color, 2);
                }

                var drawRMax = Program.Config.Item("DrawRMax").GetValue<Circle>();
                if (drawRMax.Active)
                {
                    var maxRRange = Program.Config.Item("UseRCMaxRange").GetValue<Slider>().Value;
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, maxRRange, drawRMax.Color, 2);
                }
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {

            //if (this.JungleClearActive)
            //{
            //    this.ExecJungleClear();
            //}

            if (this.LaneClearActive && Q.IsReady())
            {
                this.ExecLaneClear();
            }

            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (!t.IsValidTarget())
            {
                return;
            }

            if (E.IsReady() && Program.Config.Item("UseETH").GetValue<KeyBind>().Active && ToggleActive)
            {
                if (ObjectManager.Player.HasBuff("Recall"))
                {
                    return;
                }

                if (t.IsValidTarget(E.Range - 150)) E.CastOnUnit(t);
            }

            if (this.ComboActive)
            {
                var useQ = this.GetValue<bool>("UseQ" + (this.ComboActive ? "C" : "H"));
                var useW = this.GetValue<bool>("UseW" + (this.ComboActive ? "C" : "H"));
                var useE = this.GetValue<bool>("UseEC");
                var useR = this.GetValue<bool>("UseRC");

                if (DeathWalker.canMove())
                {
                    if (E.IsReady() && t.IsValidTarget(E.Range))
                    {
                        E.CastOnUnit(t);
                    }

                    if (W.IsReady() && t.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }

                    if (Q.IsReady() && t.IsValidTarget(Q.Range + DeathWalker.getRealAutoAttackRange(null) + 65))
                    {
                        Q.Cast(t.Position);
                    }

                    if (R.IsReady())
                    {
                        R.Cast(ObjectManager.Player.Position);
                    }
                }
            }

        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + this.Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + this.Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + this.Id, "Use E").SetValue(new StringList(new[] { "Off", "On", "On: Force focus to marked enemy" }, 2))).ValueChanged +=
                (sender, args) =>
                    {
                        /*foreach (var item in config.Items.Where(i => i.Tag == 11))
                        {
                            item.Show(args.GetNewValue<StringList>().SelectedIndex != 0);
                        }*/
                    };

            /*foreach (var enemy in HeroManager.Enemies)
            {
                config.AddItem(
                    new MenuItem(enemy.ChampionName + "_UseEC" + this.Id, Program.Tab + enemy.ChampionName).SetValue(
                        new StringList(new[] { "Don't", "Use: If I can kill him", "Use: Everytime" },Marksman.Utils.Utils.GetEnemyPriority(enemy.ChampionName) < 2 ? 1 : 2))).SetTag(11);
            }*/

            config.AddItem(new MenuItem("UseRC", "Use R").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH" + this.Id, "Q").SetValue(true));
            config.AddItem(new MenuItem("UseWH" + this.Id, "W").SetValue(true));
            config.AddItem(new MenuItem("UseEH" + this.Id, "Use E").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            config.AddItem(new MenuItem("UseETH", "E (Toggle)").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQL" + this.Id, "Use Q").SetValue(true)).ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
            {
              //  config.Item("UseQLM").Show(args.GetNewValue<bool>());
              //  Program.CClass.Config.Item("LaneMinMana").Show(args.GetNewValue<bool>());
            };
            config.AddItem(new MenuItem("UseQLM", "Min. Minion:").SetValue(new Slider(2, 1, 3)));
            config.AddItem(new MenuItem("UseWL", "Use W").SetValue(false));
            return true;
        }

        //public override bool JungleClearMenu(Menu config)
        //{
        //    config.AddItem(new MenuItem("UseQJ" + this.Id, "Use Q").SetValue(true)).ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
        //    {
        //        config.Item("UseQLM").Show(args.GetNewValue<bool>());
        //        Program.CClass.Config.Item("JungleMinMana").Show(args.GetNewValue<bool>());
        //    };

        //    config.AddItem(new MenuItem("UseQJM", "Min. Minion:").SetValue(new Slider(2, 1, 3)));
        //    config.AddItem(new MenuItem("UseWJ", "Use W").SetValue(false));
        //    config.AddItem(new MenuItem("UseEJ", "Use E").SetValue(new StringList(new[] { "Off", "On", "Just big mobs" }, 1)));
        //    return true;
        //}

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ" + this.Id, "Q range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(new MenuItem("DrawE" + this.Id, "E range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Damage After Combo").SetValue(true);

            config.AddItem(dmgAfterComboItem);

            config.AddItem(
                   new MenuItem("HarassActiveTPermashow", "Show harass permashow").SetValue(true)).ValueChanged += (s, ar) =>
                   {
                       if (ar.GetNewValue<bool>())
                       {
                           Program.CClass.Config.Item("UseETH").Permashow(true, "Harass Toggle");
                       }
                       else
                       {
                           Program.CClass.Config.Item("UseETH").Permashow(false);
                       }
                   };

            Program.CClass.Config.Item("UseETH").Permashow(config.Item("HarassActiveTPermashow").GetValue<bool>(), "Harass Toggle");
            return true;
        }

        public override bool ExtrasMenu(Menu config)
        {
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            return false;
        }

        public void ExecLaneClear()
        {
            var useQ = Program.Config.Item("UseQL").GetValue<StringList>().SelectedIndex;

            var minion =
                MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range)
                    .FirstOrDefault(m => m.Health < ObjectManager.Player.GetSpellDamage(m, SpellSlot.Q));

            if (minion != null)
            {
                switch (useQ)
                {
                    case 1:
                        minion =
                            MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range)
                                .FirstOrDefault(
                                    m =>
                                    m.Health < ObjectManager.Player.GetSpellDamage(m, SpellSlot.Q)
                                    && m.Health > ObjectManager.Player.TotalAttackDamage);
                        Q.Cast(minion);
                        break;

                    case 2:
                        minion =
                            MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range)
                                .FirstOrDefault(
                                    m =>
                                    m.Health < ObjectManager.Player.GetSpellDamage(m, SpellSlot.Q)
                                    && ObjectManager.Player.Distance(m)
                                    > DeathWalker.getRealAutoAttackRange(null) + 65);
                        Q.Cast(minion);
                        break;
                }
            }            
        }

        public void ExecJungleClear()
        {
            var jungleMobs = Marksman.Utils.Utils.GetMobs(Q.Range, Marksman.Utils.Utils.MobTypes.All);

            if (jungleMobs != null)
            {
                switch (Program.Config.Item("UseQJ").GetValue<StringList>().SelectedIndex)
                {
                    case 1:
                        {
                            Q.Cast(jungleMobs);
                            break;
                        }
                    case 2:
                        {
                            jungleMobs = Marksman.Utils.Utils.GetMobs(Q.Range, Marksman.Utils.Utils.MobTypes.BigBoys);
                            if (jungleMobs != null)
                            {
                                Q.Cast(jungleMobs);
                            }
                            break;
                        }
                }
            }
        }
    }
}
