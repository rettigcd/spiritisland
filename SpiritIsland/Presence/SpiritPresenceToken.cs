namespace SpiritIsland;

public class SpiritPresenceToken : Token, TokenClass, IAppearOnScreen {

	#region private
//	static int tokenTypeCount; // so each spirit presence gets a different number
	#endregion

	public SpiritPresenceToken() {
		// Text = "P" + (tokenTypeCount++); // !! This kind of sucks. Could be based on Spirit or starting Board-name
		// !!! DeployedPresenceDecisoin: IslandControl needs access to the PresenceToken so it can record the Location for creating hotspots during.
		Text = "Presence";      // !!! this only works in SOLO.
	}

	#region Token parts

	public TokenClass Class => this;

	public string Text { get; }

	#endregion

	#region TokenClass parts
	string TokenClass.Label => "Presence";
	TokenCategory TokenClass.Category => TokenCategory.Presence;
	#endregion
	Img IAppearOnScreen.Img => Img.Icon_Presence; // ???

}
