namespace SpiritIsland;

/// <remarks>
/// Used for Growth & Blight
/// EXCEPT Cmd.PlacePresenceWithin( int range ) which is used for 2 Powers
/// </remarks>
public class PlacePresence : SpiritAction {

	#region constructors

	public PlacePresence() :base( "Place Presence" ) {
		Range = null;
		FilterEnums = DefaultFilters;
		FilterDescription = Filter.Any;
	}

	public PlacePresence(int range):base( $"Place Presence({range})" ) {
		Range = range;
		FilterEnums = DefaultFilters;
		FilterDescription = Filter.Any;
	}

	public PlacePresence( int range, params string[] filterEnums ) : base( $"Place Presence({range},{string.Join( "Or", filterEnums )})" ) {
		Range = range;
		FilterEnums = filterEnums;
		FilterDescription = string.Join( "Or", filterEnums );
	}

	#endregion constructors


	public override async Task ActAsync( Spirit self ) {
		// From
		TokenLocation from = await self.SelectSourcePresence();
		IToken token = from.Token;

		var toOptions = self.FindSpacesWithinRange( new TargetCriteriaFactory( Range ?? int.MaxValue, FilterEnums ).Bind( self ) )
			.Where( self.Presence.CanBePlacedOn )
			.ToArray();
		if(toOptions.Length == 0)
			return; // this can happen if Ocean is dragged way-inland and is no longer near an ocean or coast.
		Space to = await self.SelectAsync( A.SpaceDecision.ToPlacePresence( toOptions, Present.Always, from.Token ) );
		await from.MoveToAsync(to);
	}

	static readonly string[] DefaultFilters = [ Filter.Any ];

	public int? Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

}