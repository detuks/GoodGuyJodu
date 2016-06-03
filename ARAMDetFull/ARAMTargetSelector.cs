using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ARAMDetFull
{
    class ARAMTargetSelector
    {
        public static Obj_AI_Hero getBestTarget(float range,bool calcInRadius = false, Vector3 fromPlus = new Vector3())
        {
            List<Obj_AI_Hero> targetable_ones =
                HeroManager.Enemies.Where(ob => ob != null && !IsInvulnerable(ob) && !ob.IsDead && !ob.IsZombie
                    && (ob.IsValidTarget((!calcInRadius) ? range : range + 90) || ob.IsValidTarget((!calcInRadius) ? range : range + 90, true, fromPlus))).ToList();

            if (targetable_ones.Count == 0)
                return null;
            if (targetable_ones.Count == 1)
                return targetable_ones.FirstOrDefault();

            Obj_AI_Hero lowestHp = targetable_ones.OrderBy(tar => tar.Health / ARAMSimulator.player.GetAutoAttackDamage(tar)).FirstOrDefault();
            if (lowestHp != null && lowestHp.MaxHealth != 0 && lowestHp.HealthPercent < 75)
                return lowestHp;
            Obj_AI_Hero bestStats = targetable_ones.OrderByDescending(tar => (tar.ChampionsKilled + tar.Assists) / ((tar.Deaths == 0) ? 0.5f : tar.Deaths)).FirstOrDefault();

            return bestStats;
        }

        public static bool IsInvulnerable(Obj_AI_Base target)
        {
            // Tryndamere's Undying Rage (R)
            if (target.HasBuff("Undying Rage") && target.Health <= target.Health*0.1f)
            {
                return true;
            }

            // Kayle's Intervention (R)
            if (target.HasBuff("JudicatorIntervention"))
            {
                return true;
            }
            //ChronoShift
            if (target.HasBuff("ChronoShift"))
            {
                return true;
            }

            return false;
        }

    }
}
