using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    internal enum JungleCampState
    {
        Unknown,
        Dead,
        Alive
    }

    internal class CallOnce
    {
        public Action A(Action action)
        {
            var context = new Context();
            Action ret = () =>
            {
                if (!context.AlreadyCalled)
                {
                    action();
                    context.AlreadyCalled = true;
                }
            };

            return ret;
        }

        private class Context
        {
            public bool AlreadyCalled;
        }
    }

    internal class JungleCamp
    {
        public TimeSpan SpawnTime { get; set; }
        public TimeSpan RespawnTimer { get; set; }
        public Vector3 Position { get; set; }
        public List<JungleMinion> Minions { get; set; }
        public JungleCampState State { get; set; }
        public float ClearTick { get; set; }
        public bool isBuff;
        public bool isDragBaron;
        public int team;
        public int dps = 0;
        public int health = 0;
        public byte campId;
        public int bonusPrio = 0;

        public float timeToCamp = 0;
        public int priority = 0;
        public bool willKillMe = false;

    }

    internal class JungleMinion
    {
        public JungleMinion(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public bool Dead { get; set; }
        public GameObject Unit { get; set; }
    }

    internal class JungleTimers
    {
        public readonly List<JungleCamp> _jungleCamps = new List<JungleCamp>
        {
            new JungleCamp //Baron
            {
                SpawnTime = TimeSpan.FromSeconds(1200),
                RespawnTimer = TimeSpan.FromSeconds(420),
                Position = new Vector3(4549.126f, 10126.66f, -63.11666f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Worm12.1.1")
                },
                isBuff = false,
                isDragBaron = true,
                team = 2,
                dps = 99,
                health = 1500,
                campId = 12
            },
            new JungleCamp //Dragon
            {
                SpawnTime = TimeSpan.FromSeconds(150),
                RespawnTimer = TimeSpan.FromSeconds(360),
                Position = new Vector3(9606.835f, 4210.494f, -60.30991f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Dragon6.1.1")
                },
                isBuff = false,
                isDragBaron = true,
                team = 2,
                campId = 6
            },
            //Order
            new JungleCamp //Wight
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(2072.131f, 8450.272f, 51.92376f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Gromp13.1.1")
                },
                isBuff = false,
                isDragBaron = false,
                team = 0,
                dps = (int)(90*0.64f),
                health = 1600,
                campId = 13,
                bonusPrio = 3
            },
            new JungleCamp //Blue
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(310),
                Position = new Vector3(3820.156f, 7920.175f, 52.21874f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Blue1.1.1"),
                    new JungleMinion("SRU_BlueMini1.1.2"),
                    new JungleMinion("SRU_BlueMini21.1.3")
                },
                isBuff = true,
                isDragBaron = false,
                dps = (int)(80*0.64f),
                health = 2000,
                team = 0,
                campId = 1
            },
            new JungleCamp //Wolfs
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(3842.77f, 6462.637f, 52.60973f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Murkwolf2.1.1"),
                    new JungleMinion("SRU_MurkwolfMini2.1.2"),
                    new JungleMinion("SRU_MurkwolfMini2.1.3")
                },
                isBuff = false,
                isDragBaron = false,
                dps = (int)(73*0.59),
                health = 1320,
                team = 0,
                campId = 2
            },
            new JungleCamp //Wraith
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(6926.0f, 5400.0f, 51.0f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Razorbeak3.1.1"),
                    new JungleMinion("SRU_RazorbeakMini3.1.2"),
                    new JungleMinion("SRU_RazorbeakMini3.1.3"),
                    new JungleMinion("SRU_RazorbeakMini3.1.4")
                },
                isBuff = false,
                isDragBaron = false,
                dps = (int)(70*0.64f),
                health = 1600,
                team = 0,
                campId = 3
            },
            new JungleCamp //Red
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(7772.412f, 4108.053f, 53.867f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Red4.1.1"),
                    new JungleMinion("SRU_RedMini4.1.2"),
                    new JungleMinion("SRU_RedMini4.1.3")
                },
                isBuff = true,
                isDragBaron = false,
                dps = (int)(104*0.60f),
                health = 1800,
                team = 0,
                campId = 4
            },
            new JungleCamp //Golems
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(8404.148f, 2726.269f, 51.2764f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Krug5.1.2"),
                    new JungleMinion("SRU_KrugMini5.1.1")
                },
                isBuff = false,
                isDragBaron = false,
                dps = (int)(100*0.60f),
                health = 1440,
                team = 0,
                campId = 5,
                bonusPrio = 7
            },
            //Chaos
            new JungleCamp //Golems
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(6424.0f, 12156.0f, 56.62551f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Krug11.1.2"),
                    new JungleMinion("SRU_KrugMini11.1.1")
                },
                isBuff = true,
                isDragBaron = false,
                dps = (int)(100*0.60f),
                health = 1440,
                team = 1,
                campId = 11,
                bonusPrio = 3
            },
            new JungleCamp //Red
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(7086.157f, 10866.92f, 56.63499f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Red10.1.1"),
                    new JungleMinion("SRU_RedMini10.1.2"),
                    new JungleMinion("SRU_RedMini10.1.3")
                },
                isBuff = true,
                isDragBaron = false,
                dps = (int)(104*0.60f),
                health = 1800,
                team = 1,
                campId = 10
            },
            new JungleCamp //Wraith
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(7970.319f, 9410.513f, 52.50048f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Razorbeak9.1.1"),
                    new JungleMinion("SRU_RazorbeakMini9.1.2"),
                    new JungleMinion("SRU_RazorbeakMini9.1.3"),
                    new JungleMinion("SRU_RazorbeakMini9.1.4")
                },
                isBuff = false,
                isDragBaron = false,
                dps = (int)(70*0.64f),
                health = 1600,
                team = 1,
                campId = 9
            },
            new JungleCamp //Wolfs
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(10972.0f, 8306.0f, 62.5235f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Murkwolf8.1.1"),
                    new JungleMinion("SRU_MurkwolfMini8.1.2"),
                    new JungleMinion("SRU_MurkwolfMini8.1.3")
                },
                isBuff = false,
                isDragBaron = false,
                dps = (int)(73*0.59),
                health = 1320,
                team = 1,
                campId = 8
            },
            new JungleCamp //Blue
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(310),
                Position = new Vector3(10938.95f, 7000.918f, 51.8691f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Blue7.1.1"),
                    new JungleMinion("SRU_BlueMini7.1.2"),
                    new JungleMinion("SRU_BlueMini27.1.3")
                },
                isBuff = true,
                isDragBaron = false,
                dps = (int)(80*0.64f),
                health = 2000,
                team = 1,
                campId = 7

            },
            new JungleCamp //Wight
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(12770.0f, 6468.0f, 51.84151f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Gromp14.1.1")
                },
                isBuff = false,
                isDragBaron = false,
                team = 1,
                dps = (int)(90*0.64f),
                health = 1600,
                campId = 14,
                bonusPrio = 3
            },
             new JungleCamp //Crab
            {
                SpawnTime = TimeSpan.FromSeconds(150),
                RespawnTimer = TimeSpan.FromSeconds(180),
                Position = new Vector3(10218.0f, 5296.0f, -62.84151f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Sru_Crab15.1.1")
                },
                isBuff = false,
                isDragBaron = false,
                team = 3,
                campId = 15,
                bonusPrio = 3
            },
             new JungleCamp //Crab
            {
                SpawnTime = TimeSpan.FromSeconds(150),
                RespawnTimer = TimeSpan.FromSeconds(180),
                Position = new Vector3(5118.0f, 9200.0f, -71.84151f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Sru_Crab16.1.1")
                },
                isBuff = false,
                isDragBaron = false,
                team = 3,
                campId = 16,
                bonusPrio = 3
            }
        };

        private readonly Action _onLoadAction;

        public JungleTimers()
        {
            Console.WriteLine("Jungle timers onn");
            _onLoadAction = new CallOnce().A(OnLoad);
            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnLoad()
        {
            GameObject.OnCreate += ObjectOnCreate;
            GameObject.OnDelete += ObjectOnDelete;
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                _onLoadAction();
                UpdateCamps();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool closestJCUp(Vector3 pos)
        {
            JungleCamp closest = _jungleCamps.OrderBy(jc => Vector3.DistanceSquared(pos, jc.Position)).First();
            float delta = Game.Time - closest.ClearTick;
            if (delta < closest.RespawnTimer.TotalSeconds)
                return false;
            return true;
        }

        public JungleCamp getBestCampToGo()
        {
            float lessDist = float.MaxValue;
            JungleCamp bestCamp = null;
            foreach (var jungleCamp in _jungleCamps)
            {
                var distTillCamp = getPathLenght(HypaJungle.player.GetPath(jungleCamp.Position));
                var timeToCamp = distTillCamp/HypaJungle.player.MoveSpeed;
                float timeTillSpawn = Game.Time - jungleCamp.ClearTick;
                Console.WriteLine(jungleCamp.ClearTick + " : " + Game.Time);
                if (timeTillSpawn + timeToCamp > jungleCamp.RespawnTimer.TotalSeconds && lessDist > distTillCamp)
                {
                    lessDist = distTillCamp;
                    bestCamp = jungleCamp;
                }
            }
            return bestCamp;
        }

        public float getPathLenght(Vector3[] vecs)
        {
            float dist = 0;
            Vector3 from = vecs[0];
            foreach (var vec in vecs)
            {
                dist += Vector3.Distance(from, vec);
                from = vec;
            }
            return dist;
        }


        private void ObjectOnDelete(GameObject sender, EventArgs args)
        {
            try
            {
                if (sender.Type != GameObjectType.obj_AI_Minion)
                    return;

                var neutral = (Obj_AI_Minion)sender;
                if (neutral.Name.Contains("Minion") || !neutral.IsValid)
                    return;

                foreach (
                    JungleMinion minion in
                        from camp in _jungleCamps
                        from minion in camp.Minions
                        where minion.Name == neutral.Name
                        select minion)
                {
                    minion.Dead = neutral.IsDead;
                    minion.Unit = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void enableCamp(byte id)
        {
            foreach (var camp in _jungleCamps)
            {
                if (camp.campId == id)
                {
                    camp.ClearTick = 0;
                    camp.State = JungleCampState.Alive;
                }
            }
        }

        public void disableCamp(byte id)
        {
            foreach (var camp in _jungleCamps)
            {
                if (camp.campId == id)
                {
                    camp.ClearTick = Game.Time;
                    camp.State = JungleCampState.Dead;
                }
                if (JungleClearer.focusedCamp != null)
                {
                    if (camp.campId == JungleClearer.focusedCamp.campId)
                    {
                        if (HypaJungle.Config.Item("autoBuy").GetValue<bool>())
                            JungleClearer.jcState = JungleClearer.JungleCleanState.GoingToShop;
                        else
                            JungleClearer.jcState = JungleClearer.JungleCleanState.SearchingBestCamp;
                    }
                }
            }
        }

        private void ObjectOnCreate(GameObject sender, EventArgs args)
        {
            try
            {
                if (sender.Type != GameObjectType.obj_AI_Minion)
                    return;

                var neutral = (Obj_AI_Minion)sender;

                if (neutral.Name.Contains("Minion") || !neutral.IsValid)
                    return;

                foreach (
                    JungleMinion minion in
                        from camp in _jungleCamps
                        from minion in camp.Minions
                        where minion.Name == neutral.Name
                        select minion)
                {
                    minion.Unit = neutral;
                    minion.Dead = neutral.IsDead;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void setUpMinionsPlace(Obj_AI_Minion neutral)
        {
            foreach (
                   JungleMinion minion in
                       from camp in _jungleCamps
                       from minion in camp.Minions
                       where minion.Name == neutral.Name
                       select minion)
            {
                minion.Unit = neutral;
                minion.Dead = neutral.IsDead;
            }
        }

        private void UpdateCamps()
        {
            foreach (JungleCamp camp in _jungleCamps)
            {
                bool allAlive = true;
                bool allDead = true;

                bool gotLightNoMinions = true;

                foreach (JungleMinion minion in camp.Minions)
                {
                    if (minion.Unit != null)
                        minion.Dead = minion.Unit.IsDead;

                    if (NavMesh.LineOfSightTest(camp.Position, camp.Position) && minion.Unit == null)
                    {
                       // allAlive = false;
                    }
                    

                    if (minion.Dead)
                        allAlive = false;
                    else
                        allDead = false;
                }

                switch (camp.State)
                {
                    case JungleCampState.Alive:
                        if (allDead && camp.Position.Distance(HypaJungle.player.Position)<600)
                        {
                            camp.State = JungleCampState.Dead;
                            camp.ClearTick = Game.Time;
                            foreach (var min in camp.Minions)
                            {
                                min.Unit = null;
                                min.Dead = true;
                            }

                        }
                        break;
                }
            }
        }
    }
}
