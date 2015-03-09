using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace HypaJungle.Camps
{
    class RedBrambleback : Camp
    {

        public RedBrambleback(byte campID, Vector3 pos, GameObjectTeam team)
            : base(campID, pos, team)
        {
            campId = campID;
            side = team;
            Minions = new List<JungleMinion>
            {
                get_SRU_Red(1),
                get_SRU_RedMini(2),
                get_SRU_RedMini(3),
            };
            SpawnTime = TimeSpan.FromSeconds(115);
            RespawnTimer = TimeSpan.FromSeconds(300);
            useOverTime = false;
            bonusPrioLowLvl = -1;
            onRespawn();
        }



        public override void customStatChanges(JunMinStats jms)
        {

        }

        public override bool canSmite()
        {
            return HypaJungle.player.Health / HypaJungle.player.MaxHealth <= 0.80;
        }

        //Get one birdy
        private JungleMinion get_SRU_Red(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 1800,
                maxHp = 4000,
                attackDamage = 80,
                armor = 20,
                magicArmor = 0,
                attackSpeed = 0.60f,
                magicGoesThrough = 1,
                physicGoesThrough = 0.83f

            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 90,//find out
                attackDamagePL = 9f//find out
            };
            JungleMinion SRU_Murkwofl = new JungleMinion("SRU_Red" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.HeavyHands, JungleMinion.Buff.None);
            return SRU_Murkwofl;
        }

        private JungleMinion get_SRU_RedMini(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 400,
                maxHp = 400 * 2,
                attackDamage = 12,
                armor = 8,
                magicArmor = 0,
                attackSpeed = 0.60f,
                physicGoesThrough = 0.93f
            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 20f,
                attackDamagePL = 1f
            };
            JungleMinion SRU_MurkwoflMini = new JungleMinion("SRU_RedMini" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.None, JungleMinion.Buff.None);
            return SRU_MurkwoflMini;
        }
    }
}
