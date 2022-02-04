namespace SpiritIsland;

public class PlacePresence : GrowthActionFactory {

	public int Range { get; }
	public string FilterEnum { get; }

	public override string Name {get;}

	#region constructors

	public PlacePresence( int range ){
		this.Range = range;
		FilterEnum = Target.Any;
		Name = $"PlacePresence({range})";
	}

	public PlacePresence(
		int range,
		string filterEnum
	) {
		this.Range = range;
		this.FilterEnum = filterEnum;
		Name = $"PlacePresence({range},{filterEnum})";
	}

	#endregion

	public override Task ActivateAsync( SelfCtx ctx ) => ctx.Presence.PlaceWithin( Range, FilterEnum );

}
