using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;



//Ignore this class for now!!
namespace HypaJungle
{
    class GameCamp
    {
        public Camp camp;
        public JungleCampState State { get; set; }
        public int level = 0;
        public float timeToCamp = 0;
        public int priority = 0;
        public bool willKillMe = false;
        public byte campId;


        public TimeSpan SpawnTime { get; set; }
        public TimeSpan RespawnTimer { get; set; }
        public Vector3 campPosition { get; set; }
        public List<Camp.JungleMinion> Minions { get; set; }

        public void onRespawn()
        {
            level = getAvgLevel();
            foreach (var jungleMinion in Minions)
            {
                jungleMinion.calcUpdatedStats(level);
            }
        }

        public int getAvgLevel()
        {
            return (int)ObjectManager.Get<Obj_AI_Hero>().Average(hero => hero.Level);
        }


    }
}
