namespace SpiritIsland.JaggedEarth;

class PlacePresenceAndBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var from = await ctx.Presence.SelectSource();
		Space to = await ctx.Presence.SelectDestinationWithinRange( new TargetCriteria(3), TargetingPowerType.None );
		await ctx.Presence.Place( from, to );
		await ctx.Target(to).Beasts.Add(1);
	}

}