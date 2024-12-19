namespace SpiritIsland.BranchAndClaw;

public class PlacePresenceOnBeastLand : SpiritAction {

	public PlacePresenceOnBeastLand():base( "Setup_PlacePresenceOnBeastLand" ) { }

	public override async Task ActAsync( Spirit self ) {
//		var gameState = GameState.Current;
		var options = ActionScope.Current.Spaces_Unfiltered.Where( space=>space.Beasts.Any );
		var space = await self.SelectAlways("Add presence to",options);
		await self.Presence.Token.AddTo( space );
	}

}