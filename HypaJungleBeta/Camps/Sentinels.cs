using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle.Camps
{
    class Senitels : Camp
    {

        public Senitels(byte campID, Vector3 pos, GameObjectTeam team)
            : base(campID, pos, team)
        {
            campId = campID;
            side = team;
            Minions = new List<JungleMinion>
            {
                get_SRU_Blue(1),
                get_SRU_BlueMini(2),
                get_SRU_BlueMini(3,true),
            };
            SpawnTime = TimeSpan.FromSeconds(115);
            RespawnTimer = TimeSpan.FromSeconds(300);
            useOverTime = false;
            onRespawn();
        }



        public override void customStatChanges(JunMinStats jms)
        {

        }

        public override bool canSmite()
        {
            return HypaJungle.player.Mana / HypaJungle.player.MaxMana<=0.80 || forceSmite;
        }

        //Get one birdy
        private JungleMinion get_SRU_Blue(int count)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 2000,
                maxHp = 4000,
                attackDamage = 73,
                armor = 20,
                magicArmor = 0,
                attackSpeed = 0.49f,
                magicGoesThrough = 1,
                physicGoesThrough = 0.83f

            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 100,//find out
                attackDamagePL = 7.5f//find out
            };
            JungleMinion SRU_Murkwofl = new JungleMinion("SRU_Blue" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.HeavyHands, JungleMinion.Buff.None);
            return SRU_Murkwofl;
        }

        private JungleMinion get_SRU_BlueMini(int count,bool lolRito = false)
        {
            JunMinStats bStats = new JunMinStats
            {
                health = 400,
                maxHp = 400 * 2,
                attackDamage = 12,
                armor = 8,
                magicArmor = 0,
                attackSpeed = 0.63f,
                physicGoesThrough = 0.93f
            };
            LevelTimeGrowth ltg = new LevelTimeGrowth
            {
                healthPL = 20f,
                attackDamagePL = 1f
            };
            JungleMinion SRU_MurkwoflMini;
            if(lolRito)
                SRU_MurkwoflMini = new JungleMinion("SRU_BlueMini2" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.None, JungleMinion.Buff.None);
            else
                SRU_MurkwoflMini = new JungleMinion("SRU_BlueMini" + campId + ".1." + count + "", bStats, ltg, JungleMinion.SmiteBuff.None, JungleMinion.Buff.None);
            
            return SRU_MurkwoflMini;
        }
    }
}
