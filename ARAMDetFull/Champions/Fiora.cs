using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ARAMDetFull.Champions
{
    class Fiora : Champion
    {

        public static float QSkillshotRange = 400;
        public static float QCircleRadius = 350;

        public Fiora()
        {

            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                {
                    new ConditionalItem(ItemId.Mercurys_Treads, ItemId.Ninja_Tabi, ItemCondition.ENEMY_AP),
                    new ConditionalItem(ItemId.Ravenous_Hydra_Melee_Only),
                    new ConditionalItem(ItemId.The_Black_Cleaver),
                    new ConditionalItem(ItemId.The_Bloodthirster),
                    new ConditionalItem(ItemId.Spirit_Visage, (ItemId)3742, ItemCondition.ENEMY_AP),
                    new ConditionalItem((ItemId)3812),
                },
                startingItems = new List<ItemId>
                {
                    ItemId.Pickaxe,ItemId.Boots_of_Speed
                }
            };

        }

        public override void useQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || target == null)
                return;
            if (safeGap(target))
                Q.Cast(target);
        }

        public override void useW(Obj_AI_Base target)
        {
            if (!W.IsReady() || target == null)
                return;
            W.Cast(target);
        }

        public override void useE(Obj_AI_Base target)
        {
            if (!E.IsReady() || target == null)
                return;
            E.Cast();
            Aggresivity.addAgresiveMove(new AgresiveMove(100, 1000));
        }

        public override void useR(Obj_AI_Base target)
        {
            if (R.CanCast(target) && !Q.IsKillable(target))
            {
                R.CastOnUnit(target);
                Aggresivity.addAgresiveMove(new AgresiveMove(100,8000));
            }

        }

        public override void setUpSpells()
        {
            //Initialize our Spells
            Q = new Spell(SpellSlot.Q, QSkillshotRange + QCircleRadius);
            Q.SetSkillshot(.25f, 100, 500, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 750);
            W.SetSkillshot(0.5f, 70, 3200, false, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E);
            E.SetTargetted(0f, 0f);

            R = new Spell(SpellSlot.R, 500);
            R.SetTargetted(.066f, 500);
        }

        public override void useSpells()
        {
            var tar = ARAMTargetSelector.getBestTarget(W.Range);
            if (tar != null) useW(tar);
            tar = ARAMTargetSelector.getBestTarget(Q.Range);
            if (tar != null) useQ(tar);
            tar = ARAMTargetSelector.getBestTarget(350);
            if (tar != null) useE(tar);
            tar = ARAMTargetSelector.getBestTarget(R.Range);
            if (tar != null) useR(tar);
        }
        
    }
}
