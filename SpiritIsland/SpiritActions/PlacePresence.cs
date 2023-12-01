namespace SpiritIsland;

public class PlacePresence : SpiritAction {

	#region constructors

	public PlacePresence(int range)
		:base( $"PlacePresence({range})" ) {
		Range = range;
		FilterEnums = DefaultFilters;
		FilterDescription = Target.Any;
	}

	public PlacePresence( int range, params string[] filterEnums )
		: base( $"PlacePresence({range},{string.Join( "Or", filterEnums )})" ) {
		Range = range;
		FilterEnums = filterEnums;
		FilterDescription = string.Join( "Or", filterEnums );
	}

	public override async Task ActAsync( SelfCtx ctx ) {
		var self = ctx.Self;
		// From
		IOption from = await self.SelectSourcePresence();
		IToken token = from is SpaceToken sp ? sp.Token : self.Presence.Token; // We could expose this as the Default Token

		var toOptions = self.FindSpacesWithinRange( new TargetCriteriaFactory( Range, FilterEnums ).Bind( ctx.Self ) )
			.Where( self.Presence.CanBePlacedOn )
			.ToArray();
		if(toOptions.Length == 0)
			throw new InvalidOperationException( "There are no places to place presence." );
		Space to = await self.Select( A.Space.ToPlacePresence( toOptions.Downgrade(), Present.Always, token ) );
		await self.Presence.PlaceAsync( from, to );

		if( ActionScope.Current.Category == ActionCategory.Spirit_Power 
			&& from is Track track 
			&& track.Action != null
		)
			await track.Action.ActAsync( ctx );
	}

	static readonly string[] DefaultFilters = new string[] { Target.Any };

	#endregion

	public int Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

}
