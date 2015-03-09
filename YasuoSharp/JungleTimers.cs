using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace YasuoSharp
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
        private readonly List<JungleCamp> _jungleCamps = new List<JungleCamp>
        {
            new JungleCamp //Baron
            {
                SpawnTime = TimeSpan.FromSeconds(900),
                RespawnTimer = TimeSpan.FromSeconds(420),
                Position = new Vector3(4549.126f, 10126.66f, -63.11666f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Worm12.1.1")
                }
            },
            new JungleCamp //Dragon
            {
                SpawnTime = TimeSpan.FromSeconds(150),
                RespawnTimer = TimeSpan.FromSeconds(360),
                Position = new Vector3(9606.835f, 4210.494f, -60.30991f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Dragon6.1.1")
                }
            },
            //Order
            new JungleCamp //Wight
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(1859.131f, 8246.272f, 54.92376f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("GreatWraith13.1.1")
                }
            },
            new JungleCamp //Blue
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(3388.156f, 7697.175f, 55.21874f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("AncientGolem1.1.1"),
                    new JungleMinion("YoungLizard1.1.2"),
                    new JungleMinion("YoungLizard1.1.3")
                }
            },
            new JungleCamp //Wolfs
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(3415.77f, 6269.637f, 55.60973f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("GiantWolf2.1.1"),
                    new JungleMinion("Wolf2.1.2"),
                    new JungleMinion("Wolf2.1.3")
                }
            },
            new JungleCamp //Wraith
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(6447.0f, 5384.0f, 60.0f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Wraith3.1.1"),
                    new JungleMinion("LesserWraith3.1.2"),
                    new JungleMinion("LesserWraith3.1.3"),
                    new JungleMinion("LesserWraith3.1.4")
                }
            },
            new JungleCamp //Red
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(7509.412f, 3977.053f, 56.867f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("LizardElder4.1.1"),
                    new JungleMinion("YoungLizard4.1.2"),
                    new JungleMinion("YoungLizard4.1.3")
                }
            },
            new JungleCamp //Golems
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(8042.148f, 2274.269f, 54.2764f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Golem5.1.2"),
                    new JungleMinion("SmallGolem5.1.1")
                }
            },
            //Chaos
            new JungleCamp //Golems
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(6005.0f, 12055.0f, 39.62551f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Golem11.1.2"),
                    new JungleMinion("SmallGolem11.1.1")
                }
            },
            new JungleCamp //Red
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(6558.157f, 10524.92f, 54.63499f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("LizardElder10.1.1"),
                    new JungleMinion("YoungLizard10.1.2"),
                    new JungleMinion("YoungLizard10.1.3")
                }
            },
            new JungleCamp //Wraith
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(7534.319f, 9226.513f, 55.50048f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Wraith9.1.1"),
                    new JungleMinion("LesserWraith9.1.2"),
                    new JungleMinion("LesserWraith9.1.3"),
                    new JungleMinion("LesserWraith9.1.4")
                }
            },
            new JungleCamp //Wolfs
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(10575.0f, 8083.0f, 65.5235f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("GiantWolf8.1.1"),
                    new JungleMinion("Wolf8.1.2"),
                    new JungleMinion("Wolf8.1.3")
                }
            },
            new JungleCamp //Blue
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(10439.95f, 6717.918f, 54.8691f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("AncientGolem7.1.1"),
                    new JungleMinion("YoungLizard7.1.2"),
                    new JungleMinion("YoungLizard7.1.3")
                }
            },
            new JungleCamp //Wight
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(50),
                Position = new Vector3(12287.0f, 6205.0f, 54.84151f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("GreatWraith14.1.1")
                }
            }
        };

        private readonly Action _onLoadAction;

        public JungleTimers()
        {
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

        private void UpdateCamps()
        {
            foreach (JungleCamp camp in _jungleCamps)
            {
                bool allAlive = true;
                bool allDead = true;

                foreach (JungleMinion minion in camp.Minions)
                {
                    if (minion.Unit != null)
                        minion.Dead = minion.Unit.IsDead;

                    if (minion.Dead)
                        allAlive = false;
                    else
                        allDead = false;
                }

                switch (camp.State)
                {
                    case JungleCampState.Unknown:
                        if (allAlive)
                        {
                            camp.State = JungleCampState.Alive;
                            camp.ClearTick = 0.0f;
                        }
                        break;
                    case JungleCampState.Dead:
                        if (allAlive)
                        {
                            camp.State = JungleCampState.Alive;
                            camp.ClearTick = 0.0f;
                        }
                        break;
                    case JungleCampState.Alive:
                        if (allDead)
                        {
                            camp.State = JungleCampState.Dead;
                            camp.ClearTick = Game.Time;
                        }
                        break;
                }
            }
        }
    }
}
