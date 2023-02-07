using System;

namespace SpiritIsland.JaggedEarth;

public class PlacePresenceAndBeast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var from = await ctx.Self.SelectSourcePresence();
		Space to = await ctx.Self.SelectDestinationWithinRange( new TargetCriteria( 3 ), false );
		await ctx.Self.Presence.Place( from, to );
		await ctx.Target(to).Beasts.Add(1);
	}

}