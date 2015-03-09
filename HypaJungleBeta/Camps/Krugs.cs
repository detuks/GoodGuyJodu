using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace HypaJungle.Camps
{
    class Krugs :Camp
    {
        public Krugs(byte campID, Vector3 pos, GameObjectTeam team)
            : base(campID, pos, team)
        {
            campId = campID;
            side = team;
            Minions = new List<JungleMinion>
            {
                get_SRU_Krug(2),
                get_SRU_KrugMini(1),
            };
            SpawnTime = TimeSpan.FromSeconds(115);
            RespawnTimer = TimeSpan.FromSeconds(100);
            bonusPrioLowLvl = 6;
            bonusPrio = 4;
            worthSmiting = true;
            useOverTime = true;
            onRespawn();
        }



        public override void customStatChanges(JunMinStats jms)
        {

        }

        public override bool canSmite()
        {
            return true;
        }

        //Get one birdy
        private JungleMinion get_SRU_Krug(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 1440,
                maxHp = 2880,
                attackDamage = 73,
                armor = 12,
                magicArmor = 0,
                attackSpeed = 0.61f,
                magicGoesThrough = 0.89f,
                physicGoesThrough = 1.09f

            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 72,//find out
                attackDamagePL = 7.5f//find out
            };
            JungleMinion SRU_Razorbeak = new JungleMinion("SRU_Krug" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.HeavyHands, JungleMinion.Buff.None);
            return SRU_Razorbeak;
        }

        private JungleMinion get_SRU_KrugMini(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 540,
                maxHp = 540 * 2,
                attackDamage = 35,
                armor = 12,
                magicArmor = 0,
                attackSpeed = 0.61f,
                magicGoesThrough = 1.09f,
                physicGoesThrough = 0.89f
            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 27,
                attackDamagePL = 3.5f
            };
            JungleMinion SRU_Razorbeak = new JungleMinion("SRU_KrugMini" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.None, JungleMinion.Buff.None);
            return SRU_Razorbeak;
        }
    }
}
