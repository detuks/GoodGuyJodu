using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;

namespace RivenSharp
{
    class Riven
    {
        public static System.Threading.Timer timer;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static Spellbook sBook = Player.Spellbook;

        public static Orbwalking.Orbwalker orbwalker;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 280);
        public static Spell Q2 = new Spell(SpellSlot.Q, 280);
        public static Spell Q3 = new Spell(SpellSlot.Q, 280);
        public static Spell W = new Spell(SpellSlot.W, 260);
        public static Spell E = new Spell(SpellSlot.E, 390);
        public static Spell R = new Spell(SpellSlot.R, 900);

        public static void setSkillshots()
        {
            R.SetSkillshot(0.25f, 300f, 120f, false, SkillshotType.SkillshotCone);
        }


        public static void doCombo(Obj_AI_Base target)
        {
            useESmart(target);
            useWSmart(target,true);
            useRSmart((Obj_AI_Hero)target);
            useHydra(target);
        }


        public static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {

            if (orbwalker.ActiveMode.ToString() == "Combo")
            { 
               // Console.WriteLine("afer attack!  "+unit.Name+" targ:"+target.Name);
               // long aaTimer = (long)0.36;
              //  timer = new System.Threading.Timer(obj => { Q.Cast(target.ServerPosition); }, null, aaTimer, System.Threading.Timeout.Infinite);
            }
        }

        public static  void useWSmart(Obj_AI_Base target, bool aaRange =false)
        {
            float range = 0;
            if (aaRange)
                range = Player.AttackRange + target.BoundingRadius;
            else
                range = W.Range + target.BoundingRadius - 20;
            if (W.IsReady() && target.Distance(Player.ServerPosition) <range)
            {
                W.Cast();
            }

        }

        public static void useESmart(Obj_AI_Base target)
        {
            if (!E.IsReady())
                return;
            float trueAARange = Player.AttackRange + target.BoundingRadius;
            float trueERange = target.BoundingRadius + E.Range;

            float dist = Player.Distance(target);
            if (dist > trueAARange && dist < trueERange)
            {
                    E.Cast(target.ServerPosition);
            }
        }
        public static void useRSmart(Obj_AI_Base target)
        {   
            if (!Player.HasBuff("RivenFengShuiEngine") && !E.IsReady())
                R.Cast();
            else if (target is Obj_AI_Hero)
            {
                Game.PrintChat("shield?: "+target.ScriptHealthBonus);
                var targ = target as Obj_AI_Hero;
                PredictionOutput po = R.GetPrediction(targ, true);
                if (po.Hitchance == HitChance.High)
                    if (Player.GetSpellDamage(target, SpellSlot.R) > ((targ.Health + target.ScriptHealthBonus)) && R.IsReady())
                        R.Cast(po.CastPosition);
            }
           
        }


        public static bool useHydra(Obj_AI_Base target)
        {
           // Console.WriteLine("Hydar da useee");
            if (target.Distance(Player.ServerPosition) < (400 + target.BoundingRadius-20))
            {
                Items.UseItem(3074, target);
                 Items.UseItem(3077, target);
                return true;
            }
            return false;
        }

        public static Vector3 difPos()
        {
            Vector3 pPos = Player.ServerPosition;
            return pPos + new Vector3(300, 300, 0);
        }

        public static void reachWithE(Obj_AI_Base target)
        {
            if (!E.IsReady())
                return;
            float trueAARange = Player.AttackRange + target.BoundingRadius;
            float trueERange = target.BoundingRadius + E.Range;

            float dist = Player.Distance(target);
            Vector2 walkPos = new Vector2();
            if (target.IsMoving)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                walkPos = tpos + (path * 100);
            }
            float targ_ms = (target.IsMoving && Player.Distance(walkPos) > dist) ? target.MoveSpeed : 0;
            float msDif = (Player.MoveSpeed - targ_ms) == 0 ? 0.0001f : (Player.MoveSpeed - targ_ms);
            float timeToReach = (dist - trueAARange) / msDif;
            //Console.WriteLine(timeToReach);
            if (dist > trueAARange && dist < trueERange)
            {
                if (timeToReach > 1.7f || timeToReach < 0.0f)
                {
                    E.Cast(target.ServerPosition);
                }
            }
        }

        public static void useSecRSmart(Obj_AI_Base target)
        {

        }

        public static void cancelAnim()
        {
            Orbwalking.ResetAutoAttackTimer();
            Console.WriteLine("Cansel anim");
            if (!Riven.useHydra(Riven.orbwalker.GetTarget()))
            {
                if (W.IsReady())
                    Riven.useWSmart(Riven.orbwalker.GetTarget());
                else
                {
                    Game.Say("/l");
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y)).Send();
                }
                   
            }


           //  Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(fill iterator up)).Send();
        }

    }
}
