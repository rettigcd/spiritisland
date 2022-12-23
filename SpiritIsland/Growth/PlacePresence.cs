namespace SpiritIsland;

public class PlacePresence : GrowthActionFactory {

	#region constructors

	public PlacePresence( int range ) {
		Range = range;
		FilterEnums = DefaultFilters;
		FilterDescription = Target.Any;
		Name = $"PlacePresence({range})";
	}

	static readonly string[] DefaultFilters = new string[] { Target.Any };

	public PlacePresence( int range, params string[] filterEnum ) {
		Range = range;
		FilterEnums = filterEnum;
		FilterDescription = string.Join( "Or", FilterEnums );
		Name = $"PlacePresence({range},{FilterDescription})";
	}

	#endregion

	public int Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

	public override string Name {get;}

	public override Task ActivateAsync( SelfCtx ctx ) => ctx.Presence.PlaceWithin( 
		ctx.TerrainMapper.Specify( Range, FilterEnums ), 
		TargetingPowerType.None
	);

}
