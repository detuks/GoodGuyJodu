#region

using System;
using System.Drawing;
using System.Linq;
using DetuksSharp;
using LeagueSharp;
using LeagueSharp.Common;
using Marksman.Champions;
using Marksman.Utils;
using Activator = Marksman.Utils.Activator;

#endregion

namespace Marksman
{
    internal class Program
    {
        public const string MenuSpace = "       "; //I'll remove after
        public const string Tab = "    ";
        public static Menu Config;
        public static Menu QuickSilverMenu;
        
//        public static Menu MenuInterruptableSpell;
        public static Champion CClass;
        public static Activator AActivator;
        public static double ActivatorTime;
        private static Obj_AI_Hero xSelectedTarget;

        public static SpellSlot SmiteSlot = SpellSlot.Unknown;

        public static Spell Smite;

        private static readonly int[] SmitePurple = {3713, 3726, 3725, 3726, 3723};
        private static readonly int[] SmiteGrey = {3711, 3722, 3721, 3720, 3719};
        private static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714};
        private static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707};

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("Deadman", "Deadman", true);
            CClass = new Champion();
            AActivator = new Activator();


            var BaseType = CClass.GetType();

            /* Update this with Activator.CreateInstance or Invoke
               http://stackoverflow.com/questions/801070/dynamically-invoking-any-function-by-passing-function-name-as-string 
               For now stays cancer.
             */
            var championName = ObjectManager.Player.ChampionName.ToLowerInvariant();

            switch (championName)
            {
                case "ashe":
                    CClass = new Ashe();
                    break;
                case "caitlyn":
                    CClass = new Caitlyn();
                    break;
                case "corki":
                    CClass = new Corki();
                    break;
                case "draven":
                    CClass = new Draven();
                    break;
                case "ezreal":
                    CClass = new Ezreal();
                    break;
                case "graves":
                    CClass = new Graves();
                    break;
                case "gnar":
                    CClass = new Gnar();
                    break;
                case "jinx":
                    CClass = new Jinx();
                    break;
                case "kalista":
                    CClass = new Kalista();
                    break;
                case "kindred":
                    CClass = new Kindred();
                    break;
                case "kogmaw":
                    CClass = new Kogmaw();
                    break;
                case "lucian":
                    CClass = new Lucian();
                    break;
                case "missfortune":
                    CClass = new MissFortune();
                    break;
                case "quinn":
                    CClass = new Quinn();
                    break;
                case "sivir":
                    CClass = new Sivir();
                    break;
                case "teemo":
                    CClass = new Teemo();
                    break;
                case "tristana":
                    CClass = new Tristana();
                    break;
                case "twitch":
                    CClass = new Twitch();
                    break;
                case "urgot":
                    CClass = new Urgot();
                    break;
                case "vayne":
                    CClass = new Vayne();
                    break;
                case "varus":
                    CClass = new Varus();
                    break;
            }


            CClass.Id = ObjectManager.Player.CharData.BaseSkinName;
            CClass.Config = Config;

            

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("DeathWalker", "Orbwalker"));

            DeathWalker.AddToMenu(Config.SubMenu("Orbwalker"));


            /* Menu Summoners */
            var summoners = Config.AddSubMenu(new Menu("Summoners", "Summoners"));
            var summonersHeal = summoners.AddSubMenu(new Menu("Heal", "Heal"));
            {
                summonersHeal.AddItem(new MenuItem("SUMHEALENABLE", "Enable").SetValue(true));
                summonersHeal.AddItem(new MenuItem("SUMHEALSLIDER", "Min. Heal Per.").SetValue(new Slider(20, 99, 1)));
            }

            var summonersBarrier = summoners.AddSubMenu(new Menu("Barrier", "Barrier"));
            {
                summonersBarrier.AddItem(new MenuItem("SUMBARRIERENABLE", "Enable").SetValue(true));
                summonersBarrier.AddItem(
                    new MenuItem("SUMBARRIERSLIDER", "Min. Heal Per.").SetValue(new Slider(20, 99, 1)));
            }

            var summonersIgnite = summoners.AddSubMenu(new Menu("Ignite", "Ignite"));
            {
                summonersIgnite.AddItem(new MenuItem("SUMIGNITEENABLE", "Enable").SetValue(true));
            }
            /* Menu Items */
            var items = Config.AddSubMenu(new Menu("Items", "Items"));
            items.AddItem(new MenuItem("BOTRK", "BOTRK").SetValue(true));
            items.AddItem(new MenuItem("GHOSTBLADE", "Ghostblade").SetValue(true));
            items.AddItem(new MenuItem("SWORD", "Sword of the Divine").SetValue(true));
            items.AddItem(new MenuItem("MURAMANA", "Muramana").SetValue(true));
            QuickSilverMenu = new Menu("QSS", "QuickSilverSash");
            items.AddSubMenu(QuickSilverMenu);
            QuickSilverMenu.AddItem(new MenuItem("AnyStun", "Any Stun").SetValue(true));
            QuickSilverMenu.AddItem(new MenuItem("AnySlow", "Any Slow").SetValue(true));
            QuickSilverMenu.AddItem(new MenuItem("AnySnare", "Any Snare").SetValue(true));
            QuickSilverMenu.AddItem(new MenuItem("AnyTaunt", "Any Taunt").SetValue(true));
            foreach (var t in AActivator.BuffList)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    if (t.ChampionName == enemy.ChampionName)
                        QuickSilverMenu.AddItem(new MenuItem(t.BuffName, t.DisplayName).SetValue(t.DefaultValue));
                }
            }
            items.AddItem(
                new MenuItem("UseItemsMode", "Use items on").SetValue(
                    new StringList(new[] {"No", "Mixed mode", "Combo mode", "Both"}, 2)));


            //var Extras = Config.AddSubMenu(new Menu("Extras", "Extras"));
            //new PotionManager(Extras);

            // If Champion is supported draw the extra menus
            if (BaseType != CClass.GetType())
            {
                SetSmiteSlot();


                var combo = new Menu("Combo", "Combo");
                if (CClass.ComboMenu(combo))
                {
                    if (SmiteSlot != SpellSlot.Unknown)
                        combo.AddItem(new MenuItem("ComboSmite", "Use Smite").SetValue(true));

                    Config.AddSubMenu(combo);
                }

                var harass = new Menu("Harass", "Harass");
                if (CClass.HarassMenu(harass))
                {
                    harass.AddItem(new MenuItem("HarassMana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
                    Config.AddSubMenu(harass);
                }

                var laneclear = new Menu("LaneClear", "LaneClear");
                if (CClass.LaneClearMenu(laneclear))
                {
                    laneclear.AddItem(
                        new MenuItem("LaneClearMana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
                    Config.AddSubMenu(laneclear);
                }

                var misc = new Menu("Misc", "Misc");
                if (CClass.MiscMenu(misc))
                {
                    Config.AddSubMenu(misc);
                }

                var extras = new Menu("Extras", "Extras");
                if (CClass.ExtrasMenu(extras))
                {
                    new PotionManager(extras);
                    Config.AddSubMenu(extras);
                }

                var drawing = new Menu("Drawings", "Drawings");
                if (CClass.DrawingMenu(drawing))
                {
                    drawing.AddItem(new MenuItem("Deadman.Drawings", "Deadman Default Draw Options"));
                    //drawing.AddItem(new MenuItem("Draw.Ping", MenuSpace + "Show Game Ping").SetValue(true));
                    drawing.AddItem(new MenuItem("Draw.ToD", MenuSpace + "Turn Off Drawings On Team Fight").SetValue(false));
                    drawing.AddItem(new MenuItem("Draw.ToDControlRange", MenuSpace + MenuSpace + "Control Range:").SetValue(new Slider(1200, 1600, 600)));
                    drawing.AddItem(new MenuItem("Draw.ToDControlRangeColor", MenuSpace + MenuSpace + "Draw Control Range:").SetValue(new Circle(false, Color.GreenYellow)));
                    drawing.AddItem(new MenuItem("Draw.ToDMinEnemy", MenuSpace + MenuSpace + "Min. Enemy Count:").SetValue(new Slider(3, 5, 0)));

                    drawing.AddItem(new MenuItem("drawMinionLastHit", MenuSpace + "Minion Last Hit").SetValue(new Circle(false,Color.GreenYellow)));
                    drawing.AddItem(new MenuItem("drawMinionNearKill", MenuSpace + "Minion Near Kill").SetValue(new Circle(false,Color.Gray)));
                    drawing.AddItem(
                        new MenuItem("drawJunglePosition", MenuSpace + "Jungle Farm Position").SetValue(false));
                    drawing.AddItem(new MenuItem("Draw.DrawMinion", MenuSpace + "Draw Minions Sprite").SetValue(false));
                    drawing.AddItem(new MenuItem("Draw.DrawTarget", MenuSpace + "Draw Target Sprite").SetValue(false));
                    //drawing.AddItem(new MenuItem("Draw.DrawSTarget", MenuSpace + "Draw Selected Target", true).SetValue(new Circle(false,Color.GreenYellow)));
                    Config.AddSubMenu(drawing);
                }
            }

            

            CClass.MainMenu(Config);
            if (championName == "sivir")
            {
                Evade.Evade.Initiliaze();
                Evade.Config.Menu.DisplayName = "E";
                Config.AddSubMenu(Evade.Config.Menu);
            }            
            //Evade.Evade.Initiliaze();
            //Config.AddSubMenu(Evade.Config.Menu);
            
            Config.AddToMainMenu();
            Sprite.Load();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            DeathWalker.AfterAttack += DeathWalker_AfterAttack;
            DeathWalker.BeforeAttack += DeathWalker_BeforeAttack;
            //Game.OnWndProc += Game_OnWndProc;
        }


        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != 0x201)
                return;

            foreach (var objAiHero in (from hero in ObjectManager.Get<Obj_AI_Hero>()
                where hero.IsValidTarget()
                select hero
                into h
                orderby h.Distance(Game.CursorPos) descending
                select h
                into enemy
                where enemy.Distance(Game.CursorPos) < 150f
                select enemy).Where(objAiHero => objAiHero != null && objAiHero != xSelectedTarget))
            {
                xSelectedTarget = objAiHero;
                TargetSelector.SetTarget(objAiHero);
            }
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            //if (CClass.Config.SubMenu("Drawings").Item("Draw.Ping").GetValue<bool>())
            //    Drawing.DrawText(Drawing.Width*0.94f, Drawing.Height*0.05f, Color.GreenYellow, "Ping: " + Game.Ping);
                
            var toD = CClass.Config.SubMenu("Drawings").Item("Draw.ToD").GetValue<bool>();
            if (toD)
            {
                var enemyCount =
                    CClass.Config.SubMenu("Drawings").Item("Draw.ToDMinEnemy").GetValue<Slider>().Value;
                var controlRange =
                    CClass.Config.SubMenu("Drawings").Item("Draw.ToDControlRange").GetValue<Slider>().Value;

                var xEnemies = HeroManager.Enemies.Count(enemies => enemies.IsValidTarget(controlRange));
                if (xEnemies >= enemyCount)
                    return;

                var toDRangeColor =
                    CClass.Config.SubMenu("Drawings").Item("Draw.ToDControlRangeColor").GetValue<Circle>();
                if (toDRangeColor.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, controlRange, toDRangeColor.Color);

            }
            /*
            var t = TargetSelector.SelectedTarget;
            if (!t.IsValidTarget())
            {
                t = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
                TargetSelector.SetTarget(t);
            }

            if (t.IsValidTarget() && ObjectManager.Player.Distance(t) < 1110)
            {
                Render.Circle.DrawCircle(t.Position, 150, Color.Yellow);
            }
            */
            var drawJunglePosition = CClass.Config.SubMenu("Drawings").Item("drawJunglePosition").GetValue<bool>();
            {
                if (drawJunglePosition)
                    Utils.Utils.Jungle.DrawJunglePosition();
            }

            var drawMinionLastHit = CClass.Config.SubMenu("Drawings").Item("drawMinionLastHit").GetValue<Circle>();
            var drawMinionNearKill = CClass.Config.SubMenu("Drawings").Item("drawMinionNearKill").GetValue<Circle>();
            if (drawMinionLastHit.Active || drawMinionNearKill.Active)
            {
                var xMinions =
                    MinionManager.GetMinions(ObjectManager.Player.Position,
                        ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300, MinionTypes.All,
                        MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                foreach (var xMinion in xMinions)
                {
                    if (drawMinionLastHit.Active && ObjectManager.Player.GetAutoAttackDamage(xMinion, true) >=
                        xMinion.Health)
                    {
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionLastHit.Color);
                    }
                    else if (drawMinionNearKill.Active &&
                             ObjectManager.Player.GetAutoAttackDamage(xMinion, true)*2 >= xMinion.Health)
                    {
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionNearKill.Color);
                    }
                }
            }

            if (CClass != null)
            {
                CClass.Drawing_OnDraw(args);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Items.HasItem(3139) || Items.HasItem(3140))
                CheckChampionBuff();

            //Update the combo and harass values.
            CClass.ComboActive = CClass.Config.Item("Orbwalk").GetValue<KeyBind>().Active;

            var vHarassManaPer = Config.Item("HarassMana").GetValue<Slider>().Value;
            CClass.HarassActive = CClass.Config.Item("Farm").GetValue<KeyBind>().Active &&
                                  ObjectManager.Player.ManaPercent >= vHarassManaPer;

            CClass.ToggleActive = ObjectManager.Player.ManaPercent >= vHarassManaPer;

            var vLaneClearManaPer = Config.Item("LaneClearMana").GetValue<Slider>().Value;
            CClass.LaneClearActive = CClass.Config.Item("LaneClear").GetValue<KeyBind>().Active &&
                                     ObjectManager.Player.ManaPercent >= vLaneClearManaPer;

            CClass.Game_OnGameUpdate(args);

            UseSummoners();
            var useItemModes = Config.Item("UseItemsMode").GetValue<StringList>().SelectedIndex;

            //Items
            if (
                !((DeathWalker.Mode.Combo == DeathWalker.CurrentMode &&
                   (useItemModes == 2 || useItemModes == 3))
                  ||
                  (DeathWalker.Mode.LaneClear == DeathWalker.CurrentMode &&
                   (useItemModes == 1 || useItemModes == 3))))
                return;

            var botrk = Config.Item("BOTRK").GetValue<bool>();
            var ghostblade = Config.Item("GHOSTBLADE").GetValue<bool>();
            var sword = Config.Item("SWORD").GetValue<bool>();
            var muramana = Config.Item("MURAMANA").GetValue<bool>();
            var target = DeathWalker.getBestTarget() as Obj_AI_Base;

            var smiteReady = (SmiteSlot != SpellSlot.Unknown &&
                              ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready);

            if (smiteReady && DeathWalker.CurrentMode == DeathWalker.Mode.Combo)
                Smiteontarget(target as Obj_AI_Hero);

            if (botrk)
            {
                if (target != null && target.Type == ObjectManager.Player.Type &&
                    target.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550)
                {
                    var hasCutGlass = Items.HasItem(3144);
                    var hasBotrk = Items.HasItem(3153);

                    if (hasBotrk || hasCutGlass)
                    {
                        var itemId = hasCutGlass ? 3144 : 3153;
                        var damage = ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Botrk);
                        if (hasCutGlass || ObjectManager.Player.Health + damage < ObjectManager.Player.MaxHealth)
                            Items.UseItem(itemId, target);
                    }
                }
            }

            if (ghostblade && target != null && target.Type == ObjectManager.Player.Type &&
                !ObjectManager.Player.HasBuff("ItemSoTD", true) /*if Sword of the divine is not active */
                && DeathWalker.inAutoAttackRange(target))
                Items.UseItem(3142);

            if (sword && target != null && target.Type == ObjectManager.Player.Type &&
                !ObjectManager.Player.HasBuff("spectralfury", true) /*if ghostblade is not active*/
                && DeathWalker.inAutoAttackRange(target))
                Items.UseItem(3131);

            if (muramana && Items.HasItem(3042))
            {
                if (target != null && CClass.ComboActive &&
                    target.Position.Distance(ObjectManager.Player.Position) < 1200)
                {
                    if (!ObjectManager.Player.HasBuff("Muramana", true))
                    {
                        Items.UseItem(3042);
                    }
                }
                else
                {
                    if (ObjectManager.Player.HasBuff("Muramana", true))
                    {
                        Items.UseItem(3042);
                    }
                }
            }
        }

        public static void UseSummoners()
        {
            if (ObjectManager.Player.IsDead)
                return;

            const int xDangerousRange = 1100;

            if (Config.Item("SUMHEALENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerheal");
                var xCanUse = ObjectManager.Player.Health <=
                              ObjectManager.Player.MaxHealth/100*Config.Item("SUMHEALSLIDER").GetValue<Slider>().Value;

                if (xCanUse && !ObjectManager.Player.InShop() &&
                    (xSlot != SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready)
                    && ObjectManager.Player.CountEnemiesInRange(xDangerousRange) > 0)
                {
                    ObjectManager.Player.Spellbook.CastSpell(xSlot);
                }
            }

            if (Config.Item("SUMBARRIERENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerbarrier");
                var xCanUse = ObjectManager.Player.Health <=
                              ObjectManager.Player.MaxHealth/100*
                              Config.Item("SUMBARRIERSLIDER").GetValue<Slider>().Value;

                if (xCanUse && !ObjectManager.Player.InShop() &&
                    (xSlot != SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready)
                    && ObjectManager.Player.CountEnemiesInRange(xDangerousRange) > 0)
                {
                    ObjectManager.Player.Spellbook.CastSpell(xSlot);
                }
            }

            if (Config.Item("SUMIGNITEENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerdot");
                var t = DeathWalker.getBestTarget() as Obj_AI_Hero;

                if (t != null && xSlot != SpellSlot.Unknown &&
                    ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready)
                {
                    if (ObjectManager.Player.Distance(t) < 650 &&
                        ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) >=
                        t.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(xSlot, t);
                    }
                }
            }
        }

        private static void DeathWalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            CClass.DeathWalker_AfterAttack(unit, target);
        }

        private static void DeathWalker_BeforeAttack(DeathWalker.BeforeAttackEventArgs args)
        {
            CClass.DeathWalker_BeforeAttack(args);
        }

        private static void CheckChampionBuff()
        {
            var canUse3139 = Items.HasItem(3139) && Items.CanUseItem(3139);
            var canUse3140 = Items.HasItem(3140) && Items.CanUseItem(3140);

            foreach (var t1 in ObjectManager.Player.Buffs)
            {
                foreach (var t in QuickSilverMenu.Items)
                {
                    if (QuickSilverMenu.Item(t.Name).GetValue<bool>())
                    {
                        if (t1.Name.ToLower().Contains(t.Name.ToLower()))
                        {
                            var t2 = t1;
                            foreach (var bx in AActivator.BuffList.Where(bx => bx.BuffName == t2.Name))
                            {
                                if (bx.Delay > 0)
                                {
                                    if (ActivatorTime + bx.Delay < Game.Time)
                                        ActivatorTime = Game.Time;

                                    if (ActivatorTime + bx.Delay <= Game.Time)
                                    {
                                        if (canUse3139) 
                                            Items.UseItem(3139);
                                        else if (canUse3140) 
                                            Items.UseItem(3140);
                                        ActivatorTime = Game.Time;
                                    }
                                }
                                else
                                {
                                        if (canUse3139) 
                                            Items.UseItem(3139);
                                        else if (canUse3140) 
                                            Items.UseItem(3140);
                                }
                            }
                        }
                    }

                    if (QuickSilverMenu.Item("AnySlow").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Slow))
                    {
                        if (canUse3139) 
                            Items.UseItem(3139);
                        else if (canUse3140) 
                            Items.UseItem(3140);
                    }
                    if (QuickSilverMenu.Item("AnySnare").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Snare))
                    {
                        if (canUse3139) 
                            Items.UseItem(3139);
                        else if (canUse3140) 
                            Items.UseItem(3140);
                    }
                    if (QuickSilverMenu.Item("AnyStun").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Stun))
                    {
                        if (canUse3139) 
                            Items.UseItem(3139);
                        else if (canUse3140) 
                            Items.UseItem(3140);
                    }
                    if (QuickSilverMenu.Item("AnyTaunt").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Taunt))
                    {
                        if (canUse3139) 
                            Items.UseItem(3139);
                        else if (canUse3140) 
                            Items.UseItem(3140);
                    }
                }
            }
        }
        private static string Smitetype
        {
            get
            {
                if (SmiteBlue.Any(i => Items.HasItem(i)))
                    return "s5_summonersmiteplayerganker";

                if (SmiteRed.Any(i => Items.HasItem(i)))
                    return "s5_summonersmiteduel";

                if (SmiteGrey.Any(i => Items.HasItem(i)))
                    return "s5_summonersmitequick";

                if (SmitePurple.Any(i => Items.HasItem(i)))
                    return "itemsmiteaoe";

                return "summonersmite";
            }
        }

        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype, StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                Smite = new Spell(SmiteSlot, 700);
            }
        }

        private static void Smiteontarget(Obj_AI_Hero t)
        {
            var useSmite = Config.Item("ComboSmite").GetValue<bool>();
            var itemCheck = SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i));
            if (itemCheck && useSmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready &&
                t.Distance(ObjectManager.Player.Position) < Smite.Range)
            {
                ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, t);
            }
        }
    }
}
