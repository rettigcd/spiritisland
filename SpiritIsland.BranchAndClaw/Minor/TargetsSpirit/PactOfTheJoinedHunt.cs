namespace SpiritIsland.BranchAndClaw;
public class PactOfTheJoinedHunt {

	[MinorCard( "Pact of the Joined Hunt", 1, Element.Sun, Element.Plant, Element.Animal ),Slow,AnySpirit]
	[Instructions( "Target Spirit Gathers 1 Dahan into one of their lands. 1 Damage in that land per Dahan present." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		// Target spirit gathers 1 dahan into one of their lands
		Space space = await ctx.Other.SelectAlways("Gather 1 dahan to", ctx.Other.Presence.Lands);
		await DoSelectedLandStuff( ctx.Other.Target( space ) );
	}

	static async Task DoSelectedLandStuff( TargetSpaceCtx ctx ) {
		// Target spirit gathers 1 dahan into one of their lands
		await ctx.GatherUpToNDahan( 1 );

		// 1 damage in that land per dahan present
		await ctx.DamageInvaders( ctx.Dahan.CountAll );
	}
}