namespace SpiritIsland.FeatherAndFlame;

public class ScotlandRavageEngine : RavageEngine {
	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {

		await base.ActivateCard( card, gameState );
		// After the Ravage step,
		await FuelInwardGrowth( card, gameState );
	}

	static async Task FuelInwardGrowth( InvaderCard card, GameState gameState ) {
		var spaces = gameState.Spaces_Unfiltered
			// to each Inland land
			.Where( ss => !ss.Space.IsOcean && !ss.Space.IsCoastal )
			// that matches a Ravage card
			.Where( card.MatchesCard )
			// and is within 1 of town/city
			.Where( ss => ss.HasAny( Human.Town_City ) 
				|| ss.Adjacent.Any( adj => adj.HasAny( Human.Town_City ) ) )
			.ToArray();
		// add 1 Town 
		foreach(var ss in spaces)
			await ss.AddDefaultAsync( Human.Town, 1 );
	}

}
