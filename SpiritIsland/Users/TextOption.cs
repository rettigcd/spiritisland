namespace SpiritIsland;

public class TextOption( string text ) : IOption {

	static public readonly TextOption Done = new TextOption("Done");

	public string Text { get; } = text;
	public bool Matches(IOption option ){
		return option is TextOption txt && txt.Text == Text;
	}
}
