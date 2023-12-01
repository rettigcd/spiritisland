namespace SpiritIsland.JaggedEarth;
public class PlacePresenceOnMountain : SpiritAction { // Similar to SharpFang initialization

	public PlacePresenceOnMountain():base( "Place Presence on Mountain" ) { }

	public override async Task ActAsync( Spirit self ) {

		// Put 1 Presence and 1 Badlands on your starting board in a mountain of your choice.
		// Push all Dahan from that land.

		// Put 1 presence on your starting board in a mountain of your choice.
		var options = GameState.Current.Spaces.Downgrade().Where( space=>space.IsMountain );
		var space = await self.SelectAsync(A.Space.ToPlacePresence(options, Present.Always,self.Presence.Token));
		await self.Presence.Token.AddTo(space);
		space.Tokens.Adjust( Token.Badlands, 1);

		// Push all dahan from that land.
		var targetCtx = self.Target(space);
		if(targetCtx.Dahan.Any)
			await targetCtx.PushDahan(targetCtx.Dahan.CountAll);
	}

}