namespace SpiritIsland;

public class MoveIncarnaToPresence : SpiritAction {

	public MoveIncarnaToPresence(bool allowAdd=true):base( "Add or Move Incarna to Presence" ) {
		_allowAdd = allowAdd;
	}

	public override async Task ActAsync( Spirit self ) {

		var space = await self.Select( "Select space to place Incarna.", self.Presence.Lands, Present.Done );
		if(space is null) return;

		// Move/Place Incarna
		await self.Incarna.MoveTo(space, _allowAdd);
	}

	readonly bool _allowAdd;
}
