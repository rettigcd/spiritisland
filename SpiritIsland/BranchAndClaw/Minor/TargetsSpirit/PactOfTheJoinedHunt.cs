using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class PactOfTheJoinedHunt {

		[MinorCard( "Pact of the Joined Hunt", 1, Speed.Slow, Element.Sun, Element.Plant, Element.Animal )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// Target spirit gathers 1 dahan into one of their lands
			var spaceCtx = await ctx.TargetSelectsPresenceLand("Gather 1 dahan to");
			await spaceCtx.GatherUpToNTokens(1,TokenType.Dahan);

			// 1 damage in that land per dahan present
			int dahanPresent = spaceCtx.Tokens.Sum(TokenType.Dahan);
			spaceCtx.Defend( dahanPresent );

		}

	}
}
