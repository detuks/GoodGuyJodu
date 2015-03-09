using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace TalonSharp
{
    class Talon
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;


        public static SummonerItems sumItems = new SummonerItems(Player);

        public static Spellbook sBook = Player.Spellbook;

        public static Orbwalking.Orbwalker orbwalker;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 125);
        public static Spell W = new Spell(SpellSlot.W, 650);
        public static Spell E = new Spell(SpellSlot.E, 700);
        public static Spell R = new Spell(SpellSlot.R, 0);

        public static Vector3 posRCast = new Vector3();

        public static void setSkillshots()
        {
            W.SetSkillshot(0.0f, 400f, 1700f, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.0f, 400f, 1700f, false, SkillshotType.SkillshotCircle);
        }

        public static void doCombo(Obj_AI_Hero target)
        {
            igniteIfKIllable(target);
            if (!target.IsValidTarget())
                return;

            if (!W.IsReady() && !E.IsReady() && R.IsReady() && Rdata.Name == "TalonShadowAssault" && (target.Health > getAAdmg(target) || targetInRange(target, 125f))
                && target.Health<(getRdmg(target)*2 + getTargetBleedDmg(target)))
            {
                PredictionOutput po = R.GetPrediction(target);
                if (po.Hitchance >= HitChance.Medium)
                {
                    R.Cast();
                    posRCast = Player.ServerPosition;
                }
            }

            if (Rdata.Name == "TalonShadowAssault")
            {
                if (target.Health<= getRdmg(target) && target.Distance(posRCast) < R.Range)
                    R.Cast();
            }


            if (E.IsReady() && targetInRange(target, E.Range))
            {
                E.Cast(target);
            }
            if (W.IsReady() && !E.IsReady() && targetInRange(target, W.Range) && !targetInRange(target, 100) && !Player.IsChanneling
                && (!targetInRange(target,250) || targetHasCut(target)))
            {
               
                    W.Cast(target);
            }
            castItemsFull(target);
        }

        private static void castItemsFull(Obj_AI_Base target)
        {
            if (target.Distance(Player) < 500)
            {
                sumItems.cast(SummonerItems.ItemIds.Ghostblade);
                sumItems.castIgnite((Obj_AI_Hero) target);
            }
            if (target.Distance(Player) < 500)
            {
                sumItems.cast(SummonerItems.ItemIds.BotRK, target);
                sumItems.cast(SummonerItems.ItemIds.Cutlass, target);
            }
            if (target.Distance(Player.ServerPosition) < (400 + target.BoundingRadius - 20))
            {
                sumItems.cast(SummonerItems.ItemIds.Tiamat);
                sumItems.cast(SummonerItems.ItemIds.Hydra);
            }
        }

        public static void doHarassHard(Obj_AI_Hero target)
        {
            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
             igniteIfKIllable(target);
            if (targetInRange(target, 125f) && !Player.IsChanneling)
            {
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }

            if (E.IsReady() && targetInRange(target, E.Range))
            {
                E.Cast(target);
            }

            if (W.IsReady() && targetInRange(target, W.Range) && targetHasCut(target) && targetBleeds(target) && !targetInRange(target, 80))
            {
                PredictionOutput po = W.GetPrediction(target);
                if (po.Hitchance == HitChance.High)
                {
                    W.Cast(po.CastPosition);
                }
            }

            castItemsFull(target);
        }

        public static void doHarassSmall(Obj_AI_Hero target)
        {
            if (W.IsReady() && targetInRange(target, W.Range))
            {
                PredictionOutput po = W.GetPrediction(target);
                if (po.Hitchance == HitChance.High)
                {
                    W.Cast(po.CastPosition);
                }
            }
        }

        public static void igniteIfKIllable(Obj_AI_Hero target)
        {
            if(target.Health < getFullComboDmg(target))
                sumItems.castIgnite(target);
        }

        public static void castHydra(Obj_AI_Base target)
        {
            if (targetInRange(target, 300f))
            {
                sumItems.cast(SummonerItems.ItemIds.Hydra);
                sumItems.cast(SummonerItems.ItemIds.Tiamat);
            }
        }

        public static bool targetInRange(Obj_AI_Base target, float range)
        {
            PredictionOutput po = Prediction.GetPrediction(target, 0.5f);
            float dist2 = Vector2.DistanceSquared(po.UnitPosition.To2D(), Player.ServerPosition.To2D());
            float range2 = range * range + target.BoundingRadius * target.BoundingRadius;
            return dist2 < range2;
        }

        public static float getNoRDmg(Obj_AI_Hero target)
        {
            float fullDmg = getAAdmg(target);
            fullDmg += getQdmg(target);
            fullDmg += getWdmg(target) * 2;
            if (haveHydra())
                fullDmg += getHydraDmg(target);
            else if (haveTiamat())
                fullDmg += getTiamDmg(target);
            float mult = 1.1f + getEproc();
            return fullDmg * mult;
        }

        public static float getFullComboDmg(Obj_AI_Hero target)
        {
            float fullDmg = getAAdmg(target);
            if(Q.IsReady())
                fullDmg += getQdmg(target);
            if (W.IsReady())
             fullDmg += getWdmg(target)*2;
            if (R.IsReady())
                fullDmg += getRdmg(target) * 2;
            if(haveHydra())
                fullDmg += getHydraDmg(target);
            else if(haveTiamat())
                fullDmg += getTiamDmg(target);
            float mult = 1.1f + ((E.IsReady())?getEproc():0f);
            return fullDmg * mult;
        }

        public static bool targetHasCut(Obj_AI_Hero target)
        {
            return target.Buffs.Any(buf => buf.Name.Contains("talondamageamp"));
        }

        public static bool targetBleeds(Obj_AI_Hero target)
        {
            return target.Buffs.Any(buf => buf.Name.Contains("talonbleeddebuff"));
        }

        public static float getTargetBleedDmg(Obj_AI_Hero target)
        {
            if (!targetBleeds(target)) return 0;

            var bufInst = target.Buffs.First(buf => buf.Name.Contains("talonbleeddebuff"));
            var time = bufInst.EndTime - Game.Time;
            var qdmgFull = Player.FlatPhysicalDamageMod + 10*Q.Level;
            return (float) qdmgFull*(time/6);
        }

        public static bool haveHydra()
        {
            return Items.CanUseItem((int)SummonerItems.ItemIds.Hydra);
        }

        public static bool haveTiamat()
        {
            return Items.CanUseItem((int)SummonerItems.ItemIds.Tiamat);
        }

        public static float getHydraDmg(Obj_AI_Hero target)
        {
            return (float) Player.GetAutoAttackDamage(target)*0.7f;
        }

        public static float getTiamDmg(Obj_AI_Hero target)
        {
            return (float)Player.GetAutoAttackDamage(target) * 0.5f;
        }

        public static float getAAdmg(Obj_AI_Hero target)
        {
            return (float) Player.GetAutoAttackDamage(target);
        }

        public static float getQdmg(Obj_AI_Hero target)
        {
            return (float) Q.GetDamage(target);
        }

        public static float getWdmg(Obj_AI_Hero target)
        {
            return (float) W.GetDamage(target);
        }

        public static float getEproc()
        {
            return 0.03f*Player.Level;
        }

        public static float getRdmg(Obj_AI_Hero target)
        {
            return (float) R.GetDamage(target);
        }
    }
}
