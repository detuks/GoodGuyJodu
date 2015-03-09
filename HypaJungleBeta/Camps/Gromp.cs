using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace HypaJungle.Camps
{
    class Gromp : Camp
    {

        public Gromp(byte campID, Vector3 pos, GameObjectTeam team)
            : base(campID, pos, team)
        {
            campId = campID;
            side = team;
            Minions = new List<JungleMinion>
            {
                get_SRU_Gromp(1)
            };
            SpawnTime = TimeSpan.FromSeconds(115);
            RespawnTimer = TimeSpan.FromSeconds(100);
            bonusPrioLowLvl = -50;
            bonusPrio = -20;
            onRespawn();
            worthSmiting = true;
        }



        public override void customStatChanges(JunMinStats jms)
        {

        }

        public override bool canSmite()
        {
            return true;
        }

        //Get one birdy
        private JungleMinion get_SRU_Gromp(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 1600,
                maxHp = 3200,
                attackDamage = 90,
                armor = 9,
                magicArmor = 0,
                attackSpeed = 0.63f,
                magicGoesThrough = 1,
                physicGoesThrough = 0.87f

            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 80,//find out
                attackDamagePL = 9f//find out
            };
            JungleMinion SRU_Murkwofl = new JungleMinion("SRU_Gromp" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.HeavyHands, JungleMinion.Buff.None);
            return SRU_Murkwofl;
        }

      
    }
}
