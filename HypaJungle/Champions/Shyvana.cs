using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    class Shyvana :Jungler
    {
       public Shyvana()
        {
            setUpSpells();
            setUpItems();
            levelUpSeq = new Spell[] {W,Q,E,W,W,R,W,E,W,E,R,E,E,Q,Q,R,Q,Q};
            buffPriority = 5;
            gotMana = false;
        }

        public override void setUpSpells()
        {
            recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R, 0);
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
                    itemIds = new List<int>{1039,2003,2003,2003}
                },
                new ItemToShop()
                {
                    goldReach = 700,
                    itemsMustHave = new List<int>{1039},
                    itemIds = new List<int>{3715,1001}
                },
                new ItemToShop()
                {
                    goldReach = 900,
                    itemsMustHave = new List<int>{3715,1001},
                    itemIds = new List<int>{1042,1042}
                },
                new ItemToShop()
                {
                    goldReach = 700,
                    itemsMustHave = new List<int>{1042,1042},
                    itemIds = new List<int>{3718}
                },
                new ItemToShop()
                {
                    goldReach = 600,
                    itemsMustHave = new List<int>{1042,1042,3715},
                    itemIds = new List<int>{3718}
                },
                new ItemToShop()
                {
                    goldReach = 999999,
                    itemsMustHave = new List<int>{3718},
                    itemIds = new List<int>{}
                },
            };
            #endregion

            checkItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            if (Q.IsReady())
                Q.Cast();
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady())
                W.Cast();
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && player.Distance(minion)<E.Range)
                E.Cast(minion.Position);
        }

        public override void UseR(Obj_AI_Minion minion)
        {

        }

        public override void attackMinion(Obj_AI_Minion minion, bool onlyAA)
        {
            UseW(minion);
            if (JungleOrbwalker.CanAttack())
            {
                UseQ(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.attackMinion(minion, minion.Position.To2D().Extend(player.Position.To2D(), 100).To3D());
        }

        public override void castWhenNear(JungleCamp camp)
        {
            if (JungleClearer.focusedCamp != null && E.IsReady())
            {
                float dist = player.Distance(JungleClearer.focusedCamp.Position);
                if (dist < E.Range * 0.8f && dist >200)
                {
                    E.Cast(camp.Position);
                }
            }
        }

        public override void doAfterAttack(Obj_AI_Base minion)
        {
            if (minion is Obj_AI_Minion)
            {
                UseQ((Obj_AI_Minion)minion);
            }
        }

        public override void doWhileRunningIdlin()
        {
            if (JungleClearer.focusedCamp != null && E.IsReady())
            {
                float dist = player.Distance(JungleClearer.focusedCamp.Position);
                if (dist/player.MoveSpeed > 8)
                {
                    UseW(null);
                }
            }
        }

        public override float getDPS(Obj_AI_Minion minion)
        {
            float dps = 0;
            if (Q.Level != 0)
                dps += Q.GetDamage(minion) / Qdata.Cooldown;
            if (W.Level != 0)
                dps += W.GetDamage(minion) / Qdata.Cooldown;
            if(E.Level != 0)
                dps +=E.GetDamage(minion) / Qdata.Cooldown;
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
    }
}
