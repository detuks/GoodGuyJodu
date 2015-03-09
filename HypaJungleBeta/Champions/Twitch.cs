using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    class Twitch : Jungler
    {

        public bool usingsafe = false;

        public Twitch()
        {
            setUpSpells();
            setUpItems();
            levelUpSeq = new Spell[] { Q, E, E, W, Q, R, Q, Q, Q, E, R, E, E, W, W, R, W };
            dragOnLvl = 7;
        }

        public override void setUpSpells()
        {
            recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 0);
            setupSafes();
        }

        public void setupSafes()
        {
            foreach (var camp in HypaJungle.jTimer.jungleCamps)
            {
                if (camp.campId == 1)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(3511.601f, 8745.617f, 52.57141f);
                }

                if (camp.campId == 2)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(3144.897f, 7106.449f, 51.89026f);
                }

                if (camp.campId == 3)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(7760.018f, 5050.575f, 49.57141f);
                }

                if (camp.campId == 4)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(7461.018f, 3253.575f, 52.57141f);
                }

                if (camp.campId == 5)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(7462.053f, 2489.813f, 52.57141f);
                }

                if (camp.campId == 6)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(10978.053f, 5454.813f, -69.57141f);
                }

                if (camp.campId == 7)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(11417.6f, 6216.028f, 51.00244f);
                }

                if (camp.campId == 8)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(11844.77f, 7682.083f, 52.72742f);
                }

                if (camp.campId == 9)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(6881.741f, 9895.717f, 54.02466f);
                }

                if (camp.campId == 10)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(7326.056f, 11643.01f, 50.21985f);
                }

                if (camp.campId == 11)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(7368.408f, 12488.37f, 56.47668f);
                }

                if (camp.campId == 14)
                {
                    camp.useSafe = true;
                    camp.safePosition = new Vector3(12104.408f, 6680.37f, 51.47668f);
                }
            }
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
                    itemIds = new List<int>{1039,2003,2003,2003,3340}
                },
                new ItemToShop()
                {
                    goldReach = 675,
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
           
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.IsValidTarget(E.Range))
            {
                foreach (var buff in minion.Buffs.Where(buff => buff.DisplayName.ToLower() == "twitchdeadlyvenom").Where(buff => buff.Count == 6))
                {
                    E.Cast();
                }
            }
        }

        public override void UseR(Obj_AI_Minion minion)
        {

        }

        public void shouldUseSafe()
        {
            if (!usingsafe  && player.Health / player.MaxHealth < 0.5f && JungleClearer.focusedCamp.useSafe)
                usingsafe = true;
            else  if (usingsafe  && player.Health / player.MaxHealth > 0.7f && JungleClearer.focusedCamp.useSafe)
                usingsafe = false;
        }

        public override void attackMinion(Obj_AI_Minion minion, bool onlyAA)
        {
            if (JungleOrbwalker.CanAttack())
            {
                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
           // shouldUseSafe();

            if (JungleClearer.focusedCamp.useSafe)
                JungleOrbwalker.attackMinion(minion, JungleClearer.focusedCamp.safePosition);
            else
                JungleOrbwalker.attackMinion(minion, minion.Position.To2D().Extend(player.Position.To2D(), 100).To3D());
        }

        public override void castWhenNear(Camp camp)
        {
            if (Q.IsReady() && camp.campPosition.Distance(player.Position, true) < 1200*1200)
            {
                Q.Cast();
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
            dps += (float)player.GetAutoAttackDamage(minion) * player.AttackSpeedMod;
            dps += 30;
            dpsFix = dps;
            return dps;
        }

        public override bool canMove()
        {
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
            return 0.1f;
        }

        public override float getAoeDmgDoneInTime(Camp.JungleMinion camp, float time, float cdResetTime)
        {
            return 0.1f;
        }

        public override float getTimeToDoDmgAoe(Camp.JungleMinion camp, float damageToDo, float cdResetTime)
        {
            var bufDmg = getFrogBuffAoe(cdResetTime, 1);
            if (bufDmg == 0)
                return 0.1f;
            return damageToDo / bufDmg;
        }

        public override float getTimeToDoDmg(Camp.JungleMinion camp, float damageToDo, float cdResetTime)
        {

            float damage = 0;
            //Qdmg can deal
            var qDmg = camp.UpdatedStats.physicGoesThrough * (getSpellDmgRaw(SpellSlot.Q));
            var tillNext = ((Qdata.Cooldown == 0) ? 10 : Qdata.Cooldown);
            var qDps = qDmg / tillNext;
            // Console.WriteLine(qDmg);

            float aaDps = camp.UpdatedStats.physicGoesThrough * (player.BaseAttackDamage + player.FlatPhysicalDamageMod) * getAAperSecond();
            float timeSkip = 0;
            if (Q.IsReady((int)(cdResetTime * 1000)))
            {
                damage += qDmg;
                timeSkip += tillNext;
            }

            damage += (player.BaseAttackDamage + player.FlatPhysicalDamageMod);
            if (damage >= damageToDo)
                return 1;

            float time = (damageToDo - damage) / (aaDps + getItemPassiveBoostDps());

            float timeWithRed = (damageToDo - damage + timeSkip * qDps) /
                                (aaDps + qDps + getItemPassiveBoostDps() + getRedBuffDmg(cdResetTime, time));

            return timeWithRed;

        }

        public override float getAAperSecond()
        {
            return 1 / player.AttackDelay;
        }

        public static int venomCount(Obj_AI_Base targ)
        {
            var buff = targ.Buffs.FirstOrDefault(b => b.DisplayName.ToLower() == "twitchdeadlyvenom");
            return buff == null ? 0 : buff.Count;
        }
    }
}
