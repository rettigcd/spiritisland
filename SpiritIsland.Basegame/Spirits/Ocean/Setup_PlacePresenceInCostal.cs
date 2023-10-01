namespace SpiritIsland.Basegame;

public class Setup_PlacePresenceInCostal : GrowthActionFactory {

	// ! Can't used normal PlacePresence, because it must be range-1, range 0 not allowed.
	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.Self.Presence.Spaces.First().Adjacent_Existing.Tokens();
		var space = await ctx.Decision( new Select.ASpace( "Add presence to", options, Present.Always ) );
		await ctx.Self.Presence.Token.AddTo( space );
	}
	public override bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Init;

}