namespace SpiritIsland.FeatherAndFlame;

public class PlacePresenceOnSpace1 : SpiritAction {

	public PlacePresenceOnSpace1():base( "Setup_PlacePresenceOnSpace1" ) { }

	// Put 1 presence on any board in land #1.
	public override async Task ActAsync( Spirit self ) {
		IEnumerable<Space> options = GameState.Current.Island.Boards.Select( b => b[1].ScopeSpace );
		Space? space = await self.SelectAsync( A.SpaceDecision.ToPlacePresence( options, Present.Always, self.Presence.Token ) );
		if(space is not null)
			await self.Presence.Token.AddTo( space );
	}

}