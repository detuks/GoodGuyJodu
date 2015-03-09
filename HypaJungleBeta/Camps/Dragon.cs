using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace HypaJungle.Camps
{
    class Dragon : Camp
    {

        public Dragon(byte campID, Vector3 pos, GameObjectTeam team)
            : base(campID, pos, team)
        {
            campId = campID;
            side = team;
            Minions = new List<JungleMinion>
            {
                get_SRU_Dragon(1)
            };
            SpawnTime = TimeSpan.FromSeconds(150);
            RespawnTimer = TimeSpan.FromSeconds(360);
            bonusPrioLowLvl = -5;
            bonusPrio += 5;
            onRespawn();
            worthSmiting = false;
            isDrag = true;
        }



        public override void customStatChanges(JunMinStats jms)
        {
            jms.health += 240*(int) (Game.Time/60);
        }

        public override bool canSmite()
        {
            return true;
        }

        //Get one birdy
        private JungleMinion get_SRU_Dragon(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 3500,
                maxHp = 7000,
                attackDamage = 230,
                armor = 21,
                magicArmor = 0,
                attackSpeed = 0.3f
            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 0,
                attackDamagePL = 0f
            };
            JungleMinion SRU_Razorbeak = new JungleMinion("SRU_Dragon" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.RazorSharp, JungleMinion.Buff.None);
            return SRU_Razorbeak;
        }
    }
}
