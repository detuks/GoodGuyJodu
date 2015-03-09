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
            levelUpSeq = new Spell[] { Q, W, W, E, Q, R, Q, Q, Q, E, R, E, E, E, W, R, W };
            buffPriority = 6;
            startCamp = StartCamp.Golems;
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
            if (Q.IsReady() && (minion.Health > Q.GetDamage(minion) || JungleClearer.focusedCamp.aliveMinCount()>1))
                Q.Cast(minion);
        }

        public override void UseW(Obj_AI_Minion minion)
        {

        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.Health/getDPS(minion)>4 && minion.Distance(player)<250)
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
                if(minion.Distance(player)>200)
                    UseQ(minion);
                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.attackMinion(minion, minion.Position.To2D().Extend(player.Position.To2D(), 100).To3D());
        }

        public override void castWhenNear(Camp camp)
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
            var qDmg = Q.GetDamage(minion);//(getSpellDmgRaw(SpellSlot.Q)+50+25*Q.Level);
            var tillNext = Qdata.Cooldown / (1 + 0.5f * player.AttackSpeedMod);
            dps += qDmg / tillNext;
            dps +=(float) player.GetAutoAttackDamage(minion)*1.15f*player.AttackSpeedMod;
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
            var qDmg = camp.UpdatedStats.physicGoesThrough * (getQdmg() + 50 + 25 * Q.Level);
            var tillNext = ((Qdata.Cooldown == 0) ? 18 : Qdata.Cooldown )/ (1 + getAAperSecond());
            var qDps = qDmg / tillNext;
           // Console.WriteLine(qDmg);

            float timeForaa = player.AttackSpeedMod / 1.1f;
            float aaDps = camp.UpdatedStats.physicGoesThrough * (player.BaseAttackDamage + player.FlatPhysicalDamageMod) / (1.1f / player.AttackDelay);
            float timeSkip = 0;
            if (Q.IsReady((int) (cdResetTime*1000)))
            {
                damage += qDmg;
                timeSkip += tillNext;
            }

            damage += (player.BaseAttackDamage + player.FlatPhysicalDamageMod);
            if (damage >= damageToDo)
                return 1;

            float time = (damageToDo - damage + timeSkip * qDps) / (aaDps + qDps + getItemPassiveBoostDps());

            float timeWithRed = (damageToDo - damage + timeSkip*qDps)/
                                (aaDps + qDps + getItemPassiveBoostDps() + getRedBuffDmg(cdResetTime, time));

            return timeWithRed;
        }

        public override float getAAperSecond()
        {
            return 1.2f/player.AttackDelay;
        }

        public override float getAoeDmgDoneInTime(Camp.JungleMinion camp, float time, float cdResetTime)
        {
            float damage = 0;

            //Qdmg can deal
            var qDmg = camp.UpdatedStats.physicGoesThrough * (getQdmg ()+ 50 + 25 * Q.Level);
            var tillNext = ((Qdata.Cooldown == 0) ? 18 : Qdata.Cooldown) / (1 + getAAperSecond());
            if(Q.IsReady((int)(cdResetTime*1000)))
            {
                damage += qDmg;
            }
            int qCastsMore = (int)(time/tillNext);
            damage += qDmg * qCastsMore;
            
            return damage;
        }

        public override float getTimeToDoDmgAoe(Camp.JungleMinion camp, float damageToDo, float cdResetTime)
        {
        //    Console.WriteLine("AS: " + player.AttackSpeedMod + " asbase " + player.AttackDelay);
            //Qdmg can deal
            var qDmg = camp.UpdatedStats.physicGoesThrough * (getQdmg() + 50 + 25 * Q.Level);
          //  Console.WriteLine("Qdmg: " + qDmg + " raw: " + getSpellDmgRaw(SpellSlot.Q));
            var tillNext = ((Qdata.Cooldown == 0) ? 18 : Qdata.Cooldown) / (1 + (getAAperSecond()));
           // Console.WriteLine("Ŗeal q cd: " + tillNext);


            var castTimes = 0;
            var cdLeft = (Qdata.CooldownExpires - Game.Time - cdResetTime>0)?Qdata.CooldownExpires - Game.Time - cdResetTime:0;
            int aproxCastTimes = (int)(damageToDo / (qDmg));
            float aproxTime = aproxCastTimes*tillNext;
            //GotNoFrogBuff
            if (timeTillFrogBuffEnd() <= cdResetTime || timeTillFrogBuffEnd() == 0 || aproxCastTimes == 0)
            {
               // Console.WriteLine("aaprox: " + Qdata.Cooldown);
                return aproxTime*aproxCastTimes + cdLeft;
            }
            float buffDmg = getFrogBuffAoe(cdResetTime, aproxTime);
            float buffQdmg = (qDmg + tillNext*buffDmg);

            //With frog
            castTimes = (int)((damageToDo - cdLeft * buffDmg) / (buffQdmg));
            float bonusTime = 0;
            bonusTime += ((damageToDo - cdLeft*buffDmg) + tillNext*buffDmg - (castTimes)*buffQdmg)/buffDmg;
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
            return !W.IsReady(2000);
        }

        public override float canHeal(float inTime, float killtime)
        {
            float heal = 0;
            if (W.Level != 0 && W.IsReady((int) (inTime*1000)))
            {
                heal =4*( W.Level * 20 + 10 + 0.3f * player.FlatMagicDamageMod);
                
            }
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
                var qDmg = (getSpellDmgRaw(SpellSlot.Q)+50+25*Q.Level);
                var tillNext = Qdata.Cooldown/(1 + 0.5f*player.AttackSpeedMod);
                aoeDpsPerSec += qDmg/tillNext;
            }
            return aoeDpsPerSec;
        }

        public float getQdmg()
        {
            if (Q.Level == 0)
                return 0;
            float dmg = -20 + 45 * Q.Level + (player.BaseAttackDamage + player.FlatPhysicalDamageMod);
            return dmg;
        }
    }
}
