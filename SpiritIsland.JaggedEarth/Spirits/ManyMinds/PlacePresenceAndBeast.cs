namespace SpiritIsland.JaggedEarth;

public class PlacePresenceAndBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var from = await ctx.Self.SelectSourcePresence();

		var options = DefaultRangeCalculator.Singleton.GetTargetOptionsFromKnownSource( ctx.Self.Presence.Spaces.Tokens(), new TargetCriteria( 3 ) );
		Space to = await ctx.Self.Gateway.Decision( Select.ASpace.ToPlacePresence( options, Present.Always, Token.Beast ) );

		await ctx.Self.Presence.Place( from, to );
		await ctx.Target(to).Beasts.AddAsync(1);
	}

}