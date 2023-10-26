namespace SpiritIsland.NatureIncarnate;

public class PlaceDestroyedPresence : GrowthActionFactory {

	#region constructors

	public PlaceDestroyedPresence( int range ) {
		Range = range;
		FilterEnums = DefaultFilters;
		FilterDescription = Target.Any;
		Name = $"PlaceDestroyedPresence({range})";
	}

	static readonly string[] DefaultFilters = new string[] { Target.Any };

	public PlaceDestroyedPresence( int range, params string[] filterEnum ) {
		Range = range;
		FilterEnums = filterEnum;
		FilterDescription = string.Join( "Or", FilterEnums );
		Name = $"PlaceDestroyedPresence({range},{FilterDescription})";
	}

	#endregion

	public int Range { get; }
	public string FilterDescription { get; }
	public string[] FilterEnums { get; }

	public override string Name { get; }

	public override async Task ActivateAsync( SelfCtx ctx ) {
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
