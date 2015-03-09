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
 * */


namespace LeeSinSharp
{
    internal class LeeSinSharp
    {
        public static string[] testSpells = { "RelicSmallLantern", "RelicLantern", "SightWard", "wrigglelantern", "ItemGhostWard", "VisionWard",
                                     "BantamTrap", "JackInTheBox","CaitlynYordleTrap", "Bushwhack"};


        

        public const string CharName = "LeeSin";

        public static Menu Config;

        public static Map map;

        public static Obj_AI_Hero target;

        public LeeSinSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {
            map = new Map();

            Game.PrintChat("LeeSin - Sharp by DeTuKs");

            try
            {

                Config = new Menu("LeeSin - SharpSwrod", "LeeSin", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                LeeSin.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                //TS
                var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
                SimpleTs.AddToMenu(TargetSelectorMenu);
                Config.AddSubMenu(TargetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));
               
                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));
               
                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));
                Config.SubMenu("harass").AddItem(new MenuItem("harassDo", "Harass adv")).SetValue(new KeyBind('H', KeyBindType.Press, false));

                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                

                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "Debug Target")).SetValue(new KeyBind('T', KeyBindType.Press, false));


                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

                LeeSin.setSkillShots();
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Yasuo- Sharpino");
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {

            target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
            LeeSin.checkLock(target);
            if (Config.Item("db_targ").GetValue<KeyBind>().Active)
            {
                LeeSin.wardJump(Game.CursorPos.To2D());
            }

            if (Config.Item("harassDo").GetValue<KeyBind>().Active)
            {
                LeeSin.doHarass();
            }


            if (LeeSin.orbwalker.ActiveMode.ToString() == "Combo")
            {
                Console.WriteLine(LeeSin.Q.ChargedSpellName);
                Console.WriteLine(LeeSin.Qdata.Name);
            }

            if (LeeSin.orbwalker.ActiveMode.ToString() == "Mixed")
            {
               
            }

            if (LeeSin.orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                
            }


            if (Config.Item("harassOn").GetValue<bool>() && LeeSin.orbwalker.ActiveMode.ToString() == "None")
            {
              
            }
        }

        private static void onDraw(EventArgs args)
        {
            Drawing.DrawCircle(LeeSin.Player.Position, 730, Color.Blue);
          /* if(map.isWall(Game.CursorPos.To2D()))
                Drawing.DrawCircle(Game.CursorPos, 200, Color.Red);
            else
                Drawing.DrawCircle(Game.CursorPos, 200, Color.Beige);
            Polygon pol;
            if ((pol = map.getInWhichPolygon(Game.CursorPos.To2D())) != null)
            {
                /*SharpDX.Vector2 start = pol.Points[pol.Count() - 1];
                foreach (SharpDX.Vector2 vecPol in pol.Points)
                {
                    SharpDX.Vector2 closest = pol.ClosestVec(start, vecPol, Game.CursorPos.To2D());
                    Drawing.DrawCircle(closest.To3D(), 100, Color.Green);
                    start = vecPol;
                }
                Drawing.DrawCircle(LeeSin.Player.Position, 400, Color.Green);
                SharpDX.Vector2 proj = pol.getProjOnPolygon(Game.CursorPos.To2D());
                Drawing.DrawCircle(proj.To3D(), 100, Color.Green);
            }*/
            //ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly).OrderBy(tur => tur.Distance(LeeSin.Player.ServerPosition)).First().Position;

            Obj_AI_Turret closest_tower = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly).OrderBy(tur => tur.Distance(LeeSin.Player.ServerPosition)).First();
            Obj_AI_Base jumpOn = ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly && !(ally is Obj_AI_Turret) && !ally.IsMe && ally.Distance(LeeSin.Player.ServerPosition)<700).OrderBy(tur => tur.Distance(closest_tower.ServerPosition)).First();

            Drawing.DrawCircle(closest_tower.Position, 300, Color.Red);
            Drawing.DrawCircle(jumpOn.Position, 200, Color.Red);



            if (LeeSin.LockedTarget != null)
                Drawing.DrawCircle(LeeSin.LockedTarget.Position, 200, Color.Red);

            foreach (Polygon pol in map.poligs)
            {
                pol.Draw(Color.BlueViolet, 3);
            }

            if(LeeSin.testSpellCast != null)
                Drawing.DrawCircle(LeeSin.testSpellCast.To3D(), 100, Color.Blue);

            if (LeeSin.testSpellProj != null)
                Drawing.DrawCircle(LeeSin.testSpellProj.To3D(), 100, Color.Red);


            //LeeSin.testSpellProj
           // foreach (Polygon pol in map.poligs)
            //{
               // pol.Draw(Color.Red, 2);
               
            //}
            
            
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Missile") || sender.Name.Contains("Minion"))
                return;
           // if(sender is Obj_AI_Minion)
          //    Obj_AI_Minion ward = (Obj_AI_Minion)sender;
                //  Console.WriteLine(ward.Type+"  - "+ward.);

        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
          
        }

        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
            if (testSpells.ToList().Contains(arg.SData.Name))
            {
                Console.WriteLine("New ward found!");
                LeeSin.testSpellCast = arg.End.To2D();
                Polygon pol;
                if ((pol = map.getInWhichPolygon(arg.End.To2D())) != null)
                {
                    LeeSin.testSpellProj = pol.getProjOnPolygon(arg.End.To2D());
                }
            }
        }




    }
}
