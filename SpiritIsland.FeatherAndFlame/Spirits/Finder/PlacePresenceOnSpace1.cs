namespace SpiritIsland.FeatherAndFlame;

public class PlacePresenceOnSpace1 : SpiritAction {

	public PlacePresenceOnSpace1():base( "Setup_PlacePresenceOnSpace1" ) { }

	// Put 1 presence on any board in land #1.
	public override async Task ActAsync( SelfCtx ctx ) {
		var options = GameState.Current.Island.Boards.Select( b => b[1] );
		var space = await ctx.SelectAsync( A.Space.ToPlacePresence( options, Present.Always, ctx.Self.Presence.Token ) );
		await ctx.Self.Presence.Token.AddTo( space );
	}

}