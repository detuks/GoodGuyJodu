using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    class MasterYi : Jungler
    {

        public bool startedMedi = false;

        public MasterYi()
        {
            setUpSpells();
            setUpItems();
            levelUpSeq = new Spell[] { Q, W, E, Q, Q, R, Q, E, Q, E, R, E, E, W, W, R, W };
            buffPriority = 3;
        }

        public override void setUpSpells()
        {
            recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 0);
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
                    itemIds = new List<int>{1039,2003,2003,3166}
                },
                new ItemToShop()
                {
                    goldReach = 350,
                    itemsMustHave = new List<int>{1039},
                    itemIds = new List<int>{3715}
                },
                new ItemToShop()
                {
                    goldReach = 350,
                    itemsMustHave = new List<int>{3715},
                    itemIds = new List<int>{1001}
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
            if (Q.IsReady() && minion.Health > Q.GetDamage(minion))
                Q.Cast(minion);
        }

        public override void UseW(Obj_AI_Minion minion)
        {

        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.Health/getDPS(minion)>4)
                E.Cast();
        }

        public override void UseR(Obj_AI_Minion minion)
        {

        }

        public override void attackMinion(Obj_AI_Minion minion,bool onlyAA)
        {
          //  if (onlyAA)return;

            if (JungleOrbwalker.CanAttack())
            {
                if(minion.Distance(player)>300)
                    UseQ(minion);
                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.attackMinion(minion, minion.Position.To2D().Extend(player.Position.To2D(), 100).To3D());
        }

        public override void castWhenNear(JungleCamp camp)
        {

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
            if (W.IsReady() && player.Health < player.MaxHealth*0.7f)
            {
                startedMedi = true;
                W.Cast();
            }
        }

        public override float getDPS(Obj_AI_Minion minion)
        {
            float dps = 0;
            dps += Q.GetDamage(minion)*2/Qdata.Cooldown;
            dps +=(float) player.GetAutoAttackDamage(minion)*1.15f*player.AttackSpeedMod;
            dpsFix = dps;
            return dps;
        }

        public override bool canMove()
        {
            if (player.HasBuff("Meditate") && player.Health != player.MaxHealth)
            {
                startedMedi = false;
                return false;
            }

            if (startedMedi)
                return false;

           
            return true;
        }

        public override float canHeal(float inTime, float killtime)
        {
            float heal = 0;
            if (W.IsReady((int) (inTime*1000)))
            {
                heal =4*( W.Level * 20 + 10 + 0.3f * player.FlatMagicDamageMod);

            }
            return player.HPRegenRate * inTime + heal;
        }
    }
}
