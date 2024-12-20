namespace SpiritIsland.BranchAndClaw;

public class AddBagPresenceToBeastLand : SpiritAction {

	public AddBagPresenceToBeastLand():base( "Setup_PlacePresenceOnBeastLand" ) { }

	public override async Task ActAsync( Spirit self ) {
		var options = ActionScope.Current.Spaces_Existing.Where( space=>space.Beasts.Any );
		Space space = await self.SelectAlways("Add presence to",options);
		await self.Presence.Token.AddTo( space );
	}

}