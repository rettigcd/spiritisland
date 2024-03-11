namespace SpiritIsland.JaggedEarth;

public class PlacePresenceAndBeast : SpiritAction {

	public PlacePresenceAndBeast():base( "Place Presence and Beast" ) { }

	public override async Task ActAsync( Spirit self ) {
		TokenLocation from = await self.SelectSourcePresence();

		var options = DefaultRangeCalculator.Singleton.GetSpaceOptions( self.Presence.Lands, new TargetCriteria( 3 ) );
		Space to = await self.SelectAsync( A.SpaceDecision.ToPlacePresence( options, Present.Always, self.Presence.Token ) );

		await from.MoveToAsync(to);
		await to.Beasts.AddAsync(1);
	}

}