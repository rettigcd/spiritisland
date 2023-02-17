namespace SpiritIsland;

public class ActionModTokenClass : IEntityClass {

	public static readonly ActionModTokenClass Mod = new ActionModTokenClass("Mod");

	public ActionModTokenClass(string label ) {  Label = label; }

	public string Label { get; }

	public TokenCategory Category => TokenCategory.Skipper;
}
