namespace SpiritIsland.JaggedEarth;

public class PlacePresenceAndBeast : SpiritAction {

	public PlacePresenceAndBeast():base( "PlacePresenceAndBeast" ) { }

	public override async Task ActAsync( Spirit self ) {
		TokenLocation from = await self.SelectSourcePresence();

		var options = DefaultRangeCalculator.Singleton.GetSpaceOptions( self.Presence.Lands.Tokens(), new TargetCriteria( 3 ) );
		Space to = await self.SelectAsync( A.Space.ToPlacePresence( options.Downgrade(), Present.Always, self.Presence.Token ) );

		await from.MoveToAsync(to);
		await to.Tokens.Beasts.AddAsync(1);
	}

}