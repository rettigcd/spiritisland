namespace SpiritIsland;

public static class ResolveOutOfPhaseAction {

	/// <summary>
	/// Selects an out-of-phase Action and Resolves it.
	/// </summary>
	public static async Task Execute( Spirit spirit ) {

		var gs = GameState.Current;
		var resultingSpeed = gs.Phase;
		var cardsOriginalSpeed = OriginalSpeed( resultingSpeed );
		var changeableFactories = spirit.GetAvailableActions( cardsOriginalSpeed ).OfType<IFlexibleSpeedActionFactory>().ToArray();

		// Select the action.
		IFlexibleSpeedActionFactory factory = (IFlexibleSpeedActionFactory)await spirit.SelectAsync(new A.TypedDecision<IActionFactory>(
			GetPrompt(resultingSpeed),
			changeableFactories, 
			Present.Done
		));

		if( factory != null) {
			// Temporarily change its speed - is this necessary?
			TemporarySpeed.Override( factory, resultingSpeed, gs );
			// Resolve it.
			await spirit.ResolveUnresolvedActionAsync( factory, resultingSpeed );
		}
	}

	static Phase OriginalSpeed( Phase resultingSpeed ) => resultingSpeed switch {
		Phase.Fast => Phase.Slow,
		Phase.Slow => Phase.Fast,
		Phase.FastOrSlow => Phase.FastOrSlow,
		_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
	};

	static string GetPrompt( Phase resultingSpeed ) => resultingSpeed switch {
		Phase.Fast => "Select action to make fast.",
		Phase.Slow => "Select action to make slow.",
		Phase.FastOrSlow => "Select action to toggle fast/slow.",
		_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
	};

}
