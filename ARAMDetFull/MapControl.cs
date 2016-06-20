using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Geometry = LeagueSharp.Common.Geometry;
using SkillshotType = LeagueSharp.Common.SkillshotType;
using Spell = LeagueSharp.Common.Spell;
using ARAMDetFull.SpellsSDK;
using LeagueSharp.Data.DataTypes;
using LeagueSharp.Data.Enumerations;
using SpellDatabase = LeagueSharp.SDK.SpellDatabase;

namespace ARAMDetFull
{
    class MapControl
    {

        public static SpellSlot[] spellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };

        internal class ChampControl
        {
            public Obj_AI_Hero hero = null;

            public float reach = 0;

            public float dangerReach = 0;

            public int activeDangers = 0;
            
            protected List<SpellDatabaseEntry> champSpells = new List<SpellDatabaseEntry>();

            public ChampControl(Obj_AI_Hero champ)
            {
                hero = champ;
                foreach (var spell in
                SpellDatabase.Spells.Where(
                    s =>
                        s.ChampionName.Equals(champ.ChampionName)))
                {
                    champSpells.Add(spell);
                }

                getReach();
            }

            public float getReach()
            {
                dangerReach = 0;
                reach = (ARAMSimulator.player.Level < 7 && hero.IsEnemy) ? 750 : hero.AttackRange + 200;
                activeDangers = 0;

                foreach (var slot in spellSlots)
                {
                    var spell = hero.Spellbook.GetSpell(slot);
                    if ((spell.CooldownExpires - Game.Time) > 1.5f || hero.Spellbook.CanUseSpell(slot) == SpellState.NotLearned)
                        continue;
                    var range = (spell.SData.CastRange < 1000) ? spell.SData.CastRange : 1000;
                    if (spell.SData.CastRange > range)
                        reach = range;
                }

                /*foreach (var sShot in champSkillShots)
                {
                    if(!hero.Spellbook.GetSpell(sShot.Slot).IsReady())
                        continue;
                    float range = (sShot.Range < 1000) ? sShot.Range + sShot.Radius : 1000;
                    if (range > reach)
                        reach = range;

                    if (sShot.IsDangerous && dangerReach <= range)
                    {
                        activeDangers++;
                        dangerReach = range;
                    }
                }

                foreach (var tSpell in champTargSpells)
                {
                    if (!hero.Spellbook.GetSpell(tSpell.Spellslot).IsReady() || tSpell.Type == SpellType.Skillshot)
                        continue;
                    float range = (tSpell.Range < 1000) ? tSpell.Range+200 : 1000;
                    if (range > reach)
                        reach = range;

                    if (isDangerousTarg(tSpell) && dangerReach <= range)
                    {
                        activeDangers++;
                        dangerReach = range;
                    }
                }*/
                return reach;
            }

            public int getccCount()
            {
                return champSpells.Count(sShot => sShot.IsDangerous);
            }
        }

        internal class MyControl : ChampControl
        {

            private Dictionary<SpellDatabaseEntry, Spell> spells = new Dictionary<SpellDatabaseEntry, Spell>();

            public static Spellbook sBook = ObjectManager.Player.Spellbook;
            public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
            public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
            public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
            public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);

