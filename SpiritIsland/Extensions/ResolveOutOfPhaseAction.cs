namespace SpiritIsland;

public class ResolveOutOfPhaseAction {

	public async Task Execute( SelfCtx ctx ) {
		var resultingSpeed = ctx.GameState.Phase;
		var originalSpeed = OriginalSpeed( resultingSpeed );
		var changeableFactories = ctx.Self.GetAvailableActions( originalSpeed ).OfType<IFlexibleSpeedActionFactory>().ToArray();
		var prompt = Prompt( resultingSpeed );

		IFlexibleSpeedActionFactory factory = (IFlexibleSpeedActionFactory)await ctx.Self.SelectFactory( prompt, changeableFactories, Present.Done );

		if(factory != null) {
			TemporarySpeed.Override( factory, resultingSpeed, ctx.GameState );
			await ctx.Self.TakeAction( factory, ctx );
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

public class TemporarySpeed : ISpeedBehavior {

	static public void Override( IFlexibleSpeedActionFactory factory, Phase phase, GameState gameState ) {
		factory.OverrideSpeedBehavior = new TemporarySpeed( phase );

		Task Restore(GameState _) { 
			factory.OverrideSpeedBehavior = null;
			return Task.CompletedTask;
		}
		gameState.TimePasses_ThisRound.Push( Restore );
	}


	readonly Phase newSpeed;
	public TemporarySpeed(Phase newSpeed ) { this.newSpeed = newSpeed; }

	public bool CouldBeActiveFor( Phase requestSpeed, Spirit _ )
		=> requestSpeed == newSpeed;
			
	public Task<bool> IsActiveFor( Phase requestSpeed, Spirit _ ) 
		=> Task.FromResult(requestSpeed == newSpeed);
}
