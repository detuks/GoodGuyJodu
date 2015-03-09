using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    class FightSimulator
    {
        private Jungler heroFigher;

        public float hpLeftAfter = 0;
        public float timeToKill = 0;

        public FightSimulator(Jungler hero)
        {
            heroFigher = hero;
        }

      /*  private void Fight(Camp campFighting)
        {
            Camp.JungleMinion minHpMin = campFighting.Minions.OrderBy(cp => cp.UpdatedStats.health).First();
            Camp.JungleMinion maxHpMin = campFighting.Minions.OrderByDescending(cp => cp.UpdatedStats.health).First();

            Console.WriteLine("Minion count: " + maxHpMin.UpdatedStats.health);

            float heroAoeDps = heroFigher.getSkillAoePerSec();
            float heroDps = heroFigher.getDPS(campFighting);

            Console.WriteLine("Hero dps:" + heroDps);

            float timeToKIllMin = heroFigher.getTimeToDoDmgAoe(minHpMin, minHpMin.UpdatedStats.health, (campFighting.timeToCamp - 3) < 0 ? 0 : campFighting.timeToCamp - 3);
            float timeToKIllMax = heroFigher.getTimeToDoDmg(maxHpMin, maxHpMin.UpdatedStats.health, (campFighting.timeToCamp - 3) < 0 ? 0 : campFighting.timeToCamp - 3);
          
            float fullCampDps = campFighting.Minions.Sum(min => min.getDps());

            float fullDmgDoneToMe = 0;

            float timeToFinish = 0;

            //Much aoe
            if (timeToKIllMin < timeToKIllMax)
            {
                Console.WriteLine("Aeo better");
                fullDmgDoneToMe += timeToKIllMin*fullCampDps;
                float restTimeToKill = timeToKIllMax - timeToKIllMin;
                fullDmgDoneToMe += restTimeToKill * maxHpMin.getDps() * heroFigher.getKrugBuffDmgRemove(campFighting.timeToCamp, timeToKIllMax);
                timeToFinish += timeToKIllMin+restTimeToKill;
            }
            else
            {
                Console.WriteLine("Single dps better: " + maxHpMin.UpdatedStats.health);
                fullDmgDoneToMe += timeToKIllMax * fullCampDps;
                float dmgDoneAor = timeToKIllMax*heroAoeDps;
                float leftMin = campFighting.Minions.Count - 1;
                float leftMinHp = leftMin * (minHpMin.UpdatedStats.health - dmgDoneAor);

                float tmeToFinish = leftMinHp/(heroAoeDps + heroDps);
                timeToFinish = tmeToFinish + timeToKIllMax;
                fullDmgDoneToMe += minHpMin.getDps()*2;
            }

            Console.WriteLine("TimeTOfinish: " + timeToFinish);
            float myHpTillThere = heroFigher.getFulHeal((campFighting.timeToCamp - 3) < 0 ? 0 : campFighting.timeToCamp - 3, timeToFinish) + Jungler.player.Health;

            float relDmgToMe = heroFigher.realPhysDmgDoneToMe(fullDmgDoneToMe);

            Console.WriteLine("HpLeft: " + (myHpTillThere-relDmgToMe));

        }*/

        public bool AroundFight(Camp campFighting, bool smite=false,bool aoeSmite = false, bool smiteIfNeedTo = false)
        {
            Camp.JungleMinion minHpMin = campFighting.Minions.OrderBy(cp => cp.UpdatedStats.health).First();
            Camp.JungleMinion maxHpMin = campFighting.Minions.OrderByDescending(cp => cp.UpdatedStats.health).First();
            smite = getSmiteCd(campFighting.timeToCamp) == 0 && smite;
           // Console.WriteLine("Smite: " + smite + " dmg: " + heroFigher.getSmiteDmg());
            float maxMinHp = maxHpMin.UpdatedStats.health - ((smite) ? (heroFigher.getSmiteDmg() / 2) : 0); ;
            float minMinHp = minHpMin.UpdatedStats.health - ((smite && aoeSmite)?(heroFigher.getSmiteDmg()/2):0);

            //Console.WriteLine("Fighting: " + campFighting.ToString());

            float timeToKillMin = heroFigher.getTimeToDoDmgAoe(minHpMin, minMinHp, (campFighting.timeToCamp - 3) < 0 ? 0 : campFighting.timeToCamp - 3);
            float timeToKillMax = heroFigher.getTimeToDoDmg(maxHpMin, maxMinHp, (campFighting.timeToCamp - 3) < 0 ? 0 : campFighting.timeToCamp - 3);
            
            float fullCampDps = campFighting.Minions.Sum(min => min.getDps());

            float fullDmgDoneToMe = 0;

            float timeToKillCamp = 0;

           // Console.WriteLine("timeToKillMin " + timeToKillMin);
            //Console.WriteLine("timeToKillMax " + timeToKillMax);


            if (campFighting.aliveMinCount() == 1)
            {
                timeToKillCamp = timeToKillMax;
                fullDmgDoneToMe = maxHpMin.getDps()*
                                  heroFigher.getKrugBuffDmgRemove(campFighting.timeToCamp, timeToKillMax);
            }
            else
            {
                //Good aoe
                if (timeToKillMin < timeToKillMax)
                {
                   // Console.WriteLine("Aeo better: " + timeToKillMin);
                    float restTimeToKill = timeToKillMax - timeToKillMin;
                    fullDmgDoneToMe += fullCampDps*timeToKillMin;

                    fullDmgDoneToMe += restTimeToKill * maxHpMin.getDps() * heroFigher.getKrugBuffDmgRemove(campFighting.timeToCamp, timeToKillMax);
                    timeToKillCamp = restTimeToKill + timeToKillMin;
                }
                else
                {
                    fullDmgDoneToMe += timeToKillMax * fullCampDps;
                    float dmgDoneAor = heroFigher.getAoeDmgDoneInTime(minHpMin, timeToKillMax, campFighting.timeToCamp) + heroFigher.getFrogBuffAoe(campFighting.timeToCamp, timeToKillMin - timeToKillMax);
                    float leftMin = campFighting.Minions.Count - 1;
                    float leftMinHp = leftMin * (minMinHp - dmgDoneAor);

                    float tmeToFinish = heroFigher.getTimeToDoDmg(minHpMin,leftMinHp,6);
                    timeToKillCamp = tmeToFinish + timeToKillMax;
                    fullDmgDoneToMe += minHpMin.getDps() * tmeToFinish;
                }

            }
           // Console.WriteLine("TimeTOfinish: " + timeToKillCamp);
            float myHpTillThere = heroFigher.getFulHeal((campFighting.timeToCamp - 3) < 0 ? 0 : campFighting.timeToCamp - 3, timeToKillCamp) ;

            float relDmgToMe = heroFigher.realPhysDmgDoneToMe(fullDmgDoneToMe);

           // Console.WriteLine("HpLeft: " + (myHpTillThere + Jungler.player.Health - relDmgToMe) + " will heal:" + myHpTillThere);

            hpLeftAfter = (myHpTillThere + Jungler.player.Health  - relDmgToMe);
            timeToKill = timeToKillCamp;

            if (smiteIfNeedTo && hpLeftAfter <= 100 && getSmiteCd(campFighting.timeToCamp)==0)
            {
                AroundFight(campFighting, true, aoeSmite);
                campFighting.forceSmite = true;
              //  Console.WriteLine("force smite!!!!");
            }
            //Update camp
            campFighting.willKillMe = hpLeftAfter < 250;
            campFighting.hpLeftAfterFight = hpLeftAfter;
            campFighting.timeToKill = timeToKill;

            return hpLeftAfter > 250;
        }

        public float getSmiteCd(float inTime =0)
        {
            float cd = Jungler.player.Spellbook.GetSpell(JungleClearer.jungler.smite).CooldownExpires - Game.Time - inTime;
            return (cd < 0) ? 0 : cd;
        }
    }
}