            public MyControl(Obj_AI_Hero champ) : base(champ)
            {
                try
                {
                    hero = champ;
                    foreach (var spell in champSpells)
                    {
                        var spl= new Spell(spell.Slot, spell.Range);
                        if ( spell.CastType.IsSkillShot())
                        {
                            bool coll = spell.CollisionObjects.Length>1;
                            spl.SetSkillshot(spell.Delay,spell.Radius,spell.MissileSpeed,coll,spell.SpellType.GetSkillshotType());
                        }
                        spells.Add(spell, spl);
                    }
                    getReach();
                    LXOrbwalker.farmRange = reach;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            public int bonusSpellBalance()
            {
                float manaUsed = 0;
                int bal = 0;
                foreach (var spell in spells)
                {
                    if (!spell.Value.IsReady())
                        continue;
                    manaUsed += spell.Value.ManaCost;
                    if (hero.MaxMana < 300 || hero.Mana- manaUsed>=0)
                    {
                        bal += (spell.Value.Slot == SpellSlot.R) ? 15 : 7;
                    }
                }
                return bal;
            }

            private int lastMinionSpellUse = LXOrbwalker.now;
            public void useSpellsOnMinions()
            {
                try
                {
                    if (lastMinionSpellUse + 277 > LXOrbwalker.now)
                        return;
                    lastMinionSpellUse = LXOrbwalker.now;
                    if (hero.MaxMana > 300 && hero.ManaPercent < 78)
                        return;
                    foreach (var spell in spells)
                    {
                        if (spell.Value.Instance.Cooldown > 15 || !spell.Value.IsReady() || spell.Value.ManaCost > hero.Mana || spell.Key.SpellTags == null || !spell.Key.SpellTags.Contains(SpellTags.Damage))
                            continue;
                        var minions = MinionManager.GetMinions((spell.Value.Range != 0) ? spell.Value.Range : 500);
                        foreach (var minion in minions)
                        {
                            if(minion.Health > spell.Value.GetDamage(minion))
                                continue;
                            var movementSpells = new List<SpellTags> { SpellTags.Dash, SpellTags.Blink, SpellTags.Teleport };
                            if (spell.Value.IsSkillshot)
                            {
                                if (!(spell.Key.SpellTags != null && spell.Key.SpellTags.Any(movementSpells.Contains)) || safeGap(minion))
                                {
                                    Console.WriteLine("Cast farm location: " + spell.Key.Slot);
                                    spell.Value.Cast(minion.Position);
                                    return;
                                }
                            }
                            else 
                            {
                                float range = (spell.Value.Range != 0) ? spell.Value.Range : 500;
                                if (spell.Key.CastType.Contains(CastType.Self))
                                {
                                    var bTarg = ARAMTargetSelector.getBestTarget(range, true);
                                    if (bTarg != null)
                                    {
                                        Console.WriteLine("Cast farm self: " + spell.Key.Slot);
                                         spell.Value.Cast();
                                        return;
                                    }
                                }
                                else if (spell.Key.CastType.Contains(CastType.EnemyMinions))
                                {
                                    if (minion != null)
                                    {
                                        if (!(spell.Key.SpellTags != null && spell.Key.SpellTags.Any(movementSpells.Contains)) || safeGap(minion))
                                        {
                                            Console.WriteLine("Cast farm target: " + spell.Key.Slot);
                                            spell.Value.CastOnUnit(minion);
                                            return;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                catch (Exception)
                {}

            }

            private int lastSpellUse = LXOrbwalker.now;
            public void useSpells()
            {
                try
                {
                    if (lastSpellUse + 277 > LXOrbwalker.now)
                        return;
                    lastSpellUse = LXOrbwalker.now;
                    foreach (var spell in spells)
                    {
                        if(!spell.Value.IsReady() || spell.Value.ManaCost > hero.Mana )
                            continue;
                        var movementSpells = new List<SpellTags> { SpellTags.Dash, SpellTags.Blink,SpellTags.Teleport };
                        var supportSpells = new List<SpellTags> { SpellTags.Shield, SpellTags.Heal, SpellTags.DamageAmplifier,
                            SpellTags.SpellShield, SpellTags.RemoveCrowdControl, };
                        if (spell.Value.IsSkillshot)
                        {
                                if (spell.Key.SpellTags != null && spell.Key.SpellTags.Any(movementSpells.Contains))
                                {
                                    if (hero.HealthPercent < 25 && hero.CountEnemiesInRange(400)>0)
                                    {
                                        Console.WriteLine("Cast esacpe location: " + spell.Key.Slot);
                                        spell.Value.Cast(hero.Position.Extend(ARAMSimulator.fromNex.Position, 1235));
                                        return;
                                    }
                                    else
                                    {
                                        var bTarg = ARAMTargetSelector.getBestTarget(spell.Value.Range, true);
                                        if (bTarg != null && safeGap(hero.Position.Extend(bTarg.Position,spell.Key.Range).To2D()))
                                        {
                                            Console.WriteLine("Cast attack location gap: " + spell.Key.Slot);
                                            spell.Value.CastIfHitchanceEquals(bTarg, HitChance.High);
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    var bTarg = ARAMTargetSelector.getBestTarget(spell.Value.Range, true);
                                    if (bTarg != null)
                                {
                                    Console.WriteLine("Cast attack location: " + spell.Key.Slot);
                                    spell.Value.CastIfHitchanceEquals(bTarg, HitChance.High);
                                        return;
                                    }
                                }
                        }
                        else
                        {
                            float range = (spell.Value.Range != 0) ? spell.Value.Range : 500;
                            if (spell.Key.CastType.Contains(CastType.Self) || spell.Key.CastType.Contains(CastType.Activate))
                            {
                                var bTarg = ARAMTargetSelector.getBestTarget(range, true);
                                if (bTarg != null)
                                {
                                    Console.WriteLine("Cast self: " + spell.Key.Slot);
                                    spell.Value.Cast();
                                    return;
                                }
                            }
                            else if(spell.Key.CastType.Contains(CastType.AllyChampions) && spell.Key.SpellTags != null && spell.Key.SpellTags.Any(supportSpells.Contains))
                            {
                                var bTarg = ARAMTargetSelector.getBestTargetAly(range, false);
                                if (bTarg != null)
                                {
                                    Console.WriteLine("Cast ally: " + spell.Key.Slot);
                                    spell.Value.CastOnUnit(bTarg);
                                    return;
                                }
                            }
                            else if (spell.Key.CastType.Contains(CastType.EnemyChampions))
                            {
                                var bTarg = ARAMTargetSelector.getBestTarget(range, true);
                                if (bTarg != null)
                                {
                                    if (!(spell.Key.SpellTags != null && spell.Key.SpellTags.Any(movementSpells.Contains)) || safeGap(bTarg))
                                    {
                                        Console.WriteLine("Cast enemy: " + spell.Key.Slot);
                                        spell.Value.CastOnUnit(bTarg);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception)
                { }
            }
            
            public float canDoDmgTo(Obj_AI_Base target, bool ignoreRange = false)
            {
                float dmgreal = 0;
                float mana = 0;
                foreach (var spell in spells.Values)
                {
                    try
                    {

                        if(spell == null || !spell.IsReady())
                            continue;

                        float dmg = 0;
                        var checkRange = spell.Range + 250;
                        if (ignoreRange || hero.Distance(target, true) < checkRange*checkRange)
                            dmg = spell.GetDamage(target);
                        if (dmg != 0)
                            mana += hero.Spellbook.GetSpell(spell.Slot).ManaCost;
                        if (hero.Mana > mana)
                            dmgreal += dmg;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                return dmgreal;
            }
        }

        public static int fearDistance
        {
            get
            {
                int assistValue = (ARAMSimulator.getType() == ARAMSimulator.ChampType.Support
                                   || ARAMSimulator.getType() == ARAMSimulator.ChampType.Tank ||
                                   ARAMSimulator.getType() == ARAMSimulator.ChampType.TankAS)
                    ? 30
                    : 20;
                int kdaScore = myControler.hero.ChampionsKilled*50 + myControler.hero.Assists* assistValue - myControler.hero.Deaths*50;
                int timeFear = 0;
                int healthFear = (int)(-(60 - myControler.hero.HealthPercent)*2);
                int score = kdaScore + timeFear +100;
                return (score < -550) ? -550 + healthFear : ((score > 500) ? 500 : score) + healthFear;
            }
        }

        public static List<ChampControl> enemy_champions = new List<ChampControl>();

        public static List<ChampControl> ally_champions = new List<ChampControl>();

        public static MyControl myControler;

        public static void setupMapControl()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if(hero.IsMe)
                    continue;

                if(hero.IsAlly)
                    ally_champions.Add(new ChampControl(hero));

                if (hero.IsEnemy)
                    enemy_champions.Add(new ChampControl(hero));
            }
            myControler = new MyControl(ObjectManager.Player);
        }


        public static bool inDanger()
        {
            int enesAround = enemy_champions.Count(ene => !ene.hero.IsDead && Utility.IsValidTarget(ene.hero, 1300));
            int allyAround = ally_champions.Count(aly => !aly.hero.IsDead && Utility.IsValidTarget(aly.hero, 700));
            return (enesAround - allyAround) > 1;
        }

        public static Obj_AI_Hero fightIsOn()
        {
            foreach (var enem in enemy_champions.Where(ene => !ene.hero.IsDead && ene.hero.IsVisible).OrderBy(ene => ene.hero.Distance(ObjectManager.Player,true)))
            {
                if (myControler.canDoDmgTo(enem.hero) > enem.hero.Health+250)
                    return enem.hero;

                if (ally_champions.Where(ene => !ene.hero.IsDead && !ene.hero.IsMe).Any(ally => enem.hero.Distance(ally.hero, true) < 500*500))
                {
                    return enem.hero;
                }
            }

            return null;
        }

        public static bool fightIsClose()
        {
            foreach (var enem in enemy_champions.Where(ene => !ene.hero.IsDead && ene.hero.IsVisible).OrderBy(ene => ene.hero.Distance(ObjectManager.Player, true)))
            {

                if (ally_champions.Where(ene => !ene.hero.IsDead && !ene.hero.IsMe).Any(ally => enem.hero.Distance(ally.hero, true) < 400 * 400))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool fightIsOn(Obj_AI_Base target)
        {
            if (myControler.canDoDmgTo(target)*0.75 > target.Health)
                    return true;

                if (ally_champions.Where(ene => !ene.hero.IsDead && !ene.hero.IsMe).Any(ally => target.Distance(ally.hero, true) < 300 * 300))
                {
                    return true;
                }

            return false;
        }


        public static int enemiesAroundPoint(Vector2 point, float range)
        {
            int count = 0;
            foreach (var ene in enemy_champions.Where(ene=>!ene.hero.IsDead))
            {
                if (ene.hero.Distance(point, true) < range*range)
                    count++;
            }
            return count;
        }

        public static int balanceAroundPoint(Vector2 point, float range)
        {
            int balance = 0;
            balance -= enemy_champions.Where(ene => !ene.hero.IsDead).Count(ene => ene.hero.Distance(point, true) < range * range);

            balance += ally_champions.Where(aly => !aly.hero.IsDead).Count(aly => aly.hero.Distance(point, true) < (range - 150) * (range - 150));
            return balance;
        }

        public static int balanceAroundPointAdvanced(Vector2 point, float rangePlus)
        {
            int balance = (point.To3D().UnderTurret(true)) ? -80 : (point.To3D().UnderTurret(false)) ? 80 : 0;
            foreach (var ene in enemy_champions)
            {
                var reach = ene.reach + rangePlus;
                if (!ene.hero.IsDead && ene.hero.Distance(point, true) < reach* reach && !unitIsUseless(ene.hero) && !notVisibleAndMostLieklyNotThere(ene.hero))
                {
                    balance -= (int) ((ene.hero.HealthPercent + 20 - ene.hero.Deaths*4 + ene.hero.ChampionsKilled*4)*
                                      ((ARAMSimulator.player.Level < 7)
                                          ? 1.3f
                                          : 1f));
                }
            }


            foreach (var aly in ally_champions)
            {
                var reach = (aly.reach-200<500)?500:(aly.reach - 200);
                if (!aly.hero.IsDead && /*aly.hero.Distance(point, true) < reach * reach &&*/
                    (Geometry.Distance(aly.hero, ARAMSimulator.toNex.Position) + reach < (Geometry.Distance(point, ARAMSimulator.toNex.Position) + fearDistance + (ARAMSimulator.tankBal * -5) + (ARAMSimulator.agrobalance * 3))))
                    balance += ((int)aly.hero.HealthPercent + 20 + 20 - aly.hero.Deaths * 4 + aly.hero.ChampionsKilled * 4);
            }
            var myBal = ((int)myControler.hero.HealthPercent + 20 + 20 - myControler.hero.Deaths * 10 +
                         myControler.hero.ChampionsKilled*10) + myControler.bonusSpellBalance();
            balance += (myBal<0)?10:myBal;
            return balance;
        }

        public static double unitIsUselessFor(Obj_AI_Base unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare || buff.Type == BuffType.Fear))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (result - Game.Time);
        }

        public static bool unitIsUseless(Obj_AI_Base unit)
        {
            return unitIsUselessFor(unit) > 0.7;
        }

        public static bool notVisibleAndMostLieklyNotThere(Obj_AI_Base unit)
        {
            var distEneNex = Geometry.Distance(ARAMSimulator.toNex.Position, unit.Position);
            var distEneNexDeepest = Geometry.Distance(ARAMSimulator.toNex.Position, ARAMSimulator.deepestAlly.Position);

            return !ARAMSimulator.deepestAlly.IsDead && distEneNexDeepest + 1500 < distEneNex;
        }

        public static ChampControl getByObj(Obj_AI_Base champ)
        {
            return enemy_champions.FirstOrDefault(ene => ene.hero.NetworkId == champ.NetworkId);
        }

        public static bool safeGap(Obj_AI_Base target)
        {
            return safeGap(target.Position.To2D()) || MapControl.fightIsOn(target) || (!ARAMTargetSelector.IsInvulnerable(target) && target.Health < myControler.canDoDmgTo(target,true)/2);
        }

        public static bool safeGap(Vector2 position)
        {
            return myControler.hero.HealthPercent < 13 || (!Sector.inTowerRange(position) &&
                   (MapControl.balanceAroundPointAdvanced(position, 500) > 0)) || position.Distance(ARAMSimulator.fromNex.Position, true) < myControler.hero.Position.Distance(ARAMSimulator.fromNex.Position, true);
        }

        public static List<int> usedRelics = new List<int>();

        public static Obj_AI_Base ClosestRelic()
        {
            var closesEnem = ClosestEnemyTobase();
            //var closesEnemTower = ClosestEnemyTobase();
            var hprelics = ObjectManager.Get<Obj_AI_Base>().Where(
                r => r.IsValid && !r.IsDead && (r.Name.Contains("HealthRelic") || r.Name.Contains("BardChime") || (r.Name.Contains("BardPickup") && ObjectManager.Player.ChampionName == "Bard")) 
                    && !usedRelics.Contains(r.NetworkId) && (closesEnem == null || r.Distance(ARAMSimulator.fromNex.Position, true) - 500 < closesEnem.Distance(ARAMSimulator.fromNex.Position, true))).ToList().OrderBy(r => ARAMSimulator.player.Distance(r, true));
            return hprelics.FirstOrDefault();
        }

        public static Obj_AI_Base ClosestEnemyTobase()
        {
            return
                HeroManager.Enemies
                    .Where(h => h.IsValid && !h.IsDead && h.IsVisible && h.IsEnemy)
                    .OrderBy(h => h.Distance(ARAMSimulator.fromNex.Position, true))
                    .FirstOrDefault();
        }

        public static Obj_AI_Base ClosestEnemyTower()
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => !tur.IsDead)
                    .OrderBy(tur => tur.Distance(ObjectManager.Player.Position, true))
                    .FirstOrDefault();
        }


        /* LOGIC!!
         * 
         * Go to Kill minions
         * If no minions go for enemy tower
         * Cut path on enemies range
         * 
         * Orbwalk all the way
         * 
         * 
         * 
         */

    }
}
