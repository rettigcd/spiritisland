namespace SpiritIsland.NatureIncarnate;

public class PlaceDestroyedPresence : SpiritAction {

	#region constructors

	public PlaceDestroyedPresence( int range )
		:base( $"PlaceDestroyedPresence({range})" )
	{
		Range = range;
		FilterEnums = DefaultFilters;
		FilterDescription = Target.Any;
	}

	static readonly string[] DefaultFilters = new string[] { Target.Any };

	public PlaceDestroyedPresence( int range, params string[] filterEnums )
		:base( $"PlaceDestroyedPresence({range},"+string.Join( "Or", filterEnums )+")" )
	{
		Range = range;
		FilterEnums = filterEnums;
		FilterDescription = string.Join( "Or", FilterEnums );
	}

	#endregion

	public int Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

	public override async Task ActAsync( SelfCtx ctx ) {
		Spirit self = ctx.Self;
		if(self.Presence.Destroyed == 0) return;
		var toOptions = self.FindSpacesWithinRange( new TargetCriteria(Range), false )
			.Where( self.Presence.CanBePlacedOn )
			.ToArray();
		if(toOptions.Length == 0)
			throw new InvalidOperationException( "There no places to place presence." );
		Space to = await self.Gateway.Decision( Select.ASpace.ToPlacePresence( toOptions, Present.Always, self.Presence.Token ) );
		await ctx.Target( to ).Presence.PlaceDestroyedHere();
	}
}
