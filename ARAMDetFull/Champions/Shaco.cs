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
	class Shaco : Champion
	{

	    private Obj_AI_Base clone;

		public Shaco()
		{
			ARAMSimulator.champBuild = new Build
			{
				coreItems = new List<ConditionalItem>
						{
							new ConditionalItem(ItemId.Morellonomicon),
							new ConditionalItem(ItemId.Ludens_Echo),
							new ConditionalItem(ItemId.Sorcerers_Shoes),
							new ConditionalItem(ItemId.Lich_Bane),
							new ConditionalItem(ItemId.Rabadons_Deathcap),
							new ConditionalItem(ItemId.Void_Staff),
						},
				startingItems = new List<ItemId>
						{
							ItemId.Needlessly_Large_Rod
						}
			};

		    GameObject.OnCreate += (sender, args) =>
		    {
                var objBase = sender as Obj_AI_Base;
		        if (objBase != null && !objBase.IsMe && objBase.Name == player.Name && objBase.IsAlly)
		        {
		            clone = objBase;
		        }
		    };

		    GameObject.OnDelete += (sender, args) =>
		    {
		        if (clone == null)
		            return;
		        if (sender.NetworkId == clone.NetworkId)
		            clone = null;
		    };

		}

		public override void useQ(Obj_AI_Base target)
		{
			if (!Q.IsReady())
				return;
		    if (safeGap(target))
		    {
		        Q.Cast(target.Position);
                Aggresivity.addAgresiveMove(new AgresiveMove(90,3000));
		    }
		    else if (EnemyInRange(2, 500) && player.HealthPercent < 65)
		        Q.Cast(player.Position.To2D().Extend(ARAMSimulator.fromNex.Position.To2D(), 400));
		}

		public override void useW(Obj_AI_Base target)
		{
			if (!W.IsReady())
				return;
			W.Cast(target.Position);
		}

		public override void useE(Obj_AI_Base target)
		{
			if (!E.IsReady())
				return;
			E.CastOnUnit(target);

		}

		public override void useR(Obj_AI_Base target)
		{
			if (!R.IsReady())
				return;
			R.Cast();
		}

		public override void setUpSpells()
		{
			Q = new Spell(SpellSlot.Q, 600);
			W = new Spell(SpellSlot.W, 525);
			E = new Spell(SpellSlot.E, 625);
			R = new Spell(SpellSlot.R, 0);
		}

		public override void useSpells()
		{
			var tar = ARAMTargetSelector.getBestTarget(Q.Range);
			if (tar != null) useQ(tar);
			tar = ARAMTargetSelector.getBestTarget(E.Range);
			if (tar != null) useE(tar);
			tar = ARAMTargetSelector.getBestTarget(W.Range+500);
			if (tar != null) useW(tar);
			tar = ARAMTargetSelector.getBestTarget(300);
			if (tar != null) useR(tar);

		    if (clone == null || clone.IsDead)
		        return;
            tar = ARAMTargetSelector.getBestTarget(1500);
            if (clone.Distance(tar.Position) > 200)
            {
                player.IssueOrder(GameObjectOrder.MovePet, tar);
            }
            else
            {
                clone.IssueOrder(GameObjectOrder.AttackUnit, tar);
            }
        }

		public static bool EnemyInRange(int numOfEnemy, float range)
		{
			return Utility.CountEnemysInRange(ObjectManager.Player, (int)range) >= numOfEnemy;
		}

	}
}
