namespace SpiritIsland.NatureIncarnate;

/// <summary> Move only. Not Add </summary>
public class MoveIncarnaAnywhere : SpiritAction {

	public MoveIncarnaAnywhere():base( "Move Incarna anywhere" ) { }
	public override async Task ActAsync( Spirit self ) {
		var incarna = self.Incarna;
		if(!incarna.IsPlaced) return; // not on board, don't add

		Space? space = await self.Select("Select space to place Incarna.", ActionScope.Current.Spaces, Present.Done);
		if(space is null) return;

		await incarna.AsSpaceToken().MoveTo( space );
	}

}
