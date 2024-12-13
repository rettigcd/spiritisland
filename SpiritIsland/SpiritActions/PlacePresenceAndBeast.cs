namespace SpiritIsland.JaggedEarth;

public class PlacePresenceAndBeast : SpiritAction {

	public PlacePresenceAndBeast():base( "Place Presence and Beast" ) { }

	public override async Task ActAsync( Spirit self ) {
		TokenLocation from = await self.SelectSourcePresence();

		var options = DefaultRangeCalculator.Singleton.GetTargetingRoute_MultiSpace( self.Presence.Lands, new TargetCriteria( 3 ) ).Targets;
		Space to = await self.SelectAsync( A.SpaceDecision.ToPlacePresence( options, Present.Always, self.Presence.Token ) );

		await from.MoveToAsync(to);
		await to.Beasts.AddAsync(1);
	}

}