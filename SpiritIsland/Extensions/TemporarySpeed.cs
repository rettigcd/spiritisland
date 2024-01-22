namespace SpiritIsland;

public class TemporarySpeed( Phase newSpeed ) : ISpeedBehavior {

	static public void Override( IFlexibleSpeedActionFactory factory, Phase phase, GameState gameState ) {
		factory.OverrideSpeedBehavior = new TemporarySpeed( phase );
		gameState.AddTimePassesAction( TimePassesAction.Once( _=> factory.OverrideSpeedBehavior = null ) );
	}

	public bool CouldBeActiveFor( Phase requestSpeed, Spirit _ )
		=> requestSpeed == newSpeed;
			
	public Task<bool> IsActiveFor( Phase requestSpeed, Spirit _ ) 
		=> Task.FromResult(requestSpeed == newSpeed);
}
