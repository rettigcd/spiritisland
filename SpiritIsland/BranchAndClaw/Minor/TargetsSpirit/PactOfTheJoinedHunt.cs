using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class PactOfTheJoinedHunt {

		[MinorCard( "Pact of the Joined Hunt", 1, Speed.Slow, Element.Sun, Element.Plant, Element.Animal )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// Target spirit gathers 1 dahan into one of their lands
			var space = await ctx.TargetSelectsPresenceLand("Gather 1 dahan to");
			await ctx.Target.MakeDecisionsFor(ctx.GameState).GatherUpToNTokens(space,1,TokenType.Dahan);

			// 1 damage in that land per dahan present
			int dahanPresent = ctx.GameState.Tokens[space].Sum(TokenType.Dahan);
			ctx.GameState.Defend(space,dahanPresent);

		}

	}
}
