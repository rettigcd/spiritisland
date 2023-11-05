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
		FilterEnums = DefaultFilters;
		FilterDescription = string.Join( "Or", FilterEnums );
	}

	public override async Task ActAsync( SelfCtx ctx ) {
		var self = ctx.Self;
		var targetCriteriaFactory = new TargetCriteriaFactory( Range, FilterEnums );
		IOption from = await self.SelectSourcePresence();
		IToken token = from is SpaceToken sp ? sp.Token : self.Presence.Token; // We could expose this as the Default Token
		bool isForPower = ActionScope.Current.Category == ActionCategory.Spirit_Power;
		var toOptions = self.FindSpacesWithinRange( targetCriteriaFactory.Bind( ctx.Self ), isForPower )
			.Where( self.Presence.CanBePlacedOn )
			.ToArray();
		if(toOptions.Length == 0)
			throw new InvalidOperationException( "There no places to place presence." );
		Space to = await self.Gateway.Decision( Select.ASpace.ToPlacePresence( toOptions, Present.Always, token ) );
		await self.Presence.Place( from, to );

		if(isForPower && from is Track track && track.Action != null)
			await track.Action.ActAsync( ctx );
	}

	static readonly string[] DefaultFilters = new string[] { Target.Any };

	#endregion

	public int Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

}
