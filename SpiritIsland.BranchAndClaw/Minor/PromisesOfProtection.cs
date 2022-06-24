namespace SpiritIsland.BranchAndClaw;

public class PromisesOfProtection {

	[MinorCard( "Promises of Protection", 0, Element.Sun, Element.Earth, Element.Animal )]
	[Fast]
	[FromSacredSite( 2 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		await ctx.GatherUpToNDahan( 2 );

		await ctx.AdjustTokensHealthForRound( 2, TokenType.Dahan );

	}

}
