using System;
using System.Collections.Generic;
using System.Linq;
using DetuksSharp;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using DeathWalker = DetuksSharp.DeathWalker;

namespace AzirSharp
{
    class Azir
    {

        public static Obj_AI_Hero Player = ObjectManager.Player;


        public static SummonerItems sumItems = new SummonerItems(Player);

        public static Spellbook sBook = Player.Spellbook;


        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 1075);
        public static Spell W = new Spell(SpellSlot.W, 350);
        public static Spell E = new Spell(SpellSlot.E, 1150);
        public static Spell R = new Spell(SpellSlot.R, 250);

        public static List<Obj_AI_Minion> MySoldiers = new List<Obj_AI_Minion>(); 

        public static void setSkillShots()
        {
            Q.SetSkillshot(0.0f, 65f, 1500f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.0f, 65f, 1500f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 700, 1400, false, SkillshotType.SkillshotLine);
        }

        public static List<Obj_AI_Minion> getUsableSoliders()
        {
            return MySoldiers.Where(sol => !sol.IsDead).ToList();
        }

        public static Obj_AI_Minion getClosestSolider(Vector3 pos)
        {
            return MySoldiers.Where(sol => !sol.IsDead).OrderBy(sol => sol.Distance(pos, true)).FirstOrDefault();
        }

        public static void doCombo(Obj_AI_Hero targ)
        {
            if (Player.IsDead)
                return;
            if (AzirSharp.Config.Item("useW").GetValue<bool>())
            {
                castWTarget(targ);
            }
            // if (getEnemiesInSolRange().Count == 0)
            if (!W.IsReady(1600) && (!enemyInAzirRange(targ) || (targ.Health <= Q.GetDamage(targ) + DeathWalker.getRealAADmg(targ))) && AzirSharp.Config.Item("useQ").GetValue<bool>())
                castQTarget(targ);

            if (AzirSharp.Config.Item("useE").GetValue<bool>() )
                castETarget(targ);

            if (AzirSharp.Config.Item("useR").GetValue<bool>())
            {
                if (targ.Health < R.GetDamage(targ))
                    R.Cast(targ);
            }
        }

        public static void doAttack()
        {
            List<Obj_AI_Hero> enes = getEnemiesInSolRange();
            if (enes != null)
            {
                 foreach (var ene in enes)
                 {
                     if (DeathWalker.canAttack() && solisAreStill())
                     {
                         Console.WriteLine("Attack");
                         DeathWalker.doAttack(ene);
                     }
                 }
            }
        }

