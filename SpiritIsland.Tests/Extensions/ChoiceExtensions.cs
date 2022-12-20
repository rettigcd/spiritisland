namespace SpiritIsland.Tests;

public static class ChoiceExtensions {

	/// <summary> Binds to the Next Decision </summary>
	public static DecisionContext NextDecision( this Spirit spirit ) => new DecisionContext( spirit );

	#region IDecsion extensions

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

	static public IOption FindChoice( this IDecision decision, string choiceText ) {
		var matchingChoices = decision.Options
			.Where( o => o.Text.Contains( choiceText ) )
			.ToArray();

		switch(matchingChoices.Length) {
			case 1: return matchingChoices[0];
			case 0: {
					IOption firstMatch = decision.Options
						.FirstOrDefault( o => o.Text.ToLower().Contains( choiceText.ToLower() ) );
					string msg = firstMatch != null
						? $"Please correct capitalization from '{choiceText}' to '{firstMatch.Text}'"
						: $"option ({choiceText} not found in {decision.FormatDecision()}";
					throw new ArgumentException( msg );
				}
			default:
				throw new Exception( $"Multiple option contain '{choiceText}' in: " + decision.FormatDecision() );
		}
	}

	static public string FormatDecision( this IDecision d )
		=> d.Prompt + ":" + d.Options.Select( x => x.Text ).Join( "," );

	static public string FormatOptions( this IDecision decision )
		=> decision.Options.Select( x => x.Text ).Join( "," );

	#endregion IDecision extensions

}