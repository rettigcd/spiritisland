namespace SpiritIsland.BranchAndClaw;

public class GrowthThroughSacrifice {

	public const string Name = "Growth Through Sacrifice";

	[MinorCard(GrowthThroughSacrifice.Name,0,Element.Moon,Element.Fire,Element.Water,Element.Plant), Fast, AnySpirit]
	[Instructions( "Destroy 1 of your Presence. Target Spirit chooses to either: Remove 1 Blight from one of their lands. -or- Add 1 Presence to one of their lands. -If you have- 2 Sun: They may do both, in the same land." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {

		// destroy one of your presence
		await Cmd.DestroyPresence().ActAsync(ctx.Self);

		// If 2 sun, do both in the same land
		await TargetSpiritAction( ctx.Other, await ctx.YouHave( "2 sun" ) );

	}

	static async Task TargetSpiritAction( Spirit other, bool doBoth ) {

		// Note - not strictly following rules - altering to allow presence in any spot that has presence.
		// Presence placed in an illegal land will allow adding more there, although it technically shouldn't.
		string joinStr = doBoth ? "AND" : "OR";
		var space = (await other.SelectLandWithPresence( $"Select location to Remove Blight {joinStr} Add Presence" ))!;

		var spaceCtx = other.Target(space);

		var removeBlight = new SpaceAction( "Remove 1 blight from one of your lands", spaceCtx => spaceCtx.RemoveBlight() );
		var addPresence = new SpaceAction( "Add 1 presence to one of your lands", spaceCtx => spaceCtx.Presence.PlaceHere() )
			.OnlyExecuteIf( x=>x.Self.Presence.CanBePlacedOn(x.Space) );

		if(!doBoth)
			await spaceCtx.SelectActionOption( removeBlight, addPresence );
		else {
			await removeBlight.ActAsync(spaceCtx);
			if(addPresence.IsApplicable(spaceCtx))
				await addPresence.ActAsync(spaceCtx);
		}

	}

}