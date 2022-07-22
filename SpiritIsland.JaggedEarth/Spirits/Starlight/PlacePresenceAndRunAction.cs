namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Allows Starlights Revealed Growth Options to be selected and available immediately
/// </summary>
class PlacePresenceAndRunAction : PlacePresence {
	public PlacePresenceAndRunAction(int range):base(range) { }
	public override async Task ActivateAsync( SelfCtx ctx ) {
		var (from,_) = await ctx.Presence.PlaceWithin( Range, FilterEnum );
		if( from is Track track && track.Action != null )
			await track.Action.ActivateAsync( ctx );
	}
}