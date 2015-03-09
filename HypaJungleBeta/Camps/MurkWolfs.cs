using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace HypaJungle.Camps
{
    class MurkWolfs : Camp
    {

        public MurkWolfs(byte campID, Vector3 pos, GameObjectTeam team)
            : base(campID, pos, team)
        {
            campId = campID;
            side = team;
            Minions = new List<JungleMinion>
            {
                get_SRU_Murkwofl(1),
                get_SRU_MurkwoflMini(2),
                get_SRU_MurkwoflMini(3),
            };
            SpawnTime = TimeSpan.FromSeconds(115);
            RespawnTimer = TimeSpan.FromSeconds(100);
            bonusPrioLowLvl = 3;
            onRespawn();
            worthSmiting = false;
            useOverTime = true;
        }



        public override void customStatChanges(JunMinStats jms)
        {

        }

        public override bool canSmite()
        {
            return forceSmite;
        }

        //Get one birdy
        private JungleMinion get_SRU_Murkwofl(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 1320,
                maxHp = 2640,
                attackDamage = 42,
                armor = 9,
                magicArmor = 0,
                attackSpeed = 0.63f,
                magicGoesThrough = 1,
                physicGoesThrough = 0.92f

            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 66,//find out
                attackDamagePL = 4.5f//find out
            };
            JungleMinion SRU_Murkwofl = new JungleMinion("SRU_Murkwolf" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.HeavyHands, JungleMinion.Buff.None);
            return SRU_Murkwofl;
        }

        private JungleMinion get_SRU_MurkwoflMini(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 420,
                maxHp = 420 * 2,
                attackDamage = 16,
                armor = 6,
                magicArmor = 0,
                attackSpeed = 0.67f,
                physicGoesThrough = 0.94f
            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 21f,
                attackDamagePL = 2f
            };
            JungleMinion SRU_MurkwoflMini = new JungleMinion("SRU_MurkwolfMini" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.None, JungleMinion.Buff.None);
            return SRU_MurkwoflMini;
        }
    }
}
