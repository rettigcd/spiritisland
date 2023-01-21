namespace SpiritIsland.FeatherAndFlame;

class ScotlandExploreEngine : ExploreEngine {

	// Level 1 - Trading Port:
	// After Setup, in Coastal lands,
	// Explore Cards add 1 Town instead of 1 Explorer.
	// "Coastal Lands" Invader cards do this for maximum 2 lands per board.

	CountDictionary<Board> _initialExploreTownsAdded = new CountDictionary<Board>();

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		await base.ActivateCard( card, gameState );
		_initialExploreTownsAdded = null; // done with initial explore.
	}

	protected override async Task AddToken( ActionableSpaceState tokens ) {
		if( _initialExploreTownsAdded != null // Initial explore
			&& tokens.Space.IsCoastal
			&& _initialExploreTownsAdded[tokens.Space.Board] < 2 // max of 2
		) {
			await tokens.AddDefault( Invader.Town, 1, AddReason.Explore );
			_initialExploreTownsAdded[tokens.Space.Board]++;
			tokens.AccessGameState().Log( new LogDebug("Trading Port: Adding town to "+tokens.Space.Text) );
		} else {
			await base.AddToken( tokens );
		}
	}
}
