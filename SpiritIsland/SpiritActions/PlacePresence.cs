namespace SpiritIsland;

/// <remarks>
/// Used for Growth & Blight
/// EXCEPT Cmd.PlacePresenceWithin( int range ) which is used for 2 Powers
/// </remarks>
public sealed class PlacePresence : SpiritAction {

	public int? Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

	// Hook for Madness aspect && Sun-Bright Whilwind growth
	// Alternative: place a IHandleTokensPlaced mod in the Spirits Mod Bucket and call it.
	public AsyncEvent<TokenMovedArgs> Placed = new();

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
		Space[] toOptions = GetDestinationOptions(self);
		if( toOptions.Length == 0 )
			return; // this can happen if Ocean is dragged way-inland and is no longer near an ocean or coast.

		var move = await self.SelectAlways(Prompts.SelectPresenceTo(), self.DeployablePresence().BuildMoves(_ => toOptions).ToArray());
		var result = await move.Apply();

		await Placed.InvokeAsync(result!);
	}

	Space[] GetDestinationOptions(Spirit self) {
		TargetCriteria criteria = new TargetCriteriaFactory(Range ?? int.MaxValue, FilterEnums).Bind(self);
		var toOptions = self.FindSpacesWithinRange(criteria)
			.Where(self.Presence.CanBePlacedOn)
			.ToArray();
		return toOptions;
	}

	static readonly string[] DefaultFilters = [ Filter.Any ];

}