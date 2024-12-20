namespace SpiritIsland.FeatherAndFlame;

public class AddBagPresenceOn1Space : SpiritAction {

	public AddBagPresenceOn1Space():base( "Setup_PlacePresenceOnSpace1" ) { }

	// Put 1 presence on any board in land #1.
	public override async Task ActAsync( Spirit self ) {
		var options = ActionScope.Current.Spaces_Existing;
		Space space = await self.SelectAlways("Add presence to", options);
		await self.Presence.Token.AddTo(space);
	}

}