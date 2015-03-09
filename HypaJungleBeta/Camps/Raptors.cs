using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace HypaJungle.Camps
{
    class Raptors : Camp
    {

        public Raptors(byte campID, Vector3 pos, GameObjectTeam team) : base(campID, pos, team)
        {
            campId = campID;
            side = team;
            Minions = new List<JungleMinion>
            {
                get_SRU_Razorbeak(1),
                get_SRU_RazorbeakMini(2),
                get_SRU_RazorbeakMini(3),
                get_SRU_RazorbeakMini(4)
            };
            SpawnTime = TimeSpan.FromSeconds(115);
            RespawnTimer = TimeSpan.FromSeconds(100);
            bonusPrioLowLvl = -5;
            onRespawn();
            worthSmiting = false;
        }



        public override void customStatChanges(JunMinStats jms)
        {

        }

        public override bool canSmite()
        {
            return forceSmite;
        }

        //Get one birdy
        private JungleMinion get_SRU_Razorbeak(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 1200,
                maxHp = 2400,
                attackDamage = 55,
                armor = 15,
                magicArmor = 0,
                attackSpeed = 0.67f
            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 60,
                attackDamagePL = 5.5f
            };
            JungleMinion SRU_Razorbeak = new JungleMinion("SRU_Razorbeak" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.RazorSharp, JungleMinion.Buff.None);
            return SRU_Razorbeak;
        }

        private JungleMinion get_SRU_RazorbeakMini(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 250,
                maxHp = 250 * 2,
                attackDamage = 20,
                armor = 5,
                magicArmor = 0,
                attackSpeed = 0.67f
            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 12.5f,
                attackDamagePL = 2f
            };
            JungleMinion SRU_Razorbeak = new JungleMinion("SRU_RazorbeakMini" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.None, JungleMinion.Buff.None);
            return SRU_Razorbeak;
        }
    }
}
