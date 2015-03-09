using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using HypaJungle.Camps;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    class JungleManager
    {
        public List<Camp> jungleCamps = new List<Camp>();

        public JungleManager()
        {
            Console.WriteLine("");
            OnGameLoad();
            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnGameLoad()
        {
            setUpCamps();
            GameObject.OnCreate += ObjectOnCreate;
            GameObject.OnDelete += ObjectOnDelete;
        }

        public void setUpCamps()
        {
            Console.WriteLine("Set up camps!!");
            jungleCamps.Add(new Gromp(13, new Vector3(2072.131f, 8450.272f, 51.92376f), GameObjectTeam.Order));
            jungleCamps.Add(new Senitels(1, new Vector3(3820.156f, 7920.175f, 52.21874f), GameObjectTeam.Order));
            jungleCamps.Add(new MurkWolfs(2, new Vector3(3842.77f, 6462.637f, 52.60973f), GameObjectTeam.Order));
            jungleCamps.Add(new Raptors(3, new Vector3(6926.0f, 5400.0f, 51.0f), GameObjectTeam.Order));
            jungleCamps.Add(new RedBrambleback(4, new Vector3(7772.412f, 4108.053f, 53.867f), GameObjectTeam.Order));
            jungleCamps.Add(new Krugs(5, new Vector3(8404.148f, 2726.269f, 51.2764f), GameObjectTeam.Order));

            jungleCamps.Add(new Dragon(6, new Vector3(9842.148f, 4430.269f, -71.2764f), GameObjectTeam.Neutral));

            jungleCamps.Add(new Gromp(14, new Vector3(12770.0f, 6468.0f, 51.84151f), GameObjectTeam.Chaos));
            jungleCamps.Add(new Senitels(7, new Vector3(10938.95f, 7000.918f, 51.8691f), GameObjectTeam.Chaos));
            jungleCamps.Add(new MurkWolfs(8, new Vector3(10972.0f, 8306.0f, 62.5235f), GameObjectTeam.Chaos));
            jungleCamps.Add(new Raptors(9, new Vector3(7970.319f, 9410.513f, 52.50048f), GameObjectTeam.Chaos));
            jungleCamps.Add(new RedBrambleback(10, new Vector3(7086.157f, 10866.92f, 56.63499f), GameObjectTeam.Chaos));
            jungleCamps.Add(new Krugs(11, new Vector3(6424.0f, 12156.0f, 56.62551f), GameObjectTeam.Chaos));

            //Drag baron soon
        }


        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                UpdateCamps();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void UpdateCamps()
        {
            foreach (Camp camp in jungleCamps)
            {
                bool allAlive = true;
                bool allDead = true;

                bool gotLightNoMinions = true;

                foreach (Camp.JungleMinion minion in camp.Minions)
                {
                    if(minion.unit == null)
                        continue;


                    if (minion.unit.IsDead)
                        allAlive = false;
                    else
                        allDead = false;
                }

                switch (camp.State)
                {
                    case Camp.JungleCampState.Alive:
                        if (allDead && camp.campPosition.Distance(HypaJungle.player.Position) < 600)
                        {
                            ///camp.State = Camp.JungleCampState.Dead;
                           // camp.ClearTick = Game.Time;
                            foreach (var min in camp.Minions)
                            {
                          //      min.unit = null;
                            }

                        }
                        break;
                }
            }
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
                    Camp.JungleMinion minion in
                        from camp in jungleCamps
                        from minion in camp.Minions
                        where minion.Name == neutral.Name
                        select minion)
                {
                    minion.unit = null;
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
                   Camp.JungleMinion minion in
                       from camp in jungleCamps
                       from minion in camp.Minions
                       select minion)
            {
                //Console.WriteLine(minion.Name+ " : "+neutral.Name);
                if(minion.Name == neutral.Name)
                     minion.unit = neutral;
            }
        }

        public void enableCamp(byte id)
        {
            foreach (var camp in jungleCamps)
            {
                if (camp.campId == id)
                {
                    camp.ClearTick = 0;
                    camp.State = Camp.JungleCampState.Alive;
                    camp.onRespawn();
                }
            }
        }

        public void disableCamp(byte id)
        {
            foreach (var camp in jungleCamps)
            {
                if (camp.campId == id)
                {
                    camp.ClearTick = Game.Time;
                    camp.State = Camp.JungleCampState.Dead;
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
                    Camp.JungleMinion minion in
                        from camp in jungleCamps
                        from minion in camp.Minions
                        where minion.Name == neutral.Name
                        select minion)
                {
                    minion.unit = neutral;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
