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

		// Find a space to place (destroyed) presense
		Space to = await self.Select( A.Space.ToPlaceDestroyedPresence(
			new TargetCriteria( Range ),
			Present.Always,
			self
		) );

		await ctx.Target( to ).Presence.PlaceDestroyedHere();
	}
}
