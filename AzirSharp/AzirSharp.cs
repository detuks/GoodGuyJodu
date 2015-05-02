using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
/*
 * ToDo:
 * 
 * 
 * check if in any my minions range <done
 * 
 * 
 * 
 * dont W far if Q not rdy
 * 
 * smart laneClear + good calc
 * 
 * smarter E
 * 
 * calc extended w aa range on other    
 * 
 * W+Q ks
 * 
 * 
 * */
using SharpDX;


namespace AzirSharp
{
    internal class AzirSharp
    {

        public const string CharName = "Azir";

        public static Menu Config;

        public static HpBarIndicator hpi = new HpBarIndicator();

        public AzirSharp()
        {
            Console.WriteLine("Azir started");
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("Azir - Sharp by DeTuKs");

            try
            {

                Config = new Menu("Azir - Sharp", "Azir", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                Azir.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                //TS
                var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(TargetSelectorMenu);
                Config.AddSubMenu(TargetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("useQ", "use Q")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useW", "use W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "use E")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));
               
                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));
               
                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                

                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "Debug Target")).SetValue(new KeyBind('T', KeyBindType.Press, false));


                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

              //  Game.OnGameSendPacket += OnGameSendPacket;
               // Game.OnGameProcessPacket += OnGameProcessPacket;

                Azir.setSkillShots();
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Yasuo- Sharpino");
            }

        }

        private static void OnGameProcessPacket(GamePacketEventArgs args)
        {
           
        }

        private static void OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 119)
                args.Process = false;
        }

        public static float startTime = 0;
        public static Vector3 startPos = new Vector3();
        public static float endTime = 0;
        public static Vector3 endPos = new Vector3();
        public static bool first = true;


        private static void OnGameUpdate(EventArgs args)
        {
            try
            {

               /* if (Azir.getUsableSoliders().Count != 0)
                {

                    Obj_AI_Minion fir = Azir.getUsableSoliders().First();
                    if (fir.IsMoving)
                    {
                        if (first)
                        {
                            startTime = Game.Time;
                            startPos = fir.ServerPosition;
                            first = false;
                        }

                    }
                    else
                    {
                        if (!first)
                        {
                            endTime = Game.Time;
                            endPos = fir.ServerPosition;
                            float dist = endPos.Distance(startPos);
                            Console.WriteLine(dist/(endTime-startTime));
                            Console.WriteLine(Azir.Player.BoundingRadius);
                            first = true;
                        }
                    }
                }*/

                if (Azir.orbwalker.ActiveMode.ToString() == "Combo")
                {
                    Obj_AI_Hero target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
                    Azir.doAttack();
                    if(target != null)
                        Azir.doCombo(target);
                }

                if (Azir.orbwalker.ActiveMode.ToString() == "Mixed")
                {
                    //Azir.doAttack();
                }

                if (Azir.orbwalker.ActiveMode.ToString() == "LaneClear")
                {
                    //Azir.doAttack();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static void onDraw(EventArgs args)
        {

        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name == "AzirSoldier" && sender.IsAlly)
            {
                Obj_AI_Minion myMin = sender as Obj_AI_Minion;
                if (myMin.SkinName == "AzirSoldier")
                    Azir.MySoldiers.Add(myMin);
            }

        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            int i = 0;
            foreach (var sol in Azir.MySoldiers)
            {
                if (sol.NetworkId == sender.NetworkId)
                {
                    Azir.MySoldiers.RemoveAt(i);
                    return;
                }
                i++;
            }
        }

        public static void OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {

           
        }




    }
}
