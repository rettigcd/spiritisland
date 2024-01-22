namespace SpiritIsland.FeatherAndFlame;

class ScotlandExploreEngine : ExploreEngine {

	// Level 1 - Trading Port:
	// After Setup, in Coastal lands,
	// Explore Cards add 1 Town instead of 1 Explorer.
	// "Coastal Lands" Invader cards do this for maximum 2 lands per board.

	CountDictionary<Board> _specialExplore = null;

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		await base.ActivateCard( card, gameState );
		_specialExplore = [];// done with initial explore. the rest are special
	}

	protected override async Task AddToken( SpaceState tokens ) {
		if( !tokens.Space.IsCoastal || _specialExplore == null)
			await base.AddToken( tokens ); // do regular initial
		else if(tokens.Space.Boards.All(board => _specialExplore[board] < 2) ) { // max of 2
			// Do special explore
			await tokens.AddDefaultAsync( Human.Town, 1, AddReason.Explore );
			foreach(var b in tokens.Space.Boards)
				++_specialExplore[b];
			ActionScope.Current.Log( new SpiritIsland.Log.Debug("Trading Port: Adding town to "+tokens.Space.Text) );
		}

	}
}
