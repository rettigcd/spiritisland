using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GuardianSerpents {

		[MinorCard( "Guardian Serpents", 1, Speed.Fast, Element.Sun, Element.Moon, Element.Earth, Element.Animal )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// Add 1 beast in one of target spirits lands
			var space = await ctx.TargetSelectsPresenceLand("Select land to add beast (+defend 4 for SS)");
			ctx.GameState.Tokens[space][BacTokens.Beast]++;

			// if target spirit has a SS in that land, defend 4 there
			if(ctx.Target.SacredSites.Contains(space))
				ctx.GameState.Defend(space,4);
		}


	}
}
