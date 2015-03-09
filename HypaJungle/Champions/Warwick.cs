using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    class Warwick : Jungler
    {
        public Warwick()
        {
            setUpSpells();
            setUpItems();
            levelUpSeq = new Spell[] { W,Q,W,E,W,R,W,Q,W,Q,R,Q,Q,E,E,R,E,E};
        }

        public override void setUpSpells()
        {
            recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 1250);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 700);
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
                    goldReach = 9999999,
                    itemsMustHave = new List<int>{3718},
                    itemIds = new List<int>{}
                }
          
            };
            #endregion

            checkItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            if(!Q.IsReady())
                return;
            float dmg = Q.GetDamage(minion);
            if ((player.Level <= 7 && (player.MaxHealth - player.Health) > dmg) || player.Level > 7)
                Q.Cast(minion);
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady() && minion.Health / getDPS(minion) > 7 && player.Distance(minion)<300)
                W.Cast();
        }

        public override void UseE(Obj_AI_Minion minion)
        {

        }

        public override void UseR(Obj_AI_Minion minion)
        {

        }

        public override void attackMinion(Obj_AI_Minion minion, bool onlyAA)
        {
            if (JungleOrbwalker.CanAttack())
            {
                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.attackMinion(minion, minion.Position.To2D().Extend(player.Position.To2D(), 150).To3D());
        }

        public override void castWhenNear(JungleCamp camp)
        {

        }

        public override void doAfterAttack(Obj_AI_Base minion)
        {
            UseQ((Obj_AI_Minion)minion);
            
        }

        public override void doWhileRunningIdlin()
        {

        }

        public override float getDPS(Obj_AI_Minion minion)
        {
            float dps = 0;
            dps += (float)player.GetAutoAttackDamage(minion) * player.AttackSpeedMod;
            dps += 30;
            dpsFix = dps;
            return dps;
        }

        public override bool canMove()
        {
            return true;
        }

        public override float canHeal(float inTime,float killtime)
        {
            float heal = killtime*player.AttackSpeedMod*(2.5f+0.5f*player.Level)*3;

            if (Q.Level != 0)
                heal += 25 + 50*Q.Level;

            return player.HPRegenRate * inTime + heal;

        }
    }
}
