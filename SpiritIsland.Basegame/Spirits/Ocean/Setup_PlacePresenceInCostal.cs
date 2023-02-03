namespace SpiritIsland.Basegame;

public class Setup_PlacePresenceInCostal : GrowthActionFactory {

	// ! Can't used normal PlacePresence, because it must be range-1, range 0 not allowed.
	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.Presence.ActiveSpaceStates.First().Adjacent;
		var space = await ctx.Decision( new Select.Space( "Add presence to", options, Present.Always ) );
		await ctx.Presence.PlaceOn( space );
	}
	public override bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Init;

}