using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    class Aatrox : Jungler
    {

        public bool startedMedi = false;

        public Aatrox()
        {
            setUpSpells();
            setUpItems();
            levelUpSeq = new Spell[] { W,Q,E,W,W,R,W,E,W,E,R,E,E,Q,Q,R,Q,Q};
            buffPriority = 6;
            startCamp = StartCamp.Golems;
            gotMana = false;
        }

        public override void setUpSpells()
        {
            recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 400);
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
                    itemIds = new List<int>{1039,2003,2003,3340}
                },
                new ItemToShop()
                {
                    goldReach = 675,
                    itemsMustHave = new List<int>{1039},
                    itemIds = new List<int>{3713,1001}
                },
                new ItemToShop()
                {
                    goldReach = 900,
                    itemsMustHave = new List<int>{3713,1001},
                    itemIds = new List<int>{1042,1042}
                },
                new ItemToShop()
                {
                    goldReach = 600,
                    itemsMustHave = new List<int>{1042,1042,3713},
                    itemIds = new List<int>{3726}
                },
                new ItemToShop()
                {
                    goldReach = 875,
                    itemsMustHave = new List<int>{3726},
                    itemIds = new List<int>{1037}
                },
                new ItemToShop()
                {
                    goldReach = 1425,
                    itemsMustHave = new List<int>{1037},
                    itemIds = new List<int>{3035}
                },
                new ItemToShop()
                {
                    goldReach = 1337,
                    itemsMustHave = new List<int>{3035},
                    itemIds = new List<int>{3134}
                },
                new ItemToShop()
                {
                    goldReach = 1363,
                    itemsMustHave = new List<int>{3134},
                    itemIds = new List<int>{3142}
                },
                new ItemToShop()
                {
                    goldReach = 1400,
                    itemsMustHave = new List<int>{3142},
                    itemIds = new List<int>{3144}
                },
                new ItemToShop()
                {
                    goldReach = 1800,
                    itemsMustHave = new List<int>{3144},
                    itemIds = new List<int>{3153}
                },
                new ItemToShop()
                {
                    goldReach = 88888999,
                    itemsMustHave = new List<int>{3153},
                    itemIds = new List<int>{}
                },
            };
            #endregion

            checkItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            if (Q.IsReady())
                Q.Cast(minion.Position);
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (!W.IsReady())
                return;
            var playerHpProc = 100*player.Health/player.MaxHealth;

            if (healIsOn() && playerHpProc >= 60)
            {
                W.Cast();
                Console.WriteLine("dmg on");
            }
            else if (!healIsOn() && playerHpProc <= 35)
            {
                W.Cast();
                Console.WriteLine("heal on");
            }

        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.Health / getDPS(minion) > 2 && minion.Distance(player)<=950)
                E.Cast(minion.Position);
        }

        public override void UseR(Obj_AI_Minion minion)
        {

        }

        public override void attackMinion(Obj_AI_Minion minion, bool onlyAA)
        {
            //  if (onlyAA)return;

            if (JungleOrbwalker.CanAttack())
            {
                if (minion.Distance(player) > 400 && minion.Distance(player) <= Q.Range)
                    UseQ(minion);
                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.attackMinion(minion, minion.Position.To2D().Extend(player.Position.To2D(), 100).To3D());
        }

        public override void castWhenNear(Camp camp)
        {
            if (Q.IsReady() && player.Distance(camp.campPosition) < Q.Range)
                Q.Cast(camp.campPosition);
        }

        public override void doAfterAttack(Obj_AI_Base minion)
        {
            if (minion is Obj_AI_Minion)
            {
                UseE((Obj_AI_Minion)minion);
            }

        }

        public override void doWhileRunningIdlin()
        {

        }

        public override float getDPS(Obj_AI_Minion minion)
        {
            float dps = 0;
            var qDmg = E.GetDamage(minion);//(getSpellDmgRaw(SpellSlot.Q)+50+25*Q.Level);
            var tillNext = Qdata.Cooldown;
            dps += qDmg / tillNext;
            dps += (float)player.GetAutoAttackDamage(minion) * 1.15f * player.AttackSpeedMod;
            dpsFix = dps;
            return dps;
        }


        /* public override float getDmgDoneInTime(Camp.JungleMinion camp, float time, float cdResetTime)
         {
             float damage = 0;
             //Qdmg can deal
             var qDmg = camp.UpdatedStats.physicGoesThrough * (getQdmg() + 50 + 25 * Q.Level);
            // Console.WriteLine(qDmg);
             var tillNext = ((Qdata.Cooldown == 0) ? 18 : Qdata.Cooldown) / (1 + getAAperSecond());
             if (Q.IsReady((int) (cdResetTime*1000)))
             {
                 damage += qDmg;
             }
             int qCastsMore = (int) (time/tillNext);
             damage += qDmg*qCastsMore;

             //AutoAttacks
             float timeForaa = player.AttackSpeedMod / 1.1f;
             int aaCastsMore = (int) (time/timeForaa);

             damage += camp.UpdatedStats.physicGoesThrough*(player.BaseAttackDamage + player.FlatPhysicalDamageMod)*aaCastsMore;

             return damage + time * getItemPassiveBoostDps();
         }*/

        public override float getTimeToDoDmg(Camp.JungleMinion camp, float damageToDo, float cdResetTime)
        {
            float damage = 0;
            //Qdmg can deal
            var qDmg = camp.UpdatedStats.physicGoesThrough * (getSpellDmgRaw(SpellSlot.E));
            var tillNext = ((Edata.Cooldown == 0) ? 18 : Edata.Cooldown) / (1);
            var qDps = qDmg / tillNext;
            // Console.WriteLine(qDmg);

            float timeForaa = player.AttackSpeedMod / 1.1f;
            float aaDps = camp.UpdatedStats.physicGoesThrough * (player.BaseAttackDamage + player.FlatPhysicalDamageMod + (15*player.Level+45)) / (1.1f / player.AttackDelay);
            float timeSkip = 0;
            if (Q.IsReady((int)(cdResetTime * 1000)))
            {
                damage += qDmg;
                timeSkip += tillNext;
            }

            damage += (player.BaseAttackDamage + player.FlatPhysicalDamageMod);
            if (damage >= damageToDo)
                return 1;

            float time = (damageToDo - damage + timeSkip * qDps) / (aaDps + qDps + getItemPassiveBoostDps());

            float timeWithRed = (damageToDo - damage + timeSkip * qDps) /
                                (aaDps + qDps + getItemPassiveBoostDps() + getRedBuffDmg(cdResetTime, time));

            return timeWithRed;
        }

        public override float getAAperSecond()
        {
            return 1.2f / player.AttackDelay;
        }

        public override float getAoeDmgDoneInTime(Camp.JungleMinion camp, float time, float cdResetTime)
        {
            float damage = 0;

            //Qdmg can deal
            var qDmg = camp.UpdatedStats.physicGoesThrough * (getSpellDmgRaw(SpellSlot.E));
            var tillNext = ((Edata.Cooldown == 0) ? 18 : Edata.Cooldown);
            if (E.IsReady((int)(cdResetTime * 1000)))
            {
                damage += qDmg;
            }
            int qCastsMore = (int)(time / tillNext);
            damage += qDmg * qCastsMore;

            return damage;
        }

        public override float getTimeToDoDmgAoe(Camp.JungleMinion camp, float damageToDo, float cdResetTime)
        {
            //    Console.WriteLine("AS: " + player.AttackSpeedMod + " asbase " + player.AttackDelay);
            //Qdmg can deal
            var qDmg = camp.UpdatedStats.physicGoesThrough * (getSpellDmgRaw(SpellSlot.E));
            //  Console.WriteLine("Qdmg: " + qDmg + " raw: " + getSpellDmgRaw(SpellSlot.Q));
            var tillNext = ((Edata.Cooldown == 0) ? 18 : Edata.Cooldown);
            // Console.WriteLine("Ŗeal q cd: " + tillNext);


            var castTimes = 0;
            var cdLeft = (Edata.CooldownExpires - Game.Time - cdResetTime > 0) ? Edata.CooldownExpires - Game.Time - cdResetTime : 0;
            int aproxCastTimes = (int)(damageToDo / (qDmg));
            float aproxTime = aproxCastTimes * tillNext;
            //GotNoFrogBuff
            if (timeTillFrogBuffEnd() <= cdResetTime || timeTillFrogBuffEnd() == 0 || aproxCastTimes == 0)
            {
                // Console.WriteLine("aaprox: " + Qdata.Cooldown);
                return aproxTime * aproxCastTimes + cdLeft;
            }
            float buffDmg = getFrogBuffAoe(cdResetTime, aproxTime);
            float buffQdmg = (qDmg + tillNext * buffDmg);

            //With frog
            castTimes = (int)((damageToDo - cdLeft * buffDmg) / (buffQdmg));
            float bonusTime = 0;
            bonusTime += ((damageToDo - cdLeft * buffDmg) + tillNext * buffDmg - (castTimes) * buffQdmg) / buffDmg;
            return castTimes * tillNext + cdLeft + bonusTime;

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

        public override bool canRecall()
        {
            return true;
        }

        public override float canHeal(float inTime, float killtime)
        {
            float heal = 0;
            if (player.Health + player.HPRegenRate * inTime + heal > player.MaxHealth)
            {
                return player.MaxHealth - player.Health + killtime * player.HPRegenRate;
            }

            return player.HPRegenRate * inTime + heal + killtime * player.HPRegenRate;
        }

        public override float getSkillAoePerSec()
        {
            float aoeDpsPerSec = 0;
            if (Q.Level != 0)
            {
                var qDmg = (getSpellDmgRaw(SpellSlot.E));
                var tillNext = Edata.Cooldown;
                aoeDpsPerSec += qDmg / tillNext;
            }
            return aoeDpsPerSec;
        }

        public bool healIsOn()
        {
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                if (buff.DisplayName == "AatroxWLife")
                    return true;
            }
            return false;
        }

        
    }
}
