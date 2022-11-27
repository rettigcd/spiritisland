namespace SpiritIsland;

/// <summary> Since tokens are directional, create separate 1 for each end. </summary>
/// <remarks> Must be in Engine project so that Memento can save/restore it.</remarks>
public class GatewayToken : Token {
	public SpaceState From { get; }
	public SpaceState To { get; }
	public GatewayToken( SpaceState from, SpaceState to ) {
		From = from;
		To = to;
		// Add self
		From.Init( this, 1 );
		From.LinkedViaWays = to;
	}
	public void RemoveSelf() {
		From.Init( this, 0 );
		From.LinkedViaWays = null;
	}
	public TokenClass Class => TokenType.OpenTheWays;
	public string Text => "Gateway";
	public string SpaceAbreviation => null;

}
