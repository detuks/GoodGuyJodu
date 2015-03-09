using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;

namespace TryndSharp
{
    class Trynd
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static Spellbook sBook = Player.Spellbook;

        public static Orbwalking.Orbwalker orbwalker;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 0);
        public static Spell W = new Spell(SpellSlot.W, 400);
        public static Spell E = new Spell(SpellSlot.E, 660);
        public static Spell R = new Spell(SpellSlot.R, 0);


        public static void doCombo(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget())
                return;

           // Console.WriteLine("Double COmbo");
           // if (TryndSharp.Config.Item("useQ").GetValue<bool>())
            useQSmart();
            if (TryndSharp.Config.Item("useW").GetValue<bool>())
                useWSmart(target);
            if (TryndSharp.Config.Item("useE").GetValue<bool>())
                useESmart(target);
        }

        public static void setSkillShots()
        {
            E.SetSkillshot(0.5f, 225f, 700f, false, SkillshotType.SkillshotLine);
        }

        public static void useQSmart()
        {
            if (!Q.IsReady())
                return;
            if (myHpProc() <= TryndSharp.Config.Item("QonHp").GetValue<Slider>().Value)
                Q.Cast();
        }

        public static void useWSmart(Obj_AI_Hero target)
        {
            if (!W.IsReady())
                return;
            //Console.WriteLine("use W");

            float trueAARange = Player.AttackRange + target.BoundingRadius;
            float trueERange = target.BoundingRadius + W.Range;

            float dist = Player.Distance(target);
            Vector2 dashPos = new Vector2();
            if (target.IsMoving)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                dashPos = tpos + (path * 100);
            }
            float targ_ms = (target.IsMoving && Player.Distance(dashPos) > dist) ? target.MoveSpeed : 0;
            float msDif = (Player.MoveSpeed - targ_ms) == 0 ? 0.0001f : (Player.MoveSpeed - targ_ms);
            float timeToReach = (dist - trueAARange) / msDif;
            //Console.WriteLine(timeToReach);
            if (dist > trueAARange && dist < trueERange)
            {
                if (timeToReach > 1.7f || timeToReach < 0.0f)
                {
                    W.Cast();
                }
            }

        }

        public static void useESmart(Obj_AI_Hero target)
        {
            if (!E.IsReady())
                return;
          //  Console.WriteLine("use E");
            float trueAARange = Player.AttackRange + target.BoundingRadius;
            float trueERange = target.BoundingRadius + E.Range;

            float dist = Player.Distance(target);
            Vector2 movePos = new Vector2();
            if (target.IsMoving)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                movePos = tpos + (path * 100);
            }
            float targ_ms = (target.IsMoving && Player.Distance(movePos) > dist) ? target.MoveSpeed : 0;
            float msDif = (Player.MoveSpeed - targ_ms) == 0 ? 0.0001f : (Player.MoveSpeed - targ_ms);
            float timeToReach = (dist - trueAARange) / msDif;
          //  Console.WriteLine(timeToReach);
            if (dist > trueAARange && dist < trueERange)
            {
                if (timeToReach > 1.7f || timeToReach < 0.0f)
                {
                    E.Cast(target);
                }
            }

        }


        public static int myHpProc()
        {
            return (int)((Player.Health / Player.MaxHealth) * 100);
        }


    }
}
