namespace SpiritIsland.NatureIncarnate;

/// <summary> Move only. Not Add </summary>
public class MoveIncarnaAnywhere : SpiritAction {

	public MoveIncarnaAnywhere():base( "Move Incarna anywhere" ) { }
	public override async Task ActAsync( Spirit self ) {
		var incarna = self.Incarna;
		if(!incarna.IsPlaced) return; // not on board, don't add

		Space space = await self.SelectAsync( new A.Space( "Select space to place Incarna.", ActionScope.Current.Tokens, Present.Done ) );
		if(space == null) return;

		await incarna.AsSpaceToken().MoveTo( space.ScopeTokens );
	}

}
