namespace SpiritIsland.FeatherAndFlame;

public class Setup_PlacePresenceOnSpace1 : GrowthActionFactory {

	// Put 1 presence on any board in land #1.
	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.GameState.Island.Boards.Select( b => b[1] );
		var space = await ctx.Decision( new Select.ASpace( "Add presence to", options, Present.Always ) );
		await ctx.Self.Presence.Token.AddTo( space );
	}
	public override bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Init;

}