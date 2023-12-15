namespace SpiritIsland;

public class DestroyedPresence : TokenOn {
	public DestroyedPresence(SpiritPresenceToken token ) {  Token = token; }
	public IToken Token { get; }

	public ILocation Source => DestroyedPresencePile.Singleton;

	public string Text => "Destroyed Presence";

	public int Count { get; set; }

}

