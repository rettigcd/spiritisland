namespace SpiritIsland;

/// <summary>
/// Base class Token for Skips and mods. Hidden
/// </summary>
public class ActionModBaseToken : Token {

	protected ActionModBaseToken( string label, UsageCost cost = UsageCost.Free ) {
		Text = label;
		Cost = cost;
	}

	public UsageCost Cost { get; } // not needed by everything, but needed by most

	public string Text { get; }

	public TokenClass Class => _classSingleton;

	public string SpaceAbreviation => null; // hidden

	#region static TokenClass

	static readonly TokenClass _classSingleton = new ActionModTokenClass();

	class ActionModTokenClass : TokenClass {

		static readonly public TokenClass Class = new ActionModTokenClass();
		public string Label => "ActionMod";

		public TokenCategory Category => TokenCategory.Skipper;
	}
	#endregion

}