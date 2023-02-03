namespace SpiritIsland;

public class ActionModTokenClass : TokenClass {

	public static readonly TokenClass Singleton = new ActionModTokenClass();

	static readonly public TokenClass Class = new ActionModTokenClass();
	public string Label => "Mod";

	public TokenCategory Category => TokenCategory.Skipper;
}
