namespace SpiritIsland.Tests;

public static class DecisionExtensions {
	static public IOption FindChoice( this IDecision decision, IOption choice ) {
		// This is really just validation, not finding.
		// Formatted this way to match the other FindChoice(text)
		return decision.Options.Contains( choice ) ? choice
			: throw new Exception( $"{choice.Text} not found in: " + decision.FormatDecision() );
	}

	static public IOption FindFirstChoice( this IDecision decision, string choiceText, int skip )
		=> decision.Options.Where( o => o.Text == choiceText ).Skip( skip ).FirstOrDefault()
		?? throw new ArgumentOutOfRangeException( nameof(choiceText), $"sequence [{decision.FormatOptions()}]does not contain option: {choiceText}" );

	static public IOption FindFirstChoiceStartsWith( this IDecision decision, string choicePrefix )
		=> decision.Options.FirstOrDefault( o => o.Text.StartsWith(choicePrefix) )
		?? throw new ArgumentOutOfRangeException( nameof( choicePrefix ), $"sequence [{decision.FormatOptions()}]does not contain option: {choicePrefix}" );

	static public IOption FindChoice( this IDecision decision, string expectedChoiceText ) {
		var matchingChoices = decision.Options
			.Where( o => o.Text == expectedChoiceText )
			.ToArray();

		switch(matchingChoices.Length) {
			case 1: return matchingChoices[0];
			case 0: {
					IOption firstMatch = decision.Options
						.FirstOrDefault( o => o.Text.ToLower().Contains( expectedChoiceText.ToLower() ) );
					string msg = firstMatch != null
						? $"Expected '{expectedChoiceText}' not found. Actual: '{firstMatch.Text}'"
						: $"option ({expectedChoiceText} not found in {decision.FormatDecision()}";
					throw new ArgumentException( msg );
				}
			default:
				throw new Exception( $"Multiple option contain '{expectedChoiceText}' in: " + decision.FormatDecision() );
		}
	}

	static public string FormatDecision( this IDecision d )
		=> d.Prompt + ":" + d.Options.Select( x => x.Text ).Join( "," );

	static public string FormatOptions( this IDecision decision )
		=> decision.Options.Select( x => x.Text ).Join( "," );

}