namespace SpiritIsland;

public class IsolateToken : TokenClassToken, IEndWhenTimePasses, IIsolate {
	public IsolateToken( string label, char initial, Img img ) 
		: base( label, initial, img ) {}

	public bool IsIsolated => true;
}

public interface IIsolate {
	bool IsIsolated { get; }
}
