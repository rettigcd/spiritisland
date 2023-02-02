namespace SpiritIsland;

public class SpiritPresenceToken : IVisibleToken, TokenClass {

	public SpiritPresenceToken() {
		Text = "Presence";      // !!! this only works in SOLO.
	}

	#region Token parts

	public string Text { get; }
	Img IVisibleToken.Img => Img.Icon_Presence;
	public TokenClass Class => this;

	#endregion

	#region TokenClass parts
	string TokenClass.Label => "Presence";
	TokenCategory TokenClass.Category => TokenCategory.Presence;
	#endregion

}
