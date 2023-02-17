namespace SpiritIsland;

public class IsolateToken : TokenClassToken, IEndWhenTimePasses {
	public IsolateToken( string label, char initial, Img img, TokenCategory cat = TokenCategory.None ) 
		: base( label, initial, img, cat ) {}
}