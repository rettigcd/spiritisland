namespace SpiritIsland;

public class PlacePresence : GrowthActionFactory {

	#region constructors

	public PlacePresence( int range ) {
		targetCriteria = new TargetCriteria( range );
		FilterEnum = Target.Any;
		FilterEnums = new string[] { FilterEnum };
		Name = $"PlacePresence({range})";
	}

	public PlacePresence( int range, params string[] filterEnum ) {
		this.targetCriteria = new TargetCriteria( range, filterEnum );
		FilterEnum = filterEnum.Length == 0 ? Target.Any : string.Join("Or",filterEnum);
		FilterEnums = filterEnum;
		Name = $"PlacePresence({range},{FilterEnum})";
	}

	#endregion

	public int Range => targetCriteria.Range;
	public string FilterEnum { get; }
	public string[] FilterEnums { get; }

	public override string Name {get;}

	public override Task ActivateAsync( SelfCtx ctx ) => ctx.Presence.PlaceWithin( targetCriteria, TargetingPowerType.None );

	readonly TargetCriteria targetCriteria;

}
