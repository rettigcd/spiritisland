namespace SpiritIsland;

public class PlacePresence : SpiritAction {

	#region constructors

	public PlacePresence() :base( "PlacePresence" ) {
		Range = null;
		FilterEnums = DefaultFilters;
		FilterDescription = Filter.Any;
	}

	public PlacePresence(int range):base( $"PlacePresence({range})" ) {
		Range = range;
		FilterEnums = DefaultFilters;
		FilterDescription = Filter.Any;
	}

	public PlacePresence( int range, params string[] filterEnums ) : base( $"PlacePresence({range},{string.Join( "Or", filterEnums )})" ) {
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
		Space to = await self.SelectAsync( A.Space.ToPlacePresence( toOptions.Downgrade(), Present.Always, from.Token ) );
		await from.MoveToAsync(to);
	}

	static readonly string[] DefaultFilters = [ Filter.Any ];

	public int? Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

}