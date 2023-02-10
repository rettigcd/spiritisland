namespace SpiritIsland.Basegame.Adversaries;

class FranceBuilder : BuildEngine {
	readonly bool _hasSlaveLabor;
	readonly bool _hasTriangleTrade;

	public FranceBuilder( int level ) {
		_hasSlaveLabor = 2 <= level;
		_hasTriangleTrade = 4 < level;
	}

	public override async Task Do1Build( GameState gameState, SpaceState spaceState ) {
		int initialCityCount = spaceState.Sum( Human.City );
		await base.Do1Build( gameState, spaceState );
		if(_hasSlaveLabor)
			DoSlaveLabor( spaceState );
		if(_hasTriangleTrade)
			await DoTriangleTrade( gameState, spaceState, initialCityCount );
	}

	static async Task DoTriangleTrade( GameState gs, SpaceState tokens, int initialCityCount ) {
		// Whenever Invaders Build a Coastal City
		if(tokens.Space.IsCoastal && tokens.Sum( Human.City ) > initialCityCount) {
			var terrainMapper = gs.Island.Terrain;
			// add 1 Town to the adjacent land with the fewest Town.
			var buildSpace = tokens.Adjacent
				.Where( terrainMapper.IsInPlay )
				.OrderBy( t => t.Sum( Human.Town ) )
				.First();
			// !!!
			_ = new ActionScope( ActionCategory.Adversary ); // !!! Dispose or don't use
			await buildSpace.AddDefault( Human.Town, 1 );
		}
	}

	static void DoSlaveLabor( SpaceState tokens ) {
		// After Invaders Build in a land with 2 Explorer or more,
		// replace all but 1 Explorer there with an equal number of Town.

		int explorerCount = tokens.Sum( Human.Explorer );
		if(explorerCount < 2) return;

		// remove explorers
		int numToReplace = explorerCount - 1;
		while(numToReplace > 0) {
			var explorerToken = tokens.OfHumanClass( Human.Explorer ).OrderByDescending( x => x.StrifeCount ).FirstOrDefault();
			int count = Math.Min( tokens[explorerToken], numToReplace );
			// Replace
			tokens.Adjust( explorerToken, -count );
			tokens.AdjustDefault( Human.Town, count );
			// next
			numToReplace -= count;
		}
	}

}

