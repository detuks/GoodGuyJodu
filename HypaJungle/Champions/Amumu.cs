using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    class Amumu : Jungler
    {
        public Amumu()
        {
            setUpSpells();
            setUpItems();
            levelUpSeq = new Spell[] { W,E,Q,E,E,R,E,E,Q,Q,R,Q,Q,W,W,R,W,W };
            buffPriority = 10;
        }

        public override void setUpSpells()
        {
            recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 375);
        }

        public override void setUpItems()
        {
            #region itemsToBuyList
           buyThings = new List<ItemToShop>
            {
                 new ItemToShop()
                {
                    goldReach = 475,
                    itemsMustHave = new List<int>{},
                    itemIds = new List<int>{1039,2003,2003,2003,2003,3340}
                },
                new ItemToShop()
                {
                    goldReach = 470,
                    itemsMustHave = new List<int>{1039},
                    itemIds = new List<int>{1080,2003,2003}
                },
                new ItemToShop()
                {
                    goldReach = 890,
                    itemsMustHave = new List<int>{1080},
                    itemIds = new List<int>{3108,2003,2003}
                },
                new ItemToShop()
                {
                    goldReach = 805,
                    itemsMustHave = new List<int>{3108},
                    itemIds = new List<int>{3206,1001}
                },
                new ItemToShop()
                {
                    goldReach = 9999999,
                    itemsMustHave = new List<int>{3206},
                    itemIds = new List<int>{}
                }
            };
            #endregion

            checkItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
           /* if (Q.IsReady())
            {
                if ((minion.Health / getDPS(minion) < 2.3f))
                    return;

                PredictionOutput po = Q.GetPrediction(minion);
                if (po.Hitchance >= HitChance.Low)
                {
                    Q.Cast(po.CastPosition);
                }
                if (po.Hitchance == HitChance.Collision)
                {
                    player.IssueOrder(GameObjectOrder.MoveTo, minion.Position);
                }
            }*/
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady())
            {
                if (W.Instance.ToggleState == 1)
                {
                    W.Cast();
                }
            }

        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.Distance(player) < 340+minion.BoundingRadius && E.GetDamage(minion)*0.7f<minion.Health)
            {
                E.Cast();
            }
        }

        public override void UseR(Obj_AI_Minion minion)
        {

        }

        public override void attackMinion(Obj_AI_Minion minion, bool onlyAA)
        {
            if (JungleOrbwalker.CanAttack())
            {
                UseQ(minion);
                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.attackMinion(minion, minion.Position.To2D().Extend(player.Position.To2D(), 150).To3D());
        }

        public override void castWhenNear(JungleCamp camp)
        {
            if (JungleClearer.focusedCamp != null && Q.IsReady())
            {
                float dist = player.Distance(JungleClearer.focusedCamp.Position);
                if (dist < Q.Range * 0.9f && dist > 200)
                {
                    Q.Cast(camp.Position);
                }
            }
        }

        public override void doAfterAttack(Obj_AI_Base minion)
        {
            
        }

        public override void doWhileRunningIdlin()
        {
            //disable W
            if (W.IsReady())
            {
                if (W.Instance.ToggleState == 2)
                {
                    W.Cast();
                }
            }
        }

        public override float getDPS(Obj_AI_Minion minion)
        {
            float dps = 0;
            if (Q.Level != 0)
                dps += Q.GetDamage(minion) / Qdata.Cooldown;
            if (E.Level != 0)
                dps += E.GetDamage(minion) / (Qdata.Cooldown-2);
            dps += (float)player.GetAutoAttackDamage(minion) * player.AttackSpeedMod;
            dpsFix = dps;
            return (dps == 0) ? 999 : dps;
        }

        public override bool canMove()
        {
            return true;
        }

        public override float canHeal(float inTime, float killtime)
        {
            return player.HPRegenRate*inTime;
        }
    }
}
