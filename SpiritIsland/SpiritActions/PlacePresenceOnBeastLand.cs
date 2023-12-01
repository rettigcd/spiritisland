namespace SpiritIsland.BranchAndClaw;

public class PlacePresenceOnBeastLand : SpiritAction {

	public PlacePresenceOnBeastLand():base( "Setup_PlacePresenceOnBeastLand" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var gameState = GameState.Current;
		var options = gameState.Spaces_Unfiltered.Where( space=>space.Beasts.Any );
		var space = await ctx.SelectAsync(new A.Space("Add presence to",options, Present.Always));
		await ctx.Self.Presence.Token.AddTo(space);
	}

}