namespace SpiritIsland.NatureIncarnate;

public class AddOrMoveIncarnaToPresence : SpiritAction {

	public AddOrMoveIncarnaToPresence():base( "Add or Move Incarna to Presence" ) { }

	public override async Task ActAsync( Spirit self ) {

		var space = await self.SelectAsync( new A.SpaceDecision( "Select space to place Incarna.", self.Presence.Lands, Present.Done ) );
		if(space == null) return;

		// Move/Place Incarna
		await self.Incarna.MoveTo(space,true);
	}

}
