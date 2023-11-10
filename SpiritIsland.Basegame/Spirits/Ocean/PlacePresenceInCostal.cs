namespace SpiritIsland.Basegame;

public class PlacePresenceInCostal : SpiritAction {

	public PlacePresenceInCostal():base( "Place Presence in Costal" ) { }

	// ! Can't used normal PlacePresence, because it must be range-1, range 0 not allowed.
	public override async Task ActAsync( SelfCtx ctx ) {
		var options = ctx.Self.Presence.Spaces.First().Adjacent_Existing;
		var space = await ctx.SelectAsync( A.Space.ToPlacePresence( options, Present.Always, ctx.Self.Presence.Token ) );
		await ctx.Self.Presence.Token.AddTo( space );
	}

}