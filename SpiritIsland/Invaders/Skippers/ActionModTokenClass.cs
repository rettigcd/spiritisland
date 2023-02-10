namespace SpiritIsland;

public class ActionModTokenClass : IEntityClass {

	public static readonly IEntityClass Singleton = new ActionModTokenClass();

	static readonly public IEntityClass Class = new ActionModTokenClass();
	public string Label => "Mod";

	public TokenCategory Category => TokenCategory.Skipper;
}
