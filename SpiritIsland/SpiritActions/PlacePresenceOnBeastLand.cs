namespace SpiritIsland.BranchAndClaw;

public class PlacePresenceOnBeastLand : SpiritAction {

	public PlacePresenceOnBeastLand():base( "Setup_PlacePresenceOnBeastLand" ) { }

	public override async Task ActAsync( Spirit self ) {
//		var gameState = GameState.Current;
		var options = ActionScope.Current.Spaces_Unfiltered.Where( space=>space.Beasts.Any );
		var space = await self.SelectAlwaysAsync(new A.SpaceDecision("Add presence to",options, Present.Always));
		await self.Presence.Token.AddTo( space );
	}

}