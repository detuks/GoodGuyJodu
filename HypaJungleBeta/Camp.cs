using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    internal abstract class Camp
    {
        internal enum JungleCampState
        {
            Unknown,
            Dead,
            Alive
        }

        internal class JunMinStats
        {
            public float armor = 0;
            public float magicArmor = 0;
            public float attackSpeed = 0;
            public float attackDamage = 0;
            public float health = 0;
            public float critChance = 0;
            public float attackrange = 0;
            public float maxHp = 0;

            public float magicGoesThrough = 1;
            public float physicGoesThrough = 1;


        }

        internal class LevelTimeGrowth
        {
            public float armorPM =0;
            public float armorPL =0;
            public float magicArmorPL = 0;
            public float magicArmorPM = 0;
            public float attackSpeedPL = 0;
            public float attackSpeedPM = 0;
            public float attackDamagePL = 0;
            public float attackDamagePM = 0;
            public float healthPL = 0;
            public float healthPM = 0;
            public float critChancePL = 0;
            public float critChancePM = 0;
            public float attackrangePL = 0;
            public float attackrangePM = 0;
            public int[] hpMultiUp = {5, 11, 17};
            public int[] dmgMultiUp = {10};


        }

        internal class JungleMinion
        {
            internal enum SmiteBuff
            {
                RazorSharp,
                HeavyHands,
                NatureSpirit,
                ManaRestore,
                HealthRestore,
                None
            }

            internal enum Buff
            {
                CrestOfInsight,//ManaBuff
                CrestOfCinders,//Redbuff
                None
            }

            public JungleMinion(string name, JunMinStats baseStats, LevelTimeGrowth ltg,SmiteBuff sBuff, Buff buf)
            {
                Name = name;
                BaseStats = baseStats;
                smiteBuff = sBuff;
                buff = buf;
                LevelGrowth = ltg;
            }

            public JunMinStats BaseStats;
            public LevelTimeGrowth LevelGrowth;
            public JunMinStats UpdatedStats;
            public string Name;
            public Obj_AI_Minion unit;
            public SmiteBuff smiteBuff = SmiteBuff.None;
            public Buff buff = Buff.None;

            public void getMinionUnit(GameObject obj,Vector3 campPos)
            {
                if (obj.Name == Name && obj.Position.Distance(campPos,true)<1000*1000)
                    unit = (Obj_AI_Minion) obj;
            }

            public void calcUpdatedStats(int level)
            {
                float tHp = BaseStats.health, tdmg = BaseStats.attackDamage;
                float aaMult = 1, hpMult = 1;
                for (int i = 0; i < level; i++)
                {
                    if (LevelGrowth.hpMultiUp.Contains(i + 2))
                        hpMult ++;
                    if (LevelGrowth.dmgMultiUp.Contains(i + 2))
                        aaMult++;
                    tHp += LevelGrowth.healthPL * hpMult;
                    if (tHp > BaseStats.maxHp)
                        tHp = BaseStats.maxHp;
                    tdmg += LevelGrowth.attackDamagePL * aaMult;
                }

                var stats = new JunMinStats
                {
                    armor = BaseStats.armor + LevelGrowth.armorPL * level,
                    magicArmor = BaseStats.magicArmor + LevelGrowth.magicArmorPL * level,
                    attackSpeed = BaseStats.attackSpeed + LevelGrowth.attackSpeedPL * level,
                    attackDamage = tdmg,
                    health = tHp,
                    critChance = BaseStats.critChance + LevelGrowth.critChancePL * level,
                    attackrange = BaseStats.attackrange + LevelGrowth.attackrangePL * level,
                    magicGoesThrough = BaseStats.magicGoesThrough,
                    physicGoesThrough = BaseStats.physicGoesThrough
                };
                UpdatedStats = stats;
            }

            public float getDps()
            {
                return UpdatedStats.attackDamage * UpdatedStats.attackSpeed;
            }
            
        }


        public TimeSpan SpawnTime { get; set; }
        public TimeSpan RespawnTimer { get; set; }
        public Vector3 campPosition { get; set; }
        public bool useSafe = false;
        public Vector3 safePosition { get; set; }
        public List<JungleMinion> Minions { get; set; }
        public JungleCampState State { get; set; }
        public float ClearTick { get; set; }
        public bool isBuff;
        public bool isDrag;
        public byte campId;
        public int bonusPrio = 0;
        public int bonusPrioLowLvl = 0;
        public GameObjectTeam side;
        public bool useOverTime = true;
        public bool worthSmiting = true;


        public int level = 0;
        public float distToCamp = 0;
        public float timeToCamp = 0;
        public int priority = 0;

        public float hpLeftAfterFight = 0;
        public float timeToKill = 0;
        public bool willKillMe = false;
        public bool forceSmite = false;



        //abstract stuff

        protected Camp(byte campID,Vector3 pos, GameObjectTeam team)
        {
            campId = campID;
            campPosition = pos;
            side = team;
            State = JungleCampState.Unknown;
        }

        public void onRespawn()
        {
            level = getAvgLevel();
            Console.WriteLine("Level: "+level);
            foreach (var jungleMinion in Minions)
            {
                jungleMinion.calcUpdatedStats(level-2);
                Console.WriteLine(jungleMinion.Name+" hp: "+jungleMinion.UpdatedStats.health+" : "+jungleMinion.UpdatedStats.attackDamage);
            }
        }

        public int getAvgLevel()
        {
            return (int)ObjectManager.Get<Obj_AI_Hero>().Average(hero => hero.Level);
        }

        public void fullUpdate(Jungler jungler)
        {
            distToCamp = JungleClearer.getPathLenght(HypaJungle.player.GetPath(campPosition));
            timeToCamp = distToCamp/HypaJungle.player.MoveSpeed;
            jungler.FightSimulator.AroundFight(this, worthSmiting, jungler.gotAoeSmite(), true);
            priority = JungleClearer.getPriorityNumber(this);
        }

        public int aliveMinCount()
        {
            return Minions.Count(min => min.unit != null && !min.unit.IsDead);
        }

        public int visibleMinCount()
        {
            return Minions.Count(min => min.unit != null && !min.unit.IsDead && min.unit.IsVisible);
        }

        public int inAARangeMinCount()
        {
            return Minions.Count(min => min.unit != null && !min.unit.IsDead && min.unit.IsVisible && JungleOrbwalker.InAutoAttackRange(min.unit));
        }


        public abstract void customStatChanges(JunMinStats jms);
        public abstract bool canSmite();
    }
}

/**
 * 
 * 
 * 
 * 
 *  internal class CampPosition
        {
            private readonly Vector3 ChaosPosition;
            private readonly Vector3 OrderPosition;

            public CampPosition(Vector3 chaosPos, Vector3 orderPos)
            {
                ChaosPosition = chaosPos;
                OrderPosition = orderPos;
            }

            public Vector3 getMyCamp()
            {
                return JungleClearer.player.Team == GameObjectTeam.Chaos ? ChaosPosition : OrderPosition;
            }

            public Vector3 getEnemyCamp()
            {
                return JungleClearer.player.Team == GameObjectTeam.Chaos ? OrderPosition : ChaosPosition;
            }
        }
 * 
 * 
 * 
 */
