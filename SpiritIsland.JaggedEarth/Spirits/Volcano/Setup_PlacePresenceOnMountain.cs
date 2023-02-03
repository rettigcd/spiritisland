namespace SpiritIsland.JaggedEarth;
class Setup_PlacePresenceOnMountain : GrowthActionFactory { // Similar to SharpFang initialization

	public override async Task ActivateAsync( SelfCtx ctx ) {

		// Put 1 Presence and 1 Badlands on your starting board in a mountain of your choice.
		// Push all Dahan from that land.

		// Put 1 presence on your starting board in a mountain of your choice.
		var options = ctx.GameState.AllActiveSpaces.Downgrade().Where( space=>space.IsMountain );
		var space = await ctx.Decision(new Select.Space("Add presence to",options, Present.Always));
		await ctx.Presence.PlaceOn(space);
		ctx.GameState.Tokens[space].Adjust( Token.Badlands, 1);

		// Push all dahan from that land.
		var targetCtx = ctx.Target(space);
		if(targetCtx.Dahan.Any)
			await targetCtx.PushDahan(targetCtx.Dahan.CountAll);
	}

	public override bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Init;

}