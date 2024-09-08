namespace SpiritIsland.BranchAndClaw;

public class PromisesOfProtection {

	[MinorCard( "Promises of Protection", 0, Element.Sun, Element.Earth, Element.Animal ),Fast,FromSacredSite( 2 )]
	[Instructions( "Gather up to 2 Dahan. Dahan have +2 Health while in target land." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		await ctx.GatherUpToNDahan( 2 );

		await ctx.Space.AdjustTokensHealthForRound( 2, Human.Dahan );

	}

}
