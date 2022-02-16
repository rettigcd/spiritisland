namespace SpiritIsland;

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
