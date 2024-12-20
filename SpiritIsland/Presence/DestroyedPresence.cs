namespace SpiritIsland;

public class DestroyedPresence( SpiritPresenceToken token ) : ITokenLocation {
	public IToken Token { get; } = token;
	public ILocation Location => DestroyedPresencePile.Singleton;

	public string Text => "Destroyed Presence";

	public int Count { get; set; }

	bool ITokenLocation.IsSacredSite => false;
}

