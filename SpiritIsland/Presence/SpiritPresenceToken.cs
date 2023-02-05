namespace SpiritIsland;

public class SpiritPresenceToken : IVisibleToken, TokenClass, ITrackMySpaces {

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

	public void Adjust( Space space, int delta ) {
		_spaces[space] += delta;
		_boards[space.Board] += delta;
	}

	public bool IsOn(Board board) => 0 < _boards[board];
	public bool IsOn( Space space ) => 0 < _spaces[space];
	public int this[Board board] => _boards[board];
	public int this[Space space] => _spaces[space];

	readonly CountDictionary<Space> _spaces= new CountDictionary<Space>();
	readonly CountDictionary<Board> _boards= new CountDictionary<Board>();
}
