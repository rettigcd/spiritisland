﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class BloodwrackPlague {

		[MajorCard("Bloodwrack Plague",4,Element.Water,Element.Earth,Element.Animal)]
		[Fast]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// add 2 disease
			var disease = ctx.Disease;
			disease.Add( 2 );

			// for each disease in target land, defend 1 in target and all adjacent lands
			ctx.Defend( disease.Count );
			foreach( var adjacent in ctx.Adjacent )
				ctx.Target(adjacent).Defend( disease.Count );

			// if you have 2 earthn 4 animal:
			if(await ctx.YouHave("2 earth,4 animal")) { 
				// 2 fear.
				ctx.AddFear(2);
				// For each disease in target land, do 1 damage in target or adjacent land
				int damage = disease.Count;
				var space = await ctx.Self.Action.Decision(new Decision.TargetSpace($"Select space to apply {damage} damage", ctx.Space.Range(1), Present.Always )); // can we wrap this and make it easier to call?
				await ctx.Target(space).DamageInvaders( damage );
			}
		}

	}

}
