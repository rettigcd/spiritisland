namespace SpiritIsland.BranchAndClaw;

public class GrowthThroughSacrifice {

	public const string Name = "Growth Through Sacrifice";

	[MinorCard(GrowthThroughSacrifice.Name,0,Element.Moon,Element.Fire,Element.Water,Element.Plant), Fast, AnySpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// destroy one of your presence
		await ctx.Presence.DestroyOneFromAnywhere( DestoryPresenceCause.SpiritPower );

		// If 2 sun, do both in the same land
		await TargetSpiritAction( ctx.OtherCtx, await ctx.YouHave( "2 sun" ) );

	}

	static async Task TargetSpiritAction( SelfCtx ctx, bool doBoth ) {

		// Note - not strictly following rules - altering to allow presence in any spot that has presence.
		// Presence placed in an illegal land will allow adding more there, although it technically shouldn't.
		string joinStr = doBoth ? "AND" : "OR";
		var spaceCtx = await ctx.TargetLandWithPresence( $"Select location to Remove Blight {joinStr} Add Presence" );

		var removeBlight = new SpaceAction( "Remove 1 blight from one of your lands", spaceCtx => spaceCtx.RemoveBlight() );
		var addPresence = new SpaceAction( "Add 1 presence to one of your lands", spaceCtx => spaceCtx.Presence.PlaceHere() );

		if(!doBoth)
			await spaceCtx.SelectActionOption( removeBlight, addPresence );
		else {
			await removeBlight.Execute(spaceCtx);
			await addPresence.Execute(spaceCtx);
		}

	}

}