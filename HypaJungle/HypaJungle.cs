using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Net;

namespace HypaJungle
{
    /*
     * Jungle
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     */


    internal class HypaJungle
    {
        public static JungleTimers jTimer;

        public static Menu Config;

        public static Obj_AI_Hero player = ObjectManager.Player;

        public static float lastSkip = 0;

        public HypaJungle()
        {
          

            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("HypaJungle by DeTuKs");
            try
            {
			
			

                ConfigLoader.setupFolders(JungleClearer.supportedChamps);

                if (!JungleClearer.supportedChamps.Contains(player.ChampionName))
                {
                    Game.PrintChat("Sory this champion is not supported yet! go vote for it in forum ;)");
                    return;
                }

                jTimer = new JungleTimers();

                Config = new Menu("HypeJungle", "hype", true);

                setChampMenu(player.ChampionName);

              //  Config.AddSubMenu(new Menu("Jungler Config", "junglerCon"));
               // Config.SubMenu("junglerCon").AddItem(new MenuItem("blabla", "Relead to work!")).SetValue(true);
              //  Config.SubMenu("junglerCon").AddItem(new MenuItem("useDefConf", "Use Default Config")).SetValue(true);
              //  Config.SubMenu("junglerCon").AddItem(new MenuItem("fileConfigHypa", "")).SetValue(ConfigLoader.getChampionConfigs(player.ChampionName));
                Config.AddSubMenu(new Menu("Jungler", "jungler"));
                Config.SubMenu("jungler").AddItem(new MenuItem("doJungle", "Do jungle")).SetValue(new KeyBind('J', KeyBindType.Toggle));
                Config.SubMenu("jungler").AddItem(new MenuItem("skipSpawn", "Debug skip")).SetValue(new KeyBind('G', KeyBindType.Press));
                Config.SubMenu("jungler").AddItem(new MenuItem("autoLVL", "Auto Level")).SetValue(true);
                Config.SubMenu("jungler").AddItem(new MenuItem("autoBuy", "Auto Buy")).SetValue(true);

                Config.AddSubMenu(new Menu("Jungle CLeaning", "jungleCleaning"));
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("smiteToKill", "smite to kill")).SetValue(false);
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("enemyJung", "do Enemy jungle")).SetValue(false);
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("doCrabs", "do Crabs")).SetValue(false);
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("getOverTime", "Get everyone OverTimeDmg")).SetValue(false);
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("checkKillability", "Check if can kill camps")).SetValue(false);

                Config.AddSubMenu(new Menu("Drawings", "draw"));
                Config.SubMenu("draw").AddItem(new MenuItem("drawStuff", "Draw??")).SetValue(false);
             

                Config.AddSubMenu(new Menu("Debug stuff", "debug"));
               Config.SubMenu("debug").AddItem(new MenuItem("debugOn", "Debug stuff")).SetValue(new KeyBind('A', KeyBindType.Press));
                Config.SubMenu("debug").AddItem(new MenuItem("showPrio", "Show priorities")).SetValue(false);

                Config.AddToMainMenu();

               

                Game.OnGameUpdate += OnGameUpdate;
                Drawing.OnDraw += onDraw;
                CustomEvents.Unit.OnLevelUp += OnLevelUp;

                Game.OnGameProcessPacket += Game_OnGameProcessPacket;
                JungleClearer.setUpJCleaner();

                //Load custom stuff
                if (!Config.Item("useDefConf_"+player.ChampionName).GetValue<bool>())
                    ConfigLoader.loadNewConfigHypa(
                       Config.Item("fileConfigHypa2_" + player.ChampionName).GetValue<StringList>().SList[
                           Config.Item("fileConfigHypa2_"+player.ChampionName).GetValue<StringList>().SelectedIndex]);

                JungleClearer.jungler.setFirstLvl();


            }
            catch(Exception ex)
            {
                Game.PrintChat("Oops. Something went wrong with HypaJungle");
                Console.WriteLine(ex);
            }

        }

        public static void setChampMenu(string champ)
        {

            Config.AddSubMenu(new Menu(champ+" Config", "junglerCon" + champ));
            Config.SubMenu("junglerCon" + champ).AddItem(new MenuItem("blabla_" + champ, "Relead to work!")).SetValue(true);
            Config.SubMenu("junglerCon" + champ).AddItem(new MenuItem("useDefConf_" + champ, "Use Default Config")).SetValue(true);
            Config.SubMenu("junglerCon" + champ).AddItem(new MenuItem("fileConfigHypa2_" + champ, "")).SetValue(ConfigLoader.getChampionConfigs(player.ChampionName));

        }

        static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.EmptyJungleCamp.Header)
            {
                Packet.S2C.EmptyJungleCamp.Struct camp = Packet.S2C.EmptyJungleCamp.Decoded(args.PacketData);
                Console.WriteLine("disable camp: "+camp.CampId);
                jTimer.disableCamp((byte)camp.CampId);
            }

            if (args.PacketData[0] == 0xE9)
            {
                GamePacket gp = new GamePacket(args.PacketData);
                gp.Position = 21;
                byte campID = gp.ReadByte();
                Console.WriteLine("Enable camp: "+campID);
                jTimer.enableCamp(campID);

            }

            //AfterAttack
            if (args.PacketData[0] == 0x65 && Config.Item("doJungle").GetValue<KeyBind>().Active)
            {
                GamePacket gp = new GamePacket(args.PacketData);
                gp.Position = 1;
                Packet.S2C.Damage.Struct dmg = Packet.S2C.Damage.Decoded(args.PacketData);

                int targetID = gp.ReadInteger();
                int dType = (int)gp.ReadByte();
                int Unknown = gp.ReadShort();
                float DamageAmount = gp.ReadFloat();
                int TargetNetworkIdCopy = gp.ReadInteger();
                int SourceNetworkId = gp.ReadInteger();
                float dmga = (float)player.GetAutoAttackDamage(ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(targetID));
                if (dmga - 10 > DamageAmount || dmga + 10 < DamageAmount)
                    return;
                if (player.NetworkId != dmg.SourceNetworkId || player.NetworkId == targetID || player.NetworkId == TargetNetworkIdCopy)
                    return;
                Obj_AI_Base targ = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(dmg.TargetNetworkId);
                if ((int) dmg.Type == 12 || (int) dmg.Type == 4 || (int) dmg.Type == 3)
                {
                    Console.WriteLine("dmg: " + DamageAmount + " : " + dmga);

                    JungleClearer.jungler.doAfterAttack(targ);
                }

            }
        }

        private static void OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (Config.Item("autoLVL").GetValue<bool>())
                JungleClearer.jungler.levelUp(sender,args);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("skipSpawn").GetValue<KeyBind>().Active) //fullDMG
            {
                if (JungleClearer.focusedCamp != null && lastSkip+1<Game.Time)
                {
                    lastSkip = Game.Time;
                    JungleClearer.skipCamp = JungleClearer.focusedCamp;
                    jTimer.disableCamp(JungleClearer.focusedCamp.campId);
                }
            }

            if (Config.Item("debugOn").GetValue<KeyBind>().Active) //fullDMG
            {
               /* foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(player))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(player);
                    if (name.Contains("cent"))
                        Console.WriteLine("{0}={1}", name, value);
                }*/

                foreach (var item in player.InventoryItems)
                {
                    Console.WriteLine(item.Id+" : "+item.Name);
                }

                Console.WriteLine(player.Mana);

                foreach (var buf in player.Buffs)
                {

                    Console.WriteLine(buf.Name);
                }

               /* foreach (SpellDataInst spell in player.Spellbook.Spells)
                {
                    Console.WriteLine(spell.Name.ToLower());
                }

                foreach (SpellDataInst spell in player.Spellbook.Spells)
                {
                    Console.WriteLine(spell.Name.ToLower()+"  "+spell.Slot);
                }*/
               

            }
            if (Config.Item("doJungle").GetValue<KeyBind>().Active) //fullDMG
            {
                try
                {
                    JungleClearer.updateJungleCleaner();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                JungleClearer.jcState = JungleClearer.JungleCleanState.GoingToShop;
            }

        }

        private static void onDraw(EventArgs args)
        {
            if (!Config.Item("drawStuff").GetValue<bool>())
                return;

            Drawing.DrawText(200, 200, Color.Green, JungleClearer.jcState.ToString() + ": " + JungleClearer.jungler.dpsFix + " : " + player.Position.X + " : " + player.Position.Y + " : "
                + player.Position.Z + " : ");
            Drawing.DrawText(200, 220, Color.Green, "DoOver: " + JungleClearer.jungler.overTimeName+" : "+JungleClearer.jungler.gotOverTime);

            if (JungleClearer.jungler.nextItem != null)
                Drawing.DrawText(200, 250, Color.Green, "Gold: "+JungleClearer.jungler.nextItem.goldReach);
            if (JungleClearer.focusedCamp != null)
             Drawing.DrawCircle(JungleClearer.focusedCamp.Position,300,Color.BlueViolet);

            foreach (var min in MinionManager.GetMinions(HypaJungle.player.Position, 800, MinionTypes.All,MinionTeam.Neutral))
            {
                if (JungleClearer.jungler.overTimeName != "" && JungleClearer.minHasOvertime(min)!=0)
                    Drawing.DrawCircle(min.Position,100,Color.Brown);

                var pScreen = Drawing.WorldToScreen(min.Position);
                Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red, min.Name+" : "+min.MaxHealth);
                var bufCount = 10;
                foreach (var buff in min.Buffs)
                {
                    Drawing.DrawText(pScreen.X, pScreen.Y + bufCount, Color.Red, buff.Name);
                    bufCount += 10;
                }
            }



            Drawing.DrawCircle(JungleClearer.getBestBuffCamp().Position, 500, Color.BlueViolet);

           /* foreach (var camp in jTimer._jungleCamps)
            {
                var pScreen = Drawing.WorldToScreen(camp.Position);

                if(JungleClearer.isInBuffWay(camp))
                    Drawing.DrawCircle(camp.Position, 200, Color.Red);
                   // Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red, camp.State.ToString() + " : " + JungleClearer.getPriorityNumber(camp));

                //Order = 0 chaos =1
            }*/

            if (Config.Item("showPrio").GetValue<bool>()) //fullDMG
            {
                foreach (var camp in jTimer._jungleCamps)
                {
                    var pScreen = Drawing.WorldToScreen(camp.Position);

                    Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red,
                        camp.State.ToString() + " : "+camp.team+ " : " + camp.priority);

                    //Order = 0 chaos =1
                }
            }
        }

    }
}
