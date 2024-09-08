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

	// For Moves
	static public SpaceToken FindSourceChoice( this IDecision decision, string expectedChoiceText ) {
		Move[] moves = decision.Options.OfType<Move>().ToArray();
		SpaceToken[] spaceTokenSources = moves.Select(m=>m.Source).Distinct().ToArray();
		bool isSingleLandSource = spaceTokenSources.Select(x=>x.Space.SpaceSpec).Distinct().Count() == 1;
		bool matcher(SpaceToken st) => st.ToString().Contains(expectedChoiceText);
		var matchingSources = spaceTokenSources.Where( matcher ).ToArray();
		switch(matchingSources.Length) {
			case 1: return matchingSources[0];
			case 0: {
					string msg = $"option ({expectedChoiceText} not found in {decision.FormatDecision()}";
					throw new ArgumentException( msg );
				}
			default:
				throw new Exception( $"Multiple option contain '{expectedChoiceText}' in: " + decision.FormatDecision() );
		}
	}

	static public IOption FindChoice( this IDecision decision, string expectedChoiceText ) {
		var matchingChoices = decision.Options
			.Where( o => o.Text == expectedChoiceText ) // not using .Contains because Numbers '1' doesn't work when > 9
			.ToArray();

		return matchingChoices.Length switch {
			1 => matchingChoices[0],
			0 => FindChoiceContains( decision, expectedChoiceText ),
			_ => throw new Exception( $"Multiple option EQUAL '{expectedChoiceText}' in: " + decision.FormatDecision() )
		};
	}

	private static IOption FindChoiceContains( IDecision decision, string expectedChoiceText ) {
		var matchingChoices = decision.Options
			.Where( o => o.Text.Contains( expectedChoiceText ) ) // not using .Contains because Numbers '1' doesn't work when > 9
			.ToArray();
		return matchingChoices.Length switch {
			1 => matchingChoices[0],
			0 => throw new ArgumentException( $"option ({expectedChoiceText}) not found in ({decision.FormatDecision()})" ),
			_ => throw new Exception( $"Multiple option CONTAIN '{expectedChoiceText}' in: " + decision.FormatDecision() )
		};
	}

	static public string FormatDecision( this IDecision d )
		=> d.Prompt + ":" + d.Options.Select( x => x.Text ).Join( "," );

	static public string FormatOptions( this IDecision decision )
		=> decision.Options.Select( x => x.Text ).Join( "," );

	///<summary>For Decisions that have SpaceTokens-options from the same Space, remove the Space.</summary>
	///<remarks>This is slower, always try straight FormatOptions first</remarks>
	static public string FromatSpaceTokenOptions( this IDecision decision ) {
		bool hasOneSpace = decision.Options.OfType<SpaceToken>()
			.Select(st=>st.Space.SpaceSpec)
			.Distinct()
			.Count() == 1;
		Func<IOption,string> formatter = hasOneSpace
			? (o) => o is SpaceToken st ? st.Token.Text : o.Text // Remove Spaces from SpaceTokens
			: (o) => o.Text; // do normal
		return decision.Options.Select( formatter ).Join(",");
	}

}