namespace SpiritIsland.PromoPack1;

class ScotlandExploreEngine : ExploreEngine {

	CountDictionary<Board> _initialExploreTownsAdded = new CountDictionary<Board>();

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		await base.ActivateCard( card, gameState );
		_initialExploreTownsAdded = null; // done with initial explore.
	}

	protected override async Task AddToken( ActionableSpaceState tokens ) {
		if( _initialExploreTownsAdded != null // Initial explore
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
