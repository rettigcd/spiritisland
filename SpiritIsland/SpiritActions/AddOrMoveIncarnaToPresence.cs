namespace SpiritIsland.NatureIncarnate;

public class AddOrMoveIncarnaToPresence : SpiritAction {

	public AddOrMoveIncarnaToPresence():base( "Add or Move Incarna to Presence" ) { }

	public override async Task ActAsync( Spirit self ) {
		if(self.Presence is not IncarnaPresence presence) return;

		var space = await self.SelectAsync( new A.Space( "Select space to place Incarna.", self.Presence.Lands, Present.Done ) );
		if(space == null) return;

		// Move/Place Incarna
		await presence.Incarna.MoveTo(space,true);
	}

}
