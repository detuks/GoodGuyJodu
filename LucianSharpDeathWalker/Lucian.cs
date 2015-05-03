using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DetuksSharp;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LucianSharp
{
    class Lucian
    {
        internal enum QhitChance
        {
            himself=0,
            easy=1,
            medium=2,
            hard=3,
            wontHit =4
        }

        public static Obj_AI_Hero player = ObjectManager.Player;

        public static SummonerItems sumItems = new SummonerItems(player);

        public static Spellbook sBook = player.Spellbook;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 600);
        public static Spell W = new Spell(SpellSlot.W, 1000);
        public static Spell E = new Spell(SpellSlot.E, 425);
        public static Spell R = new Spell(SpellSlot.R, 1400);

        public static void setSkillShots()
        {
            Q.SetSkillshot(0.35f, Qdata.SData.LineWidth, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.30f, 80f, 1600f, true, SkillshotType.SkillshotLine);
        }

        public static void doCombo(Obj_AI_Hero target)
        {
            try
            {

            if (target == null || !target.IsValid)
                return;
            if (DeathWalker.ForcedTarget != null && DeathWalker.ForcedTarget is Obj_AI_Hero)
                target = (Obj_AI_Hero)DeathWalker.ForcedTarget;
            useItems(target);
            if(target.Distance(player)>750 && !player.IsDashing() && LucianSharp.Config.Item("useQ").GetValue<bool>())
                useQonTarg(target,QhitChance.medium);
                //if(W.IsReady())
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }

        public static void doHarass(Obj_AI_Hero target)
        {
            if (target == null || !target.IsValid || player.Mana<200)
                return;


            if (!DeathWalker.inAutoAttackRange(target))
                useQonTarg(target, QhitChance.medium);
        }

        public static void useItems(Obj_AI_Hero target)
        {
            if (target.Distance(player) < 500)
            {
                sumItems.cast(SummonerItems.ItemIds.Ghostblade);
            }
            if (target.Distance(player) < 300)
            {
                sumItems.cast(SummonerItems.ItemIds.Hydra);
            }
            if (target.Distance(player) < 300)
            {
                sumItems.cast(SummonerItems.ItemIds.Tiamat);
            }
            if (target.Distance(player) < 300)
            {
                sumItems.cast(SummonerItems.ItemIds.Cutlass, target);
            }
            if (target.Distance(player) < 500 && (player.Health / player.MaxHealth) * 100 < 85)
            {
                sumItems.cast(SummonerItems.ItemIds.BotRK, target);
            }
        }

        public static bool useQonTarg(Obj_AI_Base target, QhitChance hitChance)
        {
            if (!Q.IsReady() ||  !LucianSharp.Config.Item("useQ").GetValue<bool>() || target == null)
                return false;

            if (targValidForQ(target))
            {
                Q.CastOnUnit(target);
                return true;
            }
            try
            {
                var bestQon =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(targValidForQ)
                        .OrderBy(hit => hitChOnTarg(target, hit))
                        .FirstOrDefault();
                if (bestQon != null && hitChOnTarg(target, bestQon) <= hitChance)
                {
                    Q.CastOnUnit(bestQon);
                    return true;
                }
            }
            catch (Exception)
            {
                
            }
            return false;
        }



        public static void onAfterAttack(AttackableUnit target)
        {
            // DeathWalker._lastAATick = Environment.TickCount ;

            var hero = target as Obj_AI_Hero;
            if (hero == null || DeathWalker.CurrentMode != DeathWalker.Mode.Combo) return;

            if (!useQonTarg(hero, QhitChance.hard) && LucianSharp.Config.Item("useE").GetValue<bool>())
                eAwayFrom();

            if (Q.IsReady())
            {
                if(useQonTarg((Obj_AI_Hero)target, QhitChance.medium))
                    return;
            }

            if (W.IsReady() && player.Mana>=120 && !tooEasyKill(hero) && LucianSharp.Config.Item("useW").GetValue<bool>())
            {
                var output = W.GetPrediction((Obj_AI_Hero)target);
                W.Cast(output.CastPosition);
                return;
            }
        }

        public static QhitChance hitChOnTarg(Obj_AI_Base target, Obj_AI_Base onTarg)
        {
            if(target.NetworkId == onTarg.NetworkId)
                return QhitChance.himself;

            var poly = getPolygonOn(onTarg,target.BoundingRadius*0.6f);
            var predTarPos = Prediction.GetPrediction(target, 0.35f).UnitPosition.To2D();
            var nowPos = target.Position.To2D();

            bool nowInside = poly.pointInside(nowPos);
            bool predInsode = poly.pointInside(predTarPos);

            if (nowInside && predInsode)
                return QhitChance.easy;
            if(predInsode)
                return QhitChance.medium;
            if (nowInside)
                return QhitChance.hard;

            return QhitChance.wontHit;
        }

        public static LucianMath.Polygon getPolygonOn(Obj_AI_Base target,float bonusW =0)
        {
            List<Vector2> points = new List<Vector2>();
            Vector2 rTpos = Prediction.GetPrediction(target, 0.10f).UnitPosition.To2D();
            Vector2 startP = player.ServerPosition.To2D();
            Vector2 endP = startP.Extend(rTpos, 1100 + bonusW);

            Vector2 p = (rTpos- startP);
            var per = p.Perpendicular().Normalized() * (Q.Width / 2 + bonusW);
            points.Add(startP + per);
            points.Add(startP - per);
            points.Add(endP - per);
            points.Add(endP + per);

            return new LucianMath.Polygon(points);
        }

        public static void eAwayFrom()
        {
            if(!E.IsReady())
                return;
            Vector2 backTo = player.Position.To2D();
            Obj_AI_Hero targ = null;
            int count = 0;
            foreach (var enem in ObjectManager.Get<Obj_AI_Hero>().Where(enemIsOnMe))
            {
                targ = enem;
                count++;
                backTo -= (enem.Position - player.Position).To2D();
            }

            if (count == 1 && targ.Health>fullComboOn(targ))
            {}

            if (count > 1 || (count == 1 && targ.Health > fullComboOn(targ)))
            {
                var awayTo = player.Position.To2D().Extend(backTo, 425);
                if (!inTowerRange(awayTo))
                    E.Cast(awayTo);
            }
        }

        public static float fullComboOn(Obj_AI_Base targ)
        {
            float dmg = (float)player.GetAutoAttackDamage(targ)*3;
            if (Q.IsReady())
                dmg += Q.GetDamage(targ);
            if (W.IsReady())
                dmg += W.GetDamage(targ);
            return dmg;
        }

        public static bool tooEasyKill(Obj_AI_Base target)
        {
            return target.Health < player.GetAutoAttackDamage(target)*1.5f;
        }

        public static bool enemIsOnMe(Obj_AI_Base target)
        {
            if(!target.IsMelee() || target.IsAlly || target.IsDead)
                return false;

            float distTo = target.Distance(player, true);
            float targetReack = target.AttackRange+target.BoundingRadius + player.BoundingRadius+100;
            if (distTo > targetReack*targetReack)
                return false;

            var per = target.Direction.To2D().Perpendicular();
            var dir = new Vector3(per, 0);
            var enemDir = target.Position + dir*40;
            if (distTo < enemDir.Distance(player.Position, true))
                return false;

            return true;
        }

        public static bool inTowerRange(Vector2 pos)
        {
            foreach (Obj_AI_Turret tur in ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsEnemy && tur.Health > 0))
            {
                if (pos.Distance(tur.Position.To2D()) < (850 + player.BoundingRadius))
                    return true;
            }
            return false;
        }

        public static void doKillSteal()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(ene => ene.IsEnemy && ene.IsValidTarget(1500)))
            {
                killIfPossible(enemy);
            }
        }

        public static void killIfPossible(Obj_AI_Base target)
        {
            try
            {

                var useQ = LucianSharp.Config.Item("ksOnQ").GetValue<bool>();
                var useW = LucianSharp.Config.Item("ksOnW").GetValue<bool>();
                var useE = LucianSharp.Config.Item("ksOnE").GetValue<bool>();

                var dist = target.Distance(player)-target.BoundingRadius+10;
                var tHP = target.Health;
                var qDmg = Q.GetDamage(target);
                //Console.WriteLine(tHP+" - "+qDmg);
                var wDmg = W.GetDamage(target);
                var passive = player.GetAutoAttackDamage(target)*1.45f;



                if (useE && qDmg - 20 > tHP && E.IsReady() && Q.IsReady() && dist < 1100 + E.Range && dist > Q.Range + 100)
                {
                    doProKillSteal(target);
                    DeathWalker.deathWalk(Game.CursorPos, target);
                }

                if (useQ && qDmg > tHP && Q.IsReady() && dist < 1150)
                {
                    useQonTarg(target,QhitChance.medium);
                    return;
                }


                if (useW && wDmg > tHP && W.IsReady() && dist < W.Range)
                {
                    W.CastIfHitchanceEquals(target, HitChance.Medium);
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public static Vector3 getQpredictionWithDelay(Obj_AI_Base target, float delay)
        {
            var res = Q.GetPrediction(target);
            var del = Prediction.GetPrediction(target, delay);

            var dif = del.UnitPosition - target.Position;
            return res.CastPosition + dif;
        }

        public static void doProKillSteal(Obj_AI_Base target)
        {
            var dashSpeed = E.Range/(700 + player.MoveSpeed);
            var targetPred = getQpredictionWithDelay(target, dashSpeed).To2D();
            var mins =
                MinionManager.GetMinions(player.Position, 1000)
                    .Where(min => min.Distance(targetPred, true) < 900*900)
                    .OrderByDescending(min => min.Distance(targetPred, true));
            Console.WriteLine(mins.Count());
            foreach (var min in mins)
            {

                var minPred = Prediction.GetPrediction(min, dashSpeed);

                var inter = LucianMath.getCicleLineInteraction(minPred.UnitPosition.To2D(), targetPred, player.Position.To2D(), E.Range);

                var best = inter.getBestInter(target);
                if (best.X == 0)
                    return;

                E.Cast(best);

            }
        }

        public static bool targValidForQ(Obj_AI_Base targ)
        {
            if (targ.MagicImmune || targ.IsDead || !targ.IsTargetable)
                return false;
            if (targ.IsAlly)
                return false;
            var dist = targ.Position.To2D().Distance(player.Position.To2D(), true);
            var realQRange = Q.Range + targ.BoundingRadius;
            if (dist > realQRange*realQRange)
                return false;
            return true;
        }

        public static bool gotPassiveRdy()
        {
            return ObjectManager.Player.Buffs.Any(buff => buff.Name == "lucianpassivebuff");
        }
    }
}
