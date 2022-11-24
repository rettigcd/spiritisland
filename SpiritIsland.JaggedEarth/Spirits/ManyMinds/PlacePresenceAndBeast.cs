namespace SpiritIsland.JaggedEarth;

class PlacePresenceAndBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var from = await ctx.Presence.SelectSource();
		Space to = await ctx.Presence.SelectDestinationWithinRange( 3, Target.Any, TargetingPowerType.None );
		await ctx.Self.Presence.Place( from, to, ctx.GameState );
		await ctx.Target(to).Beasts.Add(1);
	}

}