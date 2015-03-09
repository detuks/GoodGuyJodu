using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    class JungleClearer
    {
        public enum JungleCleanState
        {
            AttackingMinions,
            WaitingMinions,
            RunningToCamp,
            SearchingBestCamp,
            GoingToShop,
            DoingDragon,
            RecallForHeal,
            ThinkAfterFinishCamp
        }

        public static List<String> supportedChamps = new List<string> { "MasterYi", "Warwick", "Twitch", "Aatrox", "Jinx" }; 


        public static Obj_AI_Hero player = ObjectManager.Player;

        public static Camp focusedCamp;

        public static bool recalCasted = true;


        public static Camp skipCamp;

        public static JungleCleanState jcState = JungleCleanState.GoingToShop;

        public static Jungler jungler = new MasterYi();

        public static bool canMoveLast = true;

        public static void setUpJCleaner()
        {
            switch (player.ChampionName.ToLower())
            {
                case "masteryi":
                    jungler = new MasterYi();
                    Game.PrintChat("MasterYi loaded");
                    break;
                case("warwick"):
                    jungler = new Warwick();
                    Game.PrintChat("Warwick loaded");
                    break;
                case ("twitch"):
                    jungler = new Twitch();
                    Game.PrintChat("Twitch loaded");
                    break;
                case ("aatrox"):
                    jungler = new Aatrox();
                    Game.PrintChat("Aatrox loaded");
                    break;
                case ("jinx"):
                    jungler = new Jinx();
                    Game.PrintChat("Jinx loaded");
                    break;
                /*case "udyr":
                    jungler = new Udyr();
                    Game.PrintChat("Udyr loaded");
                    break;
                case "shyvana":
                    jungler = new Shyvana();
                    Game.PrintChat("Shyvana loaded");
                    break;
                case "leesin":
                    jungler = new LeeSin();
                    Game.PrintChat("LeeSin loaded");
                    break;
                case "amumu":
                    jungler = new Amumu();
                    Game.PrintChat("Amumu loaded");
                    break;
                case "rengar":
                    jungler = new Rengar();
                    Game.PrintChat("Rengar loaded");
                    break;*/
            }

            Game.PrintChat("Other junglers coming soon!");

        }

        public static void updateJungleCleaner()
        {
            if (player.IsDead)
            {
                jcState = JungleCleanState.RecallForHeal;
                //Console.WriteLine("RecallForHeal");
                return;
            }

            if (jcState == JungleCleanState.SearchingBestCamp)
            {
                focusedCamp = getBestCampToGo();
                if (focusedCamp != null)
                {
                    if (focusedCamp.priority >= 20)
                    {
                        GamePacket gPacketT;
                        gPacketT = Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(player.Position[0], player.Position[1], 0, 0, Packet.PingType.Normal));
                        gPacketT.Process();

                        gPacketT = Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(player.Position[0], player.Position[1], 0, 0, Packet.PingType.Fallback));
                        gPacketT.Process();
                    }

                    Console.WriteLine("New camp found "+focusedCamp);
                    Console.WriteLine("Time to finish camp " + focusedCamp.timeToKill);
                    Console.WriteLine("HP left after camp " + focusedCamp.hpLeftAfterFight);
                    Console.WriteLine("Camp level " + focusedCamp.level);
                    //puss out or kill?
                    if ( (focusedCamp.willKillMe  || (focusedCamp.priority>25 && player.Health/player.MaxHealth<0.85f)))
                    {
                        Console.WriteLine("gona diee");
                        jcState = JungleCleanState.RecallForHeal;
                    }
                    else
                    {
                        jcState = JungleCleanState.RunningToCamp;
                        Console.WriteLine("RunningToCamp");
                    }
                }
                else
                {
                    jcState = JungleCleanState.RecallForHeal;
                    Console.WriteLine("RecallForHeal");
                }
            }

            if (jcState == JungleCleanState.RunningToCamp)
            {
                if (focusedCamp.State != Camp.JungleCampState.Dead && focusedCamp.side != GameObjectTeam.Neutral)
                    jungler.castWhenNear(focusedCamp);
                jungler.checkItems();
                logicRunToCamp();
            }

            if (jcState == JungleCleanState.RunningToCamp && jungler.canMove() &&(HypaJungle.player.Position.Distance(focusedCamp.campPosition) < 200 || isCampVisible()))
            {
                jcState = JungleCleanState.WaitingMinions;
                Console.WriteLine("WaitingMinions");
            }

            if (jcState == JungleCleanState.WaitingMinions)
            {
                doWhileIdling();
            }

            if (jcState == JungleCleanState.WaitingMinions && (isCampVisible()))
            {
                jcState = JungleCleanState.AttackingMinions;
                Console.WriteLine("AttackingMinions");
            }

            if (jcState == JungleCleanState.AttackingMinions)
            {
                attackCampMinions();
                if (focusedCamp.inAARangeMinCount() == 0 && !player.IsMelee())
                    player.IssueOrder(GameObjectOrder.MoveTo, focusedCamp.campPosition);
            }

            if (jcState == JungleCleanState.AttackingMinions && isCampFinished())
            {
                if (HypaJungle.Config.Item("autoBuy").GetValue<bool>())
                {
                    jcState = JungleCleanState.GoingToShop;
                    Console.WriteLine("GoingToShop");
                }
                else
                {
                    jcState = JungleCleanState.SearchingBestCamp;
                    Console.WriteLine("SearchingBestCamp");
                }
            }

            if (jcState == JungleCleanState.ThinkAfterFinishCamp)
            {
                jcState = JungleCleanState.SearchingBestCamp;
                Console.WriteLine("SearchingBestCamp");

            }

            if (jcState == JungleCleanState.RecallForHeal)
            {
                if (jungler.recall.IsReady() && !player.IsChanneling && !jungler.inSpwan() && !recalCasted)
                {
                    jungler.recall.Cast();
                    recalCasted = true;
                }

                if (jungler.inSpwan())
                {
                    if (HypaJungle.Config.Item("autoBuy").GetValue<bool>())
                    {
                        jcState = JungleCleanState.GoingToShop;
                    }
                    else
                    {
                        if (jungler.inSpwan() && player.Health > player.MaxHealth*0.7f &&
                            (!jungler.gotMana || player.Mana > player.MaxMana*0.7f))
                        {
                            jcState = JungleCleanState.SearchingBestCamp;
                            Console.WriteLine("SearchingBestCamp");
                        }
                    }
                }


            }

            if (jcState == JungleCleanState.GoingToShop)
            {
                if (!HypaJungle.Config.Item("autoBuy").GetValue<bool>())
                {
                    jcState = JungleCleanState.SearchingBestCamp;
                    Console.WriteLine("SearchingBestCamp");
                }

                if (jungler.inSpwan())
                {
                    jungler.getItemPassiveBoostDps();
                    jungler.setupSmite();
                }

                if (jungler.inSpwan() && player.IsChanneling)
                {
                    Vector3 stopRecPos = new Vector3(6, 30, 2);
                    player.IssueOrder(GameObjectOrder.MoveTo, player.Position + stopRecPos);
                }

                if (jungler.nextItem != null && player.GoldCurrent-12 >= jungler.nextItem.goldReach)
                {
                    if (jungler.recall.IsReady() && !player.IsChanneling && !jungler.inSpwan() && !recalCasted)
                    {
                        jungler.recall.Cast();
                        recalCasted = true;
                    }
                }
                else
                {
                    if (jungler.inSpwan() && player.Health > player.MaxHealth*0.8f &&
                        (!jungler.gotMana || player.Mana > player.MaxMana * 0.8f) && (jungler.nextItem == null || player.GoldCurrent+40 <= jungler.nextItem.goldReach))
                    {
                        jcState = JungleCleanState.SearchingBestCamp;
                        Console.WriteLine("SearchingBestCamp");
                    }
                    if (!player.IsChanneling && !jungler.inSpwan())
                    {
                        jcState = JungleCleanState.SearchingBestCamp;
                        Console.WriteLine("SearchingBestCamp");
                    }

                }
            }
            else if (jcState != JungleCleanState.RecallForHeal && jcState != JungleCleanState.GoingToShop)
            {
                recalCasted = false;
            }

            if (jcState == JungleCleanState.GoingToShop && jungler.inSpwan())
            {
                if (jungler.nextItem != null && player.GoldCurrent >= jungler.nextItem.goldReach )
                    jungler.buyItems();
                if (player.Health > player.MaxHealth*0.75f && player.Mana > player.MaxMana*0.75f)
                {
                    jcState = JungleCleanState.SearchingBestCamp;
                    Console.WriteLine("SearchingBestCamp");
                }
            }
        }

        public static void cantRecall()
        {
            recalCasted = true;
        }

        public static bool canLeaveBase()
        {
            if (jungler.inSpwan() && player.Health > player.MaxHealth*0.7f &&
                (!jungler.gotMana || player.Mana > player.MaxMana*0.7f))
            {
                if (jungler.nextItem.goldReach - player.GoldCurrent > 16)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool noEnemiesAround()
        {
            return (MinionManager.GetMinions(player.Position, 500).Count == 0);
        }



        public static bool isCampVisible()
        {
            getJungleMinionsManualy();

            foreach (var min in focusedCamp.Minions)
            {
                if (min.unit != null && min.unit.IsVisible)
                {
                    return true;
                }
            }

            return false;
        }

        //will need to impliment all shortcuts here
        public static void logicRunToCamp()
        {
            jungler.doWhileRunningIdlin();

            if (jungler.canMove() && !canMoveLast)
            {
                jcState = JungleCleanState.SearchingBestCamp;
                canMoveLast = true;
                return;
            }

            if (!jungler.canMove())
            {
                canMoveLast = false;
               // jcState = JungleCleanState.SearchingBestCamp;
                return;
            }

            if ( !HypaJungle.player.IsMoving || HypaJungle.player.Path.Count() ==0 
                || HypaJungle.player.Path.Last().Distance(focusedCamp.campPosition) > 50)
            {
                HypaJungle.player.IssueOrder(GameObjectOrder.MoveTo, focusedCamp.campPosition);
            }
        }

        public static void attackCampMinions()
        {
            if (focusedCamp == null || focusedCamp.Minions == null)
                return;

            getJungleMinionsManualy();
            if (!jungler.gotOverTime || !HypaJungle.Config.Item("getOverTime").GetValue<bool>() || !focusedCamp.useOverTime)
            {
                    Camp.JungleMinion campMinions =
                   focusedCamp.Minions.Where(min => min != null && min.unit != null && min.unit is Obj_AI_Minion && !min.unit.IsDead && min.unit.IsVisible)
                       .OrderByDescending(min => (min.unit).MaxHealth).FirstOrDefault();
                    if (campMinions.unit is Obj_AI_Minion)
                        jungler.startAttack(campMinions, focusedCamp.canSmite());
                
               
            }
            else
            {
                Camp.JungleMinion campMinions =
                    focusedCamp.Minions.Where(min => min != null && min.unit != null && min.unit is Obj_AI_Minion && !min.unit.IsDead && min.unit.IsVisible)
                    .OrderBy(min => minHasOvertime(((Obj_AI_Minion)min.unit))).ThenByDescending(min => (min.unit).MaxHealth)
                        .FirstOrDefault();
                       // .OrderByDescending(min => ((Obj_AI_Minion)min.Unit).MaxHealth).First();
               
                if (campMinions.unit is Obj_AI_Minion)
                    jungler.startAttack(campMinions,focusedCamp.canSmite());

            }

        }

        public static int minHasOvertime(Obj_AI_Base min)
        {
            foreach (var buf in min.Buffs)
            {
                if (buf.Name == "itemmonsterburn")
                    return 5;
            }
            return 0;
        }

        public static void getJungleMinionsManualy()
        {
            List<Obj_AI_Base> jungles = MinionManager.GetMinions(HypaJungle.player.Position, 1000, MinionTypes.All,MinionTeam.Neutral).ToList();
            foreach (var jun in jungles)
            {
                HypaJungle.jTimer.setUpMinionsPlace((Obj_AI_Minion)jun);
            }
        }

        public static bool isCampFinished()
        {
            if (focusedCamp.State == Camp.JungleCampState.Dead && focusedCamp.aliveMinCount()==0)
                return true;
            return false;
            // return focusedCamp.Minions.All(min => min == null || min.Dead);
        }

        public static void doWhileIdling()
        {
            jungler.doWhileRunningIdlin();
        }

        public static void updateAllCamps()
        {
            foreach (var camp in HypaJungle.jTimer.jungleCamps)
            {
                camp.fullUpdate(jungler);
            }
        }

        /*
         *  is buff +5
         *  is in way of needed buff +5
         *  is close priority +10 +8 +6
         *  is spawning till get + 5 sec +4
         *  if smite ebtter get to buff then other camps
         * 
         */



        public static Camp getBestCampToGo()
        {
          /*  int minPriority = getPriorityNumber(HypaJungle.jTimer._jungleCamps.First());
            JungleCamp bestCamp = null;
            foreach (var jungleCamp in HypaJungle.jTimer._jungleCamps)
            {
                if(skipCamp != null && skipCamp.campId == jungleCamp.campId)
                    continue;
                int piro = getPriorityNumber(jungleCamp);
                if (minPriority > piro)
                {                   
                    bestCamp = jungleCamp;
                    minPriority = piro;
                }
            }
            skipCamp = null;*/

            updateAllCamps();
            return HypaJungle.jTimer.jungleCamps.OrderBy(camp => camp.priority).FirstOrDefault();
        }

        public static bool canDoDragon()
        {
            return player.Level >= jungler.dragOnLvl && player.Health/player.MaxHealth>0.70;
        }

        public static int getPriorityNumber(Camp camp)
        {
            if (camp.isDrag && !canDoDragon())
                return 999;

            if (!(canDoDragon() && camp.isDrag))
            {
                if (((camp.side != HypaJungle.player.Team)) && !HypaJungle.Config.Item("enemyJung").GetValue<bool>())
                    return 999;

                if (camp.side == GameObjectTeam.Neutral && !HypaJungle.Config.Item("doCrabs").GetValue<bool>())
                    return 999;
            }



            int priority = 0;

            var timeToCamp = camp.timeToCamp;
            var spawnTime = (Game.Time < camp.SpawnTime.TotalSeconds) ? camp.SpawnTime.TotalSeconds : camp.RespawnTimer.TotalSeconds;

            float revOn = camp.ClearTick + (float)spawnTime;
            float timeTillSpawn = (camp.State == Camp.JungleCampState.Dead)?((revOn - Game.Time > 0) ? (revOn - Game.Time) : 0):0;

            camp.willKillMe = false;
            if (camp.willKillMe && HypaJungle.Config.Item("checkKillability").GetValue<bool>())
            {
                priority += 777;
            }

            //Console.WriteLine("emm time?? " + camp.distToCamp/player.MoveSpeed);

            if (camp.isBuff)
                priority -= jungler.buffPriority;

           // priority -= camp.bonusPrio;
            priority += (int)(camp.distToCamp / player.MoveSpeed);
            priority += (int) timeTillSpawn;
            //Console.WriteLine(timeTillSpawn +" wadawdawd");
           // priority -= (camp.isBuff) ? jungler.buffPriority : 0;
            //priority -= (int)(timeTillSpawn - timeToCamp);
            //alive on come is better ;)
            //Priority focus!!
            if (player.Level <= 3)
            {
                priority -= camp.bonusPrioLowLvl;
            }
            else
            {

                priority -= camp.bonusPrio;
            }


            //if(!camp.isBuff)
              //  priority -= (isInBuffWay(camp)) ? 10 : 0;

            return priority;
        }

        public static bool isInBuffWay(Camp camp)
        {
            
            Camp bestBuff = getBestBuffCamp();
            if (bestBuff == null)
                return false;
            float distTobuff = bestBuff.campPosition.Distance(HypaJungle.player.Position,true);
            float distToCamp = camp.campPosition.Distance(HypaJungle.player.Position, true);
            float distCampToBuff = camp.campPosition.Distance(bestBuff.campPosition, true);
            if (distTobuff > distToCamp + 800 && distTobuff > distCampToBuff)
                return true;
            return false;

        }

        public static Camp getBestBuffCamp()
        {
            if (HypaJungle.jTimer.jungleCamps.Where(cp => cp.isBuff).Count() == 0)
                return null;

            Camp bestCamp = HypaJungle.jTimer.jungleCamps.Where(cp => cp.isBuff).OrderByDescending(cp => getPriorityNumber(cp)).First();
            return bestCamp;
        }


        public static float getPathLenght(Vector3[] vecs)
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

    }
}
