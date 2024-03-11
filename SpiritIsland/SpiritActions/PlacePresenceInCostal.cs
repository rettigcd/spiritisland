namespace SpiritIsland.Basegame;

public class PlacePresenceInCostal : SpiritAction {

	public PlacePresenceInCostal():base( "Place Presence in Costal" ) { }

	// ! Can't used normal PlacePresence, because it must be range-1, range 0 not allowed.
	public override async Task ActAsync( Spirit self ) {
		IEnumerable<SpaceState> options = self.Presence.Lands.First().Adjacent_Existing;
		var space = await self.SelectAsync( A.Space.ToPlacePresence( options, Present.Always, self.Presence.Token ) );
		await self.Presence.Token.AddTo( space.ScopeTokens );
	}

}