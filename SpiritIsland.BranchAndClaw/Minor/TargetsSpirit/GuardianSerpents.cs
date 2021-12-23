﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GuardianSerpents {

		[MinorCard( "Guardian Serpents", 1, Element.Sun, Element.Moon, Element.Earth, Element.Animal )]
		[Fast]
		[AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// Add 1 beast in one of target spirits lands
			var spaceCtx = await ctx.OtherCtx.TargetLandWithPresence("Select land to add beast (+defend 4 for SS)");
			await spaceCtx.Beasts.Add(1);

			// if target spirit has a SS in that land, defend 4 there
			if( spaceCtx.IsSelfSacredSite )
				spaceCtx.Defend(4);
		}


	}
}
