namespace SpiritIsland.NatureIncarnate;

/// <summary> Move only. Not Add </summary>
public class MoveIncarnaAnywhere : SpiritAction {

	public MoveIncarnaAnywhere():base( "Move Incarna anywhere" ) { }
	public override async Task ActAsync( Spirit self ) {
		// Move/Place Incarna
		if(self.Presence is not IHaveIncarna ihi) return;
		var incarna = ihi.Incarna;
		if(incarna.Space == null) return; // not on board, don't add

		Space space = await self.SelectAsync( new A.Space( "Select space to place Incarna.", GameState.Current.Spaces, Present.Done ) );
		if(space == null) return;

		await incarna.Space.RemoveAsync( incarna, 1 );
		await space.Tokens.AddAsync( incarna, 1 );

	}

}
