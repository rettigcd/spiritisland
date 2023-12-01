namespace SpiritIsland.JaggedEarth;

public class PlacePresenceAndBeast : SpiritAction {

	public PlacePresenceAndBeast():base( "PlacePresenceAndBeast" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var from = await ctx.Self.SelectSourcePresence();

		var options = DefaultRangeCalculator.Singleton.GetSpaceOptions( ctx.Self.Presence.Lands.Tokens(), new TargetCriteria( 3 ) );
		Space to = await ctx.Self.Select( A.Space.ToPlacePresence( options.Downgrade(), Present.Always, ctx.Self.Presence.Token ) );

		await ctx.Self.Presence.PlaceAsync( from, to );
		await ctx.Target(to).Beasts.AddAsync(1);
	}

}