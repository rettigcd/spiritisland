namespace SpiritIsland.Agent;

/// <summary> Baseline: plays a whole game by picking uniformly among the offered options. </summary>
public static class RandomAgent {

	public static GameOverLogEntry? Play( AgentGame game, int agentSeed, int maxDecisions = 100_000 ) {
		var rnd = new Random( agentSeed );
		for( int n = 0; n < maxDecisions; ++n ) {
			DecisionSnapshot? d = game.GetDecision();
			if( d is null ) return game.Result;
			game.Select( d.Version, d.Options[rnd.Next( d.Options.Length )] );
		}
		throw new InvalidOperationException( $"Game not finished after {maxDecisions} decisions." );
	}
}
