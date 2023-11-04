namespace SpiritIsland;

public static class ResolveOutOfPhaseAction {

	public static async Task Execute( SelfCtx ctx ) {
		var resultingSpeed = GameState.Current.Phase;
		var originalSpeed = OriginalSpeed( resultingSpeed );
		var changeableFactories = ctx.Self.GetAvailableActions( originalSpeed ).OfType<IFlexibleSpeedActionFactory>().ToArray();
		var prompt = Prompt( resultingSpeed );

		IFlexibleSpeedActionFactory factory = (IFlexibleSpeedActionFactory)await ctx.Self.SelectFactory( prompt, changeableFactories, Present.Done );

		if(factory != null) {
			TemporarySpeed.Override( factory, resultingSpeed, GameState.Current );
			await ctx.Self.TakeActionAsync( factory, resultingSpeed );
		}
	}

	static Phase OriginalSpeed( Phase resultingSpeed ) => resultingSpeed switch {
		Phase.Fast => Phase.Slow,
		Phase.Slow => Phase.Fast,
		Phase.FastOrSlow => Phase.FastOrSlow,
		_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
	};

	static string Prompt( Phase resultingSpeed ) => resultingSpeed switch {
		Phase.Fast => "Select action to make fast.",
		Phase.Slow => "Select action to make slow.",
		Phase.FastOrSlow => "Select action to toggle fast/slow.",
		_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
	};

}
