namespace SpiritIsland;

public class DestroyedPresence( SpiritPresenceToken token ) : TokenOn {
	public IToken Token { get; } = token;
	public ILocation Source => DestroyedPresencePile.Singleton;

	public string Text => "Destroyed Presence";

	public int Count { get; set; }

}

