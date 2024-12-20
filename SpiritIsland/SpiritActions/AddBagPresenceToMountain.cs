namespace SpiritIsland.JaggedEarth;

/// <summary> Adds a presence (from the Bag) to a mountain. </summary>
/// <remarks> Used for Volcano Setup. </remarks>
public class AddBagPresenceToMountain : SpiritAction { // Similar to SharpFang initialization

	public AddBagPresenceToMountain():base( "Place Presence on Mountain" ) { }

	public override async Task ActAsync( Spirit self ) {

		// Put 1 Presence and 1 Badlands on your starting board in a mountain of your choice.
		// Push all Dahan from that land.

		// Put 1 presence on your starting board in a mountain of your choice.
		var options = GameState.Current.Spaces.Where( space=>space.SpaceSpec.IsMountain );
		Space space = await self.SelectAlways("Add presence to", options);
		await self.Presence.Token.AddTo(space);
		await space.AddDefaultAsync( Token.Badlands, 1);

		// Push all dahan from that land.
		var targetCtx = self.Target(space);
		if(targetCtx.Dahan.Any)
			await targetCtx.PushDahan(targetCtx.Dahan.CountAll);
	}

}