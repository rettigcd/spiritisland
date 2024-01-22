namespace SpiritIsland.Basegame.Adversaries;

class FranceBuilder( bool _hasSlaveLabor, bool _hasTriangeTrade ) : BuildEngine {

	public override async Task Do1Build( GameState gameState, SpaceState spaceState ) {
		int initialCityCount = spaceState.Sum( Human.City );
		await base.Do1Build( gameState, spaceState );
		if(_hasSlaveLabor)
			await DoSlaveLaborAsync( spaceState );
		if(_hasTriangeTrade)
			await DoTriangleTrade( spaceState, initialCityCount );
	}

	static async Task DoTriangleTrade( SpaceState tokens, int initialCityCount ) {
		// Whenever Invaders Build a Coastal City
		if(tokens.Space.IsCoastal && tokens.Sum( Human.City ) > initialCityCount) {
			// add 1 Town to the adjacent land with the fewest Town.
			var buildSpace = tokens.Adjacent
				.OrderBy( t => t.Sum( Human.Town ) )
				.First();
			await using var scope = await ActionScope.Start(ActionCategory.Adversary);
			await buildSpace.AddDefaultAsync( Human.Town, 1 );
		}
	}

	static async Task DoSlaveLaborAsync( SpaceState tokens ) {
		// After Invaders Build in a land with 2 Explorer or more,
		// replace all but 1 Explorer there with an equal number of Town.

		int explorerCount = tokens.Sum( Human.Explorer );
		if(explorerCount < 2) return;

		// remove explorers
		int numToReplace = explorerCount - 1;
		while(0 < numToReplace) {
			var oldExplorer = tokens.HumanOfTag( Human.Explorer ).OrderByDescending( x => x.StrifeCount ).FirstOrDefault();
			int count = Math.Min( tokens[oldExplorer], numToReplace );

			await tokens.ReplaceHumanAsync( oldExplorer, Human.Town );

			// next
			numToReplace -= count;
		}
	}

}

