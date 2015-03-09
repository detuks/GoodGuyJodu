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
    class LeeSin : Jungler
    {
        public LeeSin()
        {
            setUpSpells();
            setUpItems();
            levelUpSeq = new Spell[] { Q,E,W,Q,Q,R,Q,W,Q,W,R,W,W,E,E,R,E,E};
            buffPriority = 5;
            gotMana = false;
            startCamp = StartCamp.Golems;
        }

        public override void setUpSpells()
        {
            recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 700);
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
                    itemIds = new List<int>{1039,2003,2003,3166}
                },
                new ItemToShop()
                {
                    goldReach = 350,
                    itemsMustHave = new List<int>{1039},
                    itemIds = new List<int>{3713}
                },
                new ItemToShop()
                {
                    goldReach = 350,
                    itemsMustHave = new List<int>{3713},
                    itemIds = new List<int>{1001}
                },
                new ItemToShop()
                {
                    goldReach = 900,
                    itemsMustHave = new List<int>{3713,1001},
                    itemIds = new List<int>{1042,1042}
                },
                new ItemToShop()
                {
                    goldReach = 700,
                    itemsMustHave = new List<int>{1042,1042},
                    itemIds = new List<int>{3726}
                },
                new ItemToShop()
                {
                    goldReach = 600,
                    itemsMustHave = new List<int>{1042,1042,3715},
                    itemIds = new List<int>{3726}
                },
                new ItemToShop()
                {
                    goldReach = 999999,
                    itemsMustHave = new List<int>{3726},
                    itemIds = new List<int>{}
                },
            };
            #endregion

            checkItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            if (Q.IsReady())
            {
                if (Q.Instance.Name == "BlindMonkQOne" && BuffCount() < 2)
                {
                    if((minion.Health / getDPS(minion) < 2.3f))
                        return;

                    PredictionOutput po = Q.GetPrediction(minion);
                    if (po.Hitchance >= HitChance.Low)
                    {
                        Q.Cast(po.CastPosition);
                    }
                    if (po.Hitchance == HitChance.Collision)
                    {
                        if (player.Distance(minion) > 500)
                        {
                            Q.Cast(po.CastPosition);
                        }
                        player.IssueOrder(GameObjectOrder.MoveTo, minion.Position);
                    }
                }
                else if (BuffCount() == 0 || minion.Distance(player) > 250)
                    Q.Cast();
            }
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady())
            {
                if (W.Instance.Name == "BlindMonkWOne" && BuffCount() == 0)
                    W.Cast(player);
                else if (BuffCount() == 0)
                    W.Cast();
            }

        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.Distance(player) < 300)
            {
                if (E.Instance.Name == "BlindMonkEOne" && BuffCount() == 0)
                    E.Cast();
                else if (BuffCount() == 0)      
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
                if (Q.Instance.Name == "BlindMonkQOne")
                {
                    float dist = player.Distance(JungleClearer.focusedCamp.Position);
                    if (dist < Q.Range*0.9f && dist > 200)
                    {
                        Q.Cast(camp.Position);
                    }
                }
                else
                {
                    Q.Cast();
                }
            }
        }

        public override void doAfterAttack(Obj_AI_Base minion)
        {
            
        }

        public override void doWhileRunningIdlin()
        {

        }

        public override float getDPS(Obj_AI_Minion minion)
        {
            float dps = 0;
            if (Q.Level != 0)
                dps += Q.GetDamage(minion) / Qdata.Cooldown;
            if (E.Level != 0)
                dps += E.GetDamage(minion) / Qdata.Cooldown;
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
            return player.HPRegenRate * inTime;
        }


        /*
         * Check methods
         */


        public static int BuffCount()
        {
            var buff = player.Buffs.FirstOrDefault(b => b.Name == "blindmonkpassive_cosmetic");
            return buff == null ? 0 : buff.Count;
        }
    }
}
    