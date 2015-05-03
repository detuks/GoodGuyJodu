using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DetuksSharp;
using LeagueSharp;
using LeagueSharp.Common;

namespace LucianSharp.SidaRemake
{
    class MinionClearer
    {
        public static Obj_AI_Hero player = ObjectManager.Player;
        public static List<Obj_AI_Base> EnemyMinions = new List<Obj_AI_Base>();

        private static float lowerLimit = -10;

        static MinionClearer()
        {
            EnemyMinions = MinionManager.GetMinions(player.Position, 1500);
        }

        public static Obj_AI_Base findUnkillable()
        {
            EnemyMinions = MinionManager.GetMinions(player.Position, 1500);
            foreach (var enemyMinion in EnemyMinions)
            {
                var minHealthPred = DamagePrediction.getPred(enemyMinion, DamagePrediction.PredType.PRED_UNKILLABLE);
                if (minHealthPred <= 0)
                {
                    return enemyMinion;
                }
            }
            return null;
        }

        public static Obj_AI_Base findKillable()
        {
            EnemyMinions = MinionManager.GetMinions(player.Position, 1500);
            foreach (var enemyMinion in EnemyMinions)
            {
                if (!DeathWalker.inAutoAttackRange(enemyMinion))
                    continue;
                var minHealthPred = DamagePrediction.getPred(enemyMinion, DamagePrediction.PredType.PRED_LAST_HIT);
                var dmgOnMinion = player.GetAutoAttackDamage(enemyMinion, true);
                if (minHealthPred <= dmgOnMinion && minHealthPred > lowerLimit)
                {
                    return enemyMinion;
                }
            }
            return null;
        }

        public static Obj_AI_Base shouldWait()
        {
            EnemyMinions = MinionManager.GetMinions(player.Position, 1500);
            foreach (var enemyMinion in EnemyMinions)
            {
                if (!DeathWalker.inAutoAttackRange(enemyMinion))
                    continue;
                var minHealthPred = DamagePrediction.getPred(enemyMinion, DamagePrediction.PredType.PRED_TWO_HITS);
                var dmgOnMinion = player.GetAutoAttackDamage(enemyMinion, true);
                if (minHealthPred <= dmgOnMinion )
                {
                    return enemyMinion;
                }
            }
            return null;
        }

        public static Obj_AI_Base getMinUnderTower()
        {
            foreach (var tAttack in DtsHealthPrediction.ActiveAttacksTower)
            {
                var enem = tAttack.Value.Target;
                if (enem.IsValidTarget() && enem is Obj_AI_Minion && enem.IsEnemy && DeathWalker.inAutoAttackRange(enem))
                {
                     return tAttack.Value.Target;
                }
            }
            return null;
        }

        public static Obj_AI_Base getTowerTarget()
        {
           /* foreach (var tAttack in DtsHealthPrediction.ActiveAttacksTower)
            {
                var enem = tAttack.Value.Target;
                if (enem.IsValidTarget() && enem is Obj_AI_Minion && enem.IsEnemy && DeathWalker.inAutoAttackRange(enem))
                { 
                    var time = DeathWalker.GetCurrentWindupTime() + 1000* player.Distance(enem)/ player.BasicAttack.MissileSpeed;

                    var minHealthPred = DtsHealthPrediction.LaneClearHealthPrediction(enem, (int)time*2);
                    var dmgOnMinion = (float)player.GetAutoAttackDamage(enem, true);

                    if(minHealthPred > 0 && minHealthPred < dmgOnMinion)
                        continue;
                    minHealthPred -= dmgOnMinion;

                    if (minHealthPred > 0 && minHealthPred < dmgOnMinion)
                    {
                        return tAttack.Value.Target;
                    }
                }
            }*/
            return null;
        }


    }
}
