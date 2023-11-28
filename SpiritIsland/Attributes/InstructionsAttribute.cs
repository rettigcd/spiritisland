namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Method )]
public class InstructionsAttribute : Attribute {

	public string Text { get; }

	public InstructionsAttribute( string text ) {
		Text = text;
	}

	public InstructionsAttribute( string text, string elements, string thresholdText ) {
		Text = $"{text} -If you have- {elements}: {thresholdText}";
	}



}
