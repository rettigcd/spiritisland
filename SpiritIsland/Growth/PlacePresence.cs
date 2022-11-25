namespace SpiritIsland;

public class PlacePresence : GrowthActionFactory {

	#region constructors

	public PlacePresence( int range ) {
		targetCriteria = new TargetCriteria( range );
		Name = $"PlacePresence({range})";
	}

	public PlacePresence( int range, string filterEnum ) {
		this.targetCriteria = new TargetCriteria( range, filterEnum );
		Name = $"PlacePresence({range},{filterEnum})";
	}

	#endregion

	public int Range => targetCriteria.Range;
	public string FilterEnum => targetCriteria.Filter;

	public override string Name {get;}

	public override Task ActivateAsync( SelfCtx ctx ) => ctx.Presence.PlaceWithin( targetCriteria, TargetingPowerType.None );

	readonly TargetCriteria targetCriteria;

}
