namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Method )]
public class InstructionsAttribute : Attribute {

	public string Text { get; }

	public InstructionsAttribute( string text ) {
		Text = text;
	}

}
