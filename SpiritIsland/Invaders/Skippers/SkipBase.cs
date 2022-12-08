namespace SpiritIsland;

/// <summary>
/// Base class Token for Skips and mods. Hidden
/// </summary>
public class SkipBase : Token {

	protected SkipBase(string label,UsageCost cost = UsageCost.Free ) {
		Text = label;
		Cost = cost;
	}

	public UsageCost Cost { get; } // not needed by everything, but needed by most

	public string Text { get; }

	public TokenClass Class => _classSingleton;

	public string SpaceAbreviation => null; // hidden

	#region static TokenClass

	static readonly TokenClass _classSingleton = new SkipClass();

	class SkipClass : TokenClass {

		static readonly public TokenClass Class = new SkipClass();
		public string Label => "InvaderMod";

		public TokenCategory Category => TokenCategory.Skipper;
	}
	#endregion

}