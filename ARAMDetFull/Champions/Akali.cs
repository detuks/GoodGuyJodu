using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK.Utils;

namespace ARAMDetFull.Champions
{
    class Akali : Champion
    {
        public Akali()
        {

            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Hextech_Gunblade),
                            new ConditionalItem(ItemId.Sorcerers_Shoes,ItemId.Mercurys_Treads,ItemCondition.ENEMY_LOSING),
                            new ConditionalItem(ItemId.Abyssal_Scepter,ItemId.Zhonyas_Hourglass,ItemCondition.ENEMY_AP),
                            new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                            new ConditionalItem(ItemId.Lich_Bane),
                            new ConditionalItem(ItemId.Rabadons_Deathcap),
                        },
                startingItems = new List<ItemId>
                        {
                            ItemId.Hextech_Revolver
                        }
            };
        }

        public override void useQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || target == null)
                return;
            if(Q.Cast(target) == Spell.CastStates.SuccessfullyCasted && target.InAutoAttackRange())
                Aggresivity.addAgresiveMove(new AgresiveMove(50, 1000, true));

        }

        public override void useW(Obj_AI_Base target)
        {
            if (!W.IsReady())
                return;
            if(player.CountEnemiesInRange(500)>1)
                W.Cast(player.Position);
        }

        public override void useE(Obj_AI_Base target)
        {
            if (!E.IsReady() || target == null)
                return;
            E.Cast();
        }


        public override void useR(Obj_AI_Base target)
        {
            if (!R.IsReady() || target == null)
                return;
            if (safeGap(target) || GetComboDamage(target)*0.8f > target.Health)
            {
                R.Cast(target);
                Aggresivity.addAgresiveMove(new AgresiveMove(50,2500,true));
            }
        }

        public override void useSpells()
        {
            var tar = ARAMTargetSelector.getBestTarget(Q.Range);
            if (tar != null) useQ(tar);
            tar = ARAMTargetSelector.getBestTarget(W.Range);
            if (tar != null)  useW(tar);
            tar = ARAMTargetSelector.getBestTarget(E.Range);
            if (tar != null) useE(tar);
            tar = ARAMTargetSelector.getBestTarget(R.Range);
            if (tar != null) useR(tar);
        }

        public override void killSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (hero.Distance(ObjectManager.Player) <= R.Range &&
                    player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health)
                        R.CastOnUnit(hero, true);
            }
        }

        private float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;
            float manaCost = 0;
            if (Q.IsReady() && Q.ManaCost<=player.Mana)
            {
                fComboDamage += player.GetSpellDamage(vTarget, SpellSlot.Q) +
                                player.GetSpellDamage(vTarget, SpellSlot.Q, 1);
                manaCost += Q.ManaCost - 20;
            }

            if (E.IsReady() && E.ManaCost+ manaCost <= player.Mana)
            {
                fComboDamage += player.GetSpellDamage(vTarget, SpellSlot.E);
                manaCost += E.ManaCost - 5;
            }

            if (R.IsReady())
            {
                fComboDamage += player.GetSpellDamage(vTarget, SpellSlot.R)*R.Instance.Ammo;
            }


            return (float)fComboDamage;
        }

        public override void setUpSpells()
        {
            //Create the spells
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 290f);
            R = new Spell(SpellSlot.R, 800f);
        }
    }
}
