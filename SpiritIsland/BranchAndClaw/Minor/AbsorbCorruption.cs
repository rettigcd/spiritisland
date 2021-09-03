using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class AbsorbCorruption {

		[MinorCard( "Absorb Corruption", 1, Speed.Slow, Element.Sun, Element.Earth, Element.Plant )]
		[FromPresence( 0 )]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			bool remove1 = ctx.Tokens.Has(TokenType.Blight)
				&& ctx.Self.Energy>=1
				&& await ctx.Self.UserSelectsFirstText("Select power","remove blight for 1 engery","gather 1 blight");

			if(remove1)
				ctx.Tokens[TokenType.Blight]--;
			else
				await ctx.GatherUpToNTokens(1,TokenType.Blight.Generic);
		}
	}
}
