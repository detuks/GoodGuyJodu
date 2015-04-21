using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace LucianSharp.SidaRemake
{
    class DamagePrediction
    {
        internal enum PredType
        {
            PRED_LAST_HIT = 0,
		    PRED_TWO_HITS = 1,
		    PRED_SKILL = 2,
		    PRED_UNKILLABLE = 3
        }

        public static Obj_AI_Hero player = ObjectManager.Player;

        /// <summary>
        ///     Gets minion health based on pred type
        /// </summary>
        public static float getPred(Obj_AI_Base minion, PredType type, SpellDataInst spell = null)
        {
            var result = minion.Health;
            var predHealth = minion.Health;
            switch (type)
            {
                case PredType.PRED_LAST_HIT:
                {
                    var time = LXOrbwalker.GetCurrentWindupTime() + 1000* player.Distance(minion)/ player.BasicAttack.MissileSpeed;
                    predHealth = DtsHealthPrediction.GetHealthPrediction(minion, (int)time);
                    result = predHealth;
                    break;
                }
                case PredType.PRED_TWO_HITS:
                {
                    var time = LXOrbwalker.GetCurrentWindupTime() + 1000 * player.Distance(minion) / player.BasicAttack.MissileSpeed;
                    predHealth = DtsHealthPrediction.GetHealthPrediction(minion, (int)(time*2));
                    result = predHealth;
                    break;
                }
                case PredType.PRED_SKILL:
                {
                    var time = (int)(spell.SData.SpellCastTime*1000 + 1000 * player.Distance(minion) / spell.SData.MissileSpeed);
                    predHealth = DtsHealthPrediction.GetHealthPrediction(minion, time);
                    result = predHealth;
                    break;
                }
                case PredType.PRED_UNKILLABLE:
                {
                    var time = LXOrbwalker.GetCurrentWindupTime() + 1000 * player.Distance(minion) / player.BasicAttack.MissileSpeed;
                    predHealth = DtsHealthPrediction.GetHealthPrediction(minion, (int)(time*1.5f));
                    result = predHealth;
                    break;
                }
            }


            return result;
        }

    }
}
