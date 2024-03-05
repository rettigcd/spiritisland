namespace SpiritIsland;

public class DestroyedPresence( SpiritPresenceToken token ) : TokenLocation {
	public IToken Token { get; } = token;
	public ILocation Location => DestroyedPresencePile.Singleton;

	public string Text => "Destroyed Presence";

	public int Count { get; set; }

}

