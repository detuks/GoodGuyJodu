using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;

namespace LeeSinSharp
{
    class LeeSin
    {
        public static Vector2 testSpellCast;
        public static Vector2 testSpellProj;

        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static Spellbook sBook = Player.Spellbook;

        public static Orbwalking.Orbwalker orbwalker;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 1100);
        public static Spell W = new Spell(SpellSlot.W, 700);
        public static Spell E = new Spell(SpellSlot.E, 350);
        public static Spell R = new Spell(SpellSlot.R, 375);

        public static Obj_AI_Hero LockedTarget;

        public static Vector2 harassStart;

        public static void checkLock(Obj_AI_Hero target)
        {
            //if (!target.IsValidTarget())
            //    return;
            if (LeeSin.orbwalker.ActiveMode.ToString() == "None" && !LeeSinSharp.Config.Item("harassDo").GetValue<KeyBind>().Active && LockedTarget != null)//Reset all values
            {
                LockedTarget = null;
            }
            else if (orbwalker.ActiveMode.ToString() == "Combo")
                LockedTarget = target;
            else if (target.IsValidTarget() && LockedTarget == null && (LeeSin.orbwalker.ActiveMode.ToString() != "None" || LeeSinSharp.Config.Item("harassDo").GetValue<KeyBind>().Active))
            {
                Console.WriteLine("SetLock");
                LockedTarget = target;
            }
        }


        public static void setSkillShots()
        {
            Q.SetSkillshot(0.4f, 60f, 1800f, true, Prediction.SkillshotType.SkillshotLine);
            E.SetSkillshot(0.4f, 350f, 0f, false, Prediction.SkillshotType.SkillshotCircle);
        }


        public static void doHarass()
        {
            //Console.WriteLine("Harass");
            moveTo(Game.CursorPos.To2D());
            if (LockedTarget == null)
                return;

            if (!castQFirstSmart())
                if(!castQSecondSmart())
                    if (!castEFirst())
                        getBackHarass();

            /*if (Q.IsReady() && Qdata.Name == "BlindMonkQOne")
                castQFirstSmart();
            else if (Q.IsReady() && Qdata.Name == "blindmonkqtwo" && W.IsReady())
            {
                harassStart = Player.Position.To2D();
                castQSecondSmart();
            }
            else if (E.IsReady())
            {
                //Console.WriteLine("Cast wcast E " + Edata.Name);
                castEFirst();
            }
            else if (Qdata.Name != "BlindMonkQOne")
                getBackHarass();*/

        }



        public static bool getBackHarass()
        {
            Console.WriteLine("Jump away");
            Obj_AI_Turret closest_tower = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            Obj_AI_Base jumpOn = ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly && !(ally is Obj_AI_Turret) && !ally.IsMe && ally.Distance(LeeSin.Player.ServerPosition) < 700).OrderBy(tur => tur.Distance(closest_tower.ServerPosition)).First();
            W.Cast(jumpOn);
           // wardJump(closest_tower.Position.To2D());
            return false;
        }

        public static bool targetHasQ(Obj_AI_Hero target)
        {
            foreach (BuffInstance buf in target.Buffs)
            {
                if (buf.Name == "BlindMonkQOne")
                    return true;
                //Console.WriteLine(buf.Name);
            }
            return false;
            /*if(target.HasBuff("blindmonkpassive_cosmetic") 
                || (target.HasBuff("BlindMonkQOne") && (target.Buffs.ToList().Find(buf => buf.Name == "BlindMonkQOne").EndTime-Game.Time)>=0.3))
                return true;
            return false;*/
        }

        public static bool castQFirstSmart()
        {
            if (!Q.IsReady() || Qdata.Name != "BlindMonkQOne" || LockedTarget == null)
                return false;

            
            Prediction.PredictionOutput predict = Q.GetPrediction(LockedTarget);
            if (predict.HitChance > Prediction.HitChance.LowHitchance)
            {
                Q.Cast(predict.CastPosition);
                return true;
            }
            return true;
        }

        public static bool castQSecondSmart()
        {
            if (!Q.IsReady() || Qdata.Name != "blindmonkqtwo" || LockedTarget == null)
                return false;
            if (targetHasQ(LockedTarget) && inDistance(LockedTarget.Position.To2D(), Player.ServerPosition.To2D(), 1200))
            {
                Q.Cast();
                return true;
            }
            return true;
        }



        public static bool castEFirst()
        {
            if (!E.IsReady() || LockedTarget == null || Edata.Name != "BlindMonkEOne")
                return false;

            Console.WriteLine("Cast wcast E " + Edata.Name);
            if (inDistance(LockedTarget.Position.To2D(), Player.ServerPosition.To2D(), E.Range+20))
            {
                E.Cast();
                return true;
            }
            return true;    
        }

        public static int getJumpWardId()
        {
            int[] wardIds = { 3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043 };
            foreach (int id in wardIds)
            {
                if (Items.HasItem(id) && Items.CanUseItem(id))
                    return id;
            }
            return -1;
        }

        public static void moveTo(Vector2 Pos)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Pos.To3D());
        }

        public static void wardJump(Vector2 pos)
        {
            Vector2 posStart = pos;
            if (!W.IsReady())
                return;
            bool wardIs = false;
            Console.WriteLine("ward jumps");
            if (!inDistance(pos, Player.ServerPosition.To2D(), W.Range+15))
            {
                Console.WriteLine("ward jumpsaway");    
                pos = Player.ServerPosition.To2D() + Vector2.Normalize(pos - Player.ServerPosition.To2D())*600;
            }

            if(!W.IsReady() && W.ChargedSpellName == "")
                return;
            foreach (Obj_AI_Base ally in ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly
                && !(ally is Obj_AI_Turret) && inDistance(posStart, ally.ServerPosition.To2D(), 200)))
            {
                //if (ally is Obj_AI_Minion && ally.IsImmovable)
                    wardIs = true;
                moveTo(pos);
                if (inDistance(Player.ServerPosition.To2D(), ally.ServerPosition.To2D(), W.Range + ally.BoundingRadius))
                    W.Cast(ally);
                return;
            }
            Polygon pol;
            if ((pol = LeeSinSharp.map.getInWhichPolygon(pos)) != null)
            {
                if (inDistance(pol.getProjOnPolygon(pos), Player.ServerPosition.To2D(), W.Range+15) && !wardIs && inDistance(pol.getProjOnPolygon(pos), pos, 200))
                    putWard(pos);
            }
            else if(!wardIs)
            {
                putWard(pos);
            }

        }

        public static bool putWard(Vector2 pos)
        {
            int wardItem;
            if ((wardItem = getJumpWardId()) != -1)
            {
                foreach (var slot in Player.InventoryItems.Where(slot => slot.Id == (ItemId)wardItem))
                {
                    slot.UseItem(pos.To3D());
                    return true;
                }
            }
            return false;
        }


        public static bool inDistance(Vector2 pos1, Vector2 pos2, float distance)
        {
            float dist2 = Vector2.DistanceSquared(pos1, pos2);
            return (dist2 <= distance * distance) ? true : false;
        }
    }
}
