namespace SpiritIsland;

public class IsolateToken( string label, char initial, Img img ) 
	: TokenClassToken( label, initial, img )
	, IEndWhenTimePasses
	, IIsolate
{
	public bool IsIsolated => true;
}

public interface IIsolate {
	bool IsIsolated { get; }
}
