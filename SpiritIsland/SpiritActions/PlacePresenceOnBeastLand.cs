namespace SpiritIsland.BranchAndClaw;

public class PlacePresenceOnBeastLand : SpiritAction {

	public PlacePresenceOnBeastLand():base( "Setup_PlacePresenceOnBeastLand" ) { }

	public override async Task ActAsync( Spirit self ) {
//		var gameState = GameState.Current;
		var options = ActionScope.Current.Tokens_Unfiltered.Where( space=>space.Beasts.Any );
		var space = await self.SelectAsync(new A.Space("Add presence to",options, Present.Always));
		await self.Presence.Token.AddTo(space.ScopeTokens);
	}

}