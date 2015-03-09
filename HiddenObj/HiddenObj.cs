using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using System.Net;
namespace HiddenObj
{
    internal class HiddenObj
    {



        public static List<ListedHO> allObjects = new List<ListedHO>();

        public HiddenObj()
        {
            CustomEvents.Game.OnGameLoad += onLoad;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Drawing.OnDraw += onDraw;
        }

        public static void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
           // if (obj.Name.Contains("Turret") || obj.Name.Contains("Minion"))
           //     return;
           // Console.WriteLine(obj.BasicAttack.Name+" -:- "+obj.SkinName);
        }



        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("missile") || sender.Name.Contains("Minion"))
                return;

            Obj_AI_Base objis = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(sender.NetworkId);
            //Console.WriteLine(sender.Name+" - "+objis.SkinName);
            //Console.WriteLine(sender.Name + " - " + sender.Type + " - " + sender.Flags);
            HidObject ho = HidObjects.IsHidObj(objis.SkinName);
            if (ho != null)
            {
                allObjects.Add(new ListedHO(sender.NetworkId,sender.Name,ho.Duration,ho.ObjColor,ho.Range,sender.Position,Game.Time));
            }
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            int i=0;
            foreach (var lho in allObjects)
            {
                if (lho.NetworkId == sender.NetworkId)
                {
                    allObjects.RemoveAt(i);
                    return;
                }
                i++;
            }
        }

        private static void onLoad(EventArgs args)
        {
            Game.PrintChat("Hidden Objects 0.1 by DeTuKs");
		}

        private static void onDraw(EventArgs args)
        {
            //Utility.DrawCircle(ObjectManager.Player.Position, 500, System.Drawing.Color.FromArgb(255, 186, 201, 46));
            //Drawing.DrawText(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Z, Color.FromArgb(255, 0, 0, 0), "awdawawd");
            foreach (var lho in allObjects)
            {
                if (lho.Duration == -1 || (int)((lho.CreatedAt + lho.Duration + 1) - Game.Time) > 0)
                {
                    Utility.DrawCircle(lho.Position, 50, lho.ObjColor);
                    if (lho.Duration > 0)
                    {
                        Vector2 locOnScreen = Drawing.WorldToScreen(lho.Position);
                        Drawing.DrawText(locOnScreen.X - 10, locOnScreen.Y - 10, lho.ObjColor, "" + (int)((lho.CreatedAt + lho.Duration + 1) - Game.Time));
                    }
                }
            }
        }
    }
}