        public static void castQTarget(Obj_AI_Hero target)
        {
            if (!Q.IsReady())
                return;

            try
            {

                if (getMiddleDelay(target) != -1)
                {
                    PredictionOutput po2 = Prediction.GetPrediction(target, getMiddleDelay(target)*1.1f);
                    PredictionOutput po = Q.GetPrediction(target);
                    if (po2.Hitchance > HitChance.Low)
                    {
                        Q.Cast(po2.UnitPosition);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void castWTarget(Obj_AI_Hero target)
        {
            if ((!W.IsReady() && Wdata.Ammo == 0) || Player.IsAttackingPlayer)
                return;
            PredictionOutput po = Prediction.GetPrediction(target, 0.2f);
            if (Qdata.CooldownExpires < Game.Time || po.UnitPosition.Distance(Player.Position, true) < 630*630)
            {
                W.Cast(po.UnitPosition);
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

        }

        public static void castETarget(Obj_AI_Hero target)
        {
            if (!E.IsReady())
                return;

            List<Obj_AI_Minion> solis = getUsableSoliders().Where(sol => !sol.IsMoving && !sol.UnderTurret(true)).ToList();
            if (solis.Count == 0)
                return;
            foreach (var sol in solis)
            {
                float toSol = Player.Distance(sol.Position);

                //Collision.GetCollision(new List<Vector3>{sol.Position},getMyEPred(sol));
                PredictionOutput po = Prediction.GetPrediction(target,toSol/1500f);


                if (sol.Distance(po.UnitPosition)<325 && interact(Player.Position.To2D(), sol.Position.To2D(), po.UnitPosition.To2D(), 45) 
                    && interactsOnlyWithTarg(target,sol,Player.Distance(po.UnitPosition)))
                {
                    E.Cast(sol.Position);
                    return;
                }


                /*if (po.CollisionObjects.Count == 0)
                    continue;
                Console.WriteLine(po.CollisionObjects.Count);
                Obj_AI_Base col = po.CollisionObjects.OrderBy(obj => obj.Distance(Player.Position)).First();
                if (col.NetworkId == target.NetworkId)
                {
                    E.Cast(sol);
                    return;
                }*/

            }
        }

        public static void doFlyToMouse(Vector3 pos)
        {
            var closest = getClosestSolider(pos);
            var dist = Player.Distance(pos, true);
            if ((closest == null || (closest.Distance(pos, true) > dist*dist)) && W.IsReady() &&
                Qdata.CooldownExpires < Game.Time && E.IsReady(200))
            {
                W.Cast(pos);
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (closest == null || (closest.Distance(pos, true) > dist * dist))
                return;
            if ((closest.Distance(pos, true) > Player.Distance(pos, true)))
            {
                if (Q.IsReady() && E.IsReady(250))
                    Q.Cast(pos);
                if (E.IsReady() && closest.IsMoving)
                    E.CastOnUnit(closest);
            }
            else
            {
                if (E.IsReady() && Q.IsReady(150))
                {
                    E.CastOnUnit(closest);
                }
            var clo = closest.Distance(Player, true);
            if (Player.IsDashing() && Q.IsReady())
                    Q.Cast(pos);
            }



        }

        public static void goFullIn(Obj_AI_Hero target)
        {
            //R logic here!

            try
            {
                if(E.IsReady())
                    castETarget(target);
                var dist = Player.Distance(target);
                if (R.IsReady() && !Player.IsDashing())
                {
                    
                    Obj_AI_Base tower = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0).OrderBy(tur => Player.Distance(tur)).First();
                    if (tower != null)
                    {
                        var pol = DeathMath.getPolygonOn(Player.Position.Extend(tower.Position, -175).To2D(), tower.Position.To2D(), R.Width,270);
                        if(pol.pointInside(target.Position.To2D()))
                            R.Cast(tower.Position);
                    }
                }
                var aprTime = dist / E.Speed;
                var output = Prediction.GetPrediction(target, aprTime);
                if (Player.Distance(output.UnitPosition,true)<1050*1050)
                    doFlyToMouse(output.UnitPosition.Extend(Player.Position, -120));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
           
        }



        public static float getFullDmgOn(Obj_AI_Hero target)
        {
            float dmg = 0;
            if (Qdata.CooldownExpires < Game.Time)
                dmg += Q.GetDamage(target);
            if (E.IsReady())
                dmg += E.GetDamage(target);
            if (R.IsReady())
                dmg += R.GetDamage(target);

            //dmg += DeathWalker.getRealAADmg(target);

            return dmg;
        }


        public static bool interactsOnlyWithTarg(Obj_AI_Hero target,Obj_AI_Base sol, float distColser)
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(obj => obj.IsValid && obj.IsEnemy && obj.NetworkId != target.NetworkId))
            {
                float myDistToIt = Player.Distance(hero);
                PredictionOutput po = Prediction.GetPrediction(hero, myDistToIt/1500f);
                if (myDistToIt < distColser &&
                    interact(sol.Position.To2D(), Player.Position.To2D(), po.UnitPosition.To2D(), 65))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool interact(Vector2 p1, Vector2 p2, Vector2 pC, float radius)
        {

            Vector2 p3 = new Vector2();
            p3.X = pC.X + radius;
            p3.Y = pC.Y + radius;
            float m = ((p2.Y - p1.Y) / (p2.X - p1.X));
            float Constant = (m * p1.X) - p1.Y;

            float b = -(2f * ((m * Constant) + p3.X + (m * p3.Y)));
            float a = (1 + (m * m));
            float c = ((p3.X * p3.X) + (p3.Y * p3.Y) - (radius * radius) + (2f * Constant * p3.Y) + (Constant * Constant));
            float D = ((b * b) - (4f * a * c));
            if (D > 0)
            {
                return true;
            }
            else
                return false;

        }

        public static float getMiddleDelay(Obj_AI_Hero target)
        {
            float allRange = 0;
            List<Obj_AI_Minion> solis = getUsableSoliders().Where(sol => (sol.Distance(target.ServerPosition)>325 
                || sol.Distance(Prediction.GetPrediction(target,0.7f).UnitPosition)>325)).ToList();
            if (solis.Count == 0)
                return -1;
            foreach (var sol in solis)
            {
                float dist = sol.Distance(target.ServerPosition);
                allRange += dist;
            }
            return (allRange/(1500f*solis.Count));
        }

        public static PredictionInput getMyEPred(Obj_AI_Base sol)
        {
            PredictionInput pi = new PredictionInput();
            pi.Aoe = false;
            pi.Collision = true;
            pi.Delay = 0.0f;
            pi.From = Player.ServerPosition;
            pi.Radius = 65f;
            pi.Range = 1150f;
            pi.RangeCheckFrom = Player.ServerPosition;
            pi.Speed = 1500f;
            pi.Unit = sol;
            pi.Type = SkillshotType.SkillshotLine;
            pi.UseBoundingRadius = false;
            pi.CollisionObjects = new[]{ CollisionableObjects.Heroes};
            return pi;
        }

        public static PredictionInput getSoliderPred(Obj_AI_Base sol, Obj_AI_Hero target)
        {
            PredictionInput pi = new PredictionInput();
            pi.Aoe = true;
            pi.Collision = false;
            pi.Delay = 0.0f;
            pi.From = sol.ServerPosition;
            pi.Radius = 65f;
            pi.Range = 900f;
            pi.RangeCheckFrom = Player.ServerPosition;
            pi.Speed = 1500f;
            pi.Unit = target;
            pi.Type = SkillshotType.SkillshotLine;
            pi.UseBoundingRadius = true;
            return pi;
        }

        public static bool solisAreStill()
        {
            List<Obj_AI_Minion> solis = getUsableSoliders();
            foreach (var sol in solis)
            {
                if (sol.IsWindingUp)
                {
                   // Console.WriteLine("isAuta awdawdAWD");
                    return false;
                }
            }
            return true;
        }

        public static bool enemyInAzirRange(Obj_AI_Base ene)
        {
            var solis = getUsableSoliders();

            return solis.Count != 0 && solis.Where(sol => !sol.IsMoving && sol.Distance(Player, true) < 1225 * 1225).Any(sol => ene.Distance(sol) < 325);
        }

        public static List<Obj_AI_Hero> getEnemiesInSolRange()
        {
            List<Obj_AI_Minion> solis = getUsableSoliders();
            List<Obj_AI_Hero> enemies = DeathWalker.AllEnemys.Where(ene => ene.IsEnemy && ene.IsVisible && !ene.IsDead).ToList();
            List<Obj_AI_Hero> inRange = new List<Obj_AI_Hero>();

            if (solis.Count == 0)
                return null;
            foreach (var ene in enemies)
             {
                foreach (var sol in solis)
                {
                    if (ene.Distance(sol) < 350)
                    {
                        inRange.Add(ene);
                        break;
                    }
                }
            }
            return inRange;
        }

    }
}
