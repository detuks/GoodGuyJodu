using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace AzirSharp
{
    class Azir
    {

        public static Obj_AI_Hero Player = ObjectManager.Player;


        public static SummonerItems sumItems = new SummonerItems(Player);

        public static Spellbook sBook = Player.Spellbook;

        public static Orbwalking.Orbwalker orbwalker;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 750);
        public static Spell W = new Spell(SpellSlot.W, 450);
        public static Spell E = new Spell(SpellSlot.E, 650);
        public static Spell R = new Spell(SpellSlot.R, 250);

        public static List<Obj_AI_Minion> MySoldiers = new List<Obj_AI_Minion>(); 

        public static void setSkillShots()
        {
            Q.SetSkillshot(0.0f, 65f, 1500f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.0f, 65f, 1500f, false, SkillshotType.SkillshotLine);
        }

        public static List<Obj_AI_Minion> getUsableSoliders()
        {
            return MySoldiers.Where(sol => !sol.IsDead).ToList();
        }

        public static void doCombo(Obj_AI_Hero targ)
        {
            if (Player.IsDead)
                return;
            if (AzirSharp.Config.Item("useW").GetValue<bool>())
                castWTarget(targ);
            // if (getEnemiesInSolRange().Count == 0)
            if (AzirSharp.Config.Item("useQ").GetValue<bool>())
                castQTarget(targ);

            if (AzirSharp.Config.Item("useE").GetValue<bool>())
                castETarget(targ);
        }

        public static void doAttack()
        {
            List<Obj_AI_Hero> enes = getEnemiesInSolRange();
            if (enes != null)
            {
                 foreach (var ene in enes)
                 {

                     if (Orbwalking.CanMove(0) && Orbwalking.CanAttack() && solisAreStill())
                     {
                         Orbwalking.LastAATick = Environment.TickCount;
                         Player.IssueOrder(GameObjectOrder.AttackUnit, ene);
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
            if (!W.IsReady() && Wdata.Ammo == 0)
                return;
            PredictionOutput po = Prediction.GetPrediction(target, 0.2f);
            W.Cast(po.UnitPosition);

        }

        public static void castETarget(Obj_AI_Hero target)
        {
            if (!E.IsReady())
                return;

            List<Obj_AI_Minion> solis = getUsableSoliders().Where(sol => !sol.IsMoving).ToList();
            if (solis.Count == 0)
                return;
            foreach (var sol in solis)
            {
                float toSol = Player.Distance(sol.Position);

                //Collision.GetCollision(new List<Vector3>{sol.Position},getMyEPred(sol));
                PredictionOutput po = Prediction.GetPrediction(target,toSol/1500f);


                if (sol.Distance(po.UnitPosition)<325 && interact(Player.Position.To2D(), sol.Position.To2D(), po.UnitPosition.To2D(), 65) 
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
            pi.Range = 1030f;
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
            pi.Range = 830f;
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


        public static List<Obj_AI_Hero> getEnemiesInSolRange()
        {
            List<Obj_AI_Minion> solis = getUsableSoliders();
            List<Obj_AI_Hero> enemies = ObjectManager.Get<Obj_AI_Hero>().Where(ene => ene.IsEnemy && ene.IsVisible && !ene.IsDead).ToList();
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
