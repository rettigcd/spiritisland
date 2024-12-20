namespace SpiritIsland.JaggedEarth;

public class PlacePresenceAndBeast : SpiritAction {

	public PlacePresenceAndBeast():base( "Place Presence and Beast" ) { }

	public override async Task ActAsync( Spirit self ) {
		// Range 3
		Space[] toOptions = DefaultRangeCalculator.Singleton.GetTargetingRoute_MultiSpace(self.Presence.Lands, new TargetCriteria(3)).Targets;
		var move = await self.SelectAlways(Prompts.SelectPresenceTo(), self.DeployablePresence().BuildMoves(_ => toOptions).ToArray());
		await move.Apply();

		// Add beast
		await ((Space)move.Destination).Beasts.AddAsync(1);
	}

}