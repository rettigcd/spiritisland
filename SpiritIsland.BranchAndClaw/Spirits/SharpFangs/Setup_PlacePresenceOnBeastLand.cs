namespace SpiritIsland.BranchAndClaw;

class Setup_PlacePresenceOnBeastLand : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var gameState = ctx.GameState;
		var options = gameState.AllSpaces.Where( space=>space.Beasts.Any );
		var space = await ctx.Decision(new Select.Space("Add presence to",options, Present.Always));
		await ctx.Presence.PlaceOn(space);
	}

	public override bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Init;

}