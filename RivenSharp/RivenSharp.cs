using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

/*TODO
 * Combo calc and choose best
 * Farming
 * Interupt
 * 
 * gap close with q
 * 
 * mash q if les hp
 * 
 * smart cancel combos
 * 
 * 
 */

namespace RivenSharp
{
    class RivenSharp
    {

        public const string CharName = "Riven";

        public static Menu Config;

        

        public RivenSharp()
        {
            if (ObjectManager.Player.BaseSkinName != CharName)
                return;
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;
          
        }

        private static void onLoad(EventArgs args)
        {
            Game.PrintChat("RivenSharp by DeTuKs Donate if you love my assams :)");
            Config = new Menu("Riven - Sharp", "Riven", true);
            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
           Riven.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
           //TS
           var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
           SimpleTs.AddToMenu(TargetSelectorMenu);
           Config.AddSubMenu(TargetSelectorMenu);
            //Combo
            Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);

            //Debug
            Config.AddSubMenu(new Menu("Debug", "debug"));
            Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "Debug Target")).SetValue(new KeyBind('T', KeyBindType.Press, false));
			
			//Donate
			Config.AddSubMenu(new Menu("Donate", "Donate"));
			Config.SubMenu("debug").AddItem(new MenuItem("domateMe", "PayPal:")).SetValue(true);
			Config.SubMenu("debug").AddItem(new MenuItem("domateMe2", "dtk600@gmail.com")).SetValue(true);
			Config.SubMenu("debug").AddItem(new MenuItem("domateMe3", "Tnx ^.^")).SetValue(true);

            Config.AddToMainMenu();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += OnGameUpdate;

            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            GameObject.OnPropertyChange += OnPropertyChange;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Obj_AI_Base.OnPlayAnimation += OnPlayAnimation;
            Orbwalking.AfterAttack += Riven.AfterAttack;

            Game.OnGameSendPacket += OnGameSendPacket;
            Game.OnGameProcessPacket += OnGameProcessPacket;
        }

        /*
         * 
         */
        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("db_targ").GetValue<KeyBind>().Active)
            {

            }

            if (Riven.orbwalker.ActiveMode.ToString() == "Combo")
            {
                 Obj_AI_Hero target = SimpleTs.GetTarget(500, SimpleTs.DamageType.Physical);
                 Riven.doCombo(target);
                 //Console.WriteLine(target.NetworkId);
            }
        }


        private static void onDraw(EventArgs args)
        {
            foreach (Obj_AI_Hero enHero in ObjectManager.Get<Obj_AI_Hero>().Where(enHero => enHero.IsEnemy && enHero.Health > 0))
            {
                Utility.DrawCircle(enHero.Position, enHero.BoundingRadius + Riven.E.Range + Riven.Player.AttackRange, Color.Blue);
                //Drawing.DrawCircle(enHero.Position, enHero.BoundingRadius + Riven.E.Range+Riven.Player.AttackRange, Color.Blue);
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
           // if (sender.Name.Contains("missile") || sender.Name.Contains("Minion"))
           //     return;
           // Console.WriteLine("Object: " + sender.Name);
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {

        }


        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base sender, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
            // if (sender.Name.Contains("missile") || sender.Name.Contains("Minion"))
            //    return;
             //Console.WriteLine("Spell: "+sender.Name + " - " + arg.SData.SpellCastTime);
             if (arg.SData.Name.Contains("RivenBasic"))
             {
                // Riven.timer = new System.Threading.Timer(obj => { Riven.Player.IssueOrder(GameObjectOrder.MoveTo, Riven.difPos()); }, null, (long)100, System.Threading.Timeout.Infinite);
             }
             //Console.WriteLine(sender.Name + " - " + objis.SkinName);
        }

        public static void OnPropertyChange(LeagueSharp.GameObject obj, LeagueSharp.GameObjectPropertyChangeEventArgs prop)
        {
           // Console.WriteLine("obj: " + obj.Name + " - " + prop.NewValue);
        }

        public static void OnPlayAnimation(LeagueSharp.GameObject value0, GameObjectPlayAnimationEventArgs value1)
        {
           // if (value1.Animation.Contains("Spell"))
           // {
           //     Console.WriteLine("Hydra");
           //     Utility.DelayAction.Add(Game.Ping + 150, delegate { Riven.useHydra(Riven.orbwalker.GetTarget()); });
           // }
        }


        public static void OnGameProcessPacket(GamePacketEventArgs args)
        {
            try
            {
                if (Config.Item("db_targ").GetValue<KeyBind>().Active)
                {
                    LogPacket(args);
                }
                if (Riven.orbwalker.ActiveMode.ToString() == "Combo")
                {
                    if (args.PacketData[0] == 101 && Riven.Q.IsReady())
                    {
                       // LogPacket(args);
                        GamePacket gp = new GamePacket(args.PacketData);
                        gp.Position = 5;
                        int dType = (int)gp.ReadByte();
                        int targetID = gp.ReadInteger();
                        int source = gp.ReadInteger();
                        if (Riven.Player.NetworkId != source)
                            return;
                        Obj_AI_Hero targ = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(targetID);
                        if(dType == 12 || dType == 3)
                            Riven.Q.Cast(targ.Position);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void OnGameSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (args.PacketData[0] == 119)
                    args.Process = false;

                //if (Riven.orbwalker.ActiveMode.ToString() == "Combo")
                 //   LogPacket(args);
                if (args.PacketData[0]==154 && Riven.orbwalker.ActiveMode.ToString() == "Combo")
                {
                    Packet.C2S.Cast.Struct cast = Packet.C2S.Cast.Decoded(args.PacketData);
                    if ((int)cast.Slot > -1 && (int)cast.Slot < 5 )
                        Utility.DelayAction.Add(Game.Ping, delegate { Riven.cancelAnim(); });

                    if (cast.Slot == SpellSlot.E && Riven.R.IsReady())
                    {
                        Console.WriteLine("cast QQQQ");
                        Utility.DelayAction.Add(Game.Ping + 100, delegate { Riven.useRSmart(Riven.orbwalker.GetTarget()); });
                    }
                    //Console.WriteLine(cast.Slot + " : " + Game.Ping);
                   /* if (cast.Slot == SpellSlot.Q)
                        Orbwalking.ResetAutoAttackTimer();
                    else if (cast.Slot == SpellSlot.W && Riven.Q.IsReady())
                        Utility.DelayAction.Add(Game.Ping+200, delegate { Riven.useHydra(Riven.orbwalker.GetTarget()); });
                    else if (cast.Slot == SpellSlot.E && Riven.W.IsReady())
                    {
                        Console.WriteLine("cast QQQQ");
                        Utility.DelayAction.Add(Game.Ping+200, delegate { Riven.useWSmart(Riven.orbwalker.GetTarget()); });
                    }
                    else if ((int)cast.Slot == 131 && Riven.W.IsReady())
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        Utility.DelayAction.Add(Game.Ping +200, delegate { Riven.useWSmart(Riven.orbwalker.GetTarget()); });
                    }*/
                        // LogPacket(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void LogPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != 114 && args.PacketData[0] != 119 && args.PacketData[0] != 168 ) //97 = move
            {
                 GamePacket gp = new GamePacket(args.PacketData);
          //      gp.Position = 5;
          //      int dType = (int)gp.ReadByte();
          //      int target = gp.ReadInteger();
          //      int source = gp.ReadInteger();
          //      Console.WriteLine("DamageT: " + dType);
          //      Console.WriteLine("targetId: " + target);
           //     Console.WriteLine("SourceId: " + source + " - " + Riven.Player.NetworkId);
                Console.WriteLine("Head: " + args.PacketData[0]);
                Console.WriteLine("Channel: {0}{3}Flag: {1}{3}Data: {2}{3}{3}", args.Channel, args.ProtocolFlag,
                    args.PacketData.Aggregate(string.Empty,
                        (current, d) => current + (d.ToString(CultureInfo.InvariantCulture) + " ")), Environment.NewLine);
            }
        }


      


    }
}
