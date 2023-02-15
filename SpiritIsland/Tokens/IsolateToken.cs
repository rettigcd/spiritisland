namespace SpiritIsland;

public class IsolateToken : TokenClassToken, ISpaceEntityWithEndOfRoundCleanup {
	public IsolateToken( string label, char initial, Img img, TokenCategory cat = TokenCategory.None ) 
		: base( label, initial, img, cat ) {}

	public void EndOfRoundCleanup( SpaceState spaceState ) => spaceState.Init(this,0);
}