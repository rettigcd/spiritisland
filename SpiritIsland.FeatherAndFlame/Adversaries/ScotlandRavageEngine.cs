namespace SpiritIsland.FeatherAndFlame;

public class ScotlandRavageEngine : RavageEngine {
	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {

		await base.ActivateCard( card, gameState );
		// After the Ravage step,
		FuelInwardGrowth( card, gameState );
	}

	static void FuelInwardGrowth( InvaderCard card, GameState gameState ) {
		var spaces = gameState.AllSpaces
			// to each Inland land
			.Where( ss => !ss.Space.IsOcean && !ss.Space.IsCoastal )
			// that matches a Ravage card
			.Where( card.MatchesCard )
			// and is within 1 of town/ city
			.Where( ss => ss.HasAny( Invader.Town_City ) || ss.Adjacent.Any( adj => adj.HasAny( Invader.Town_City ) ) ) // !!! What about Isolate?, maybe we need a 2nd .Adjacent called .Adjacent_NotIsolated
			.ToArray();
		// add 1 Town 
		foreach(var ss in spaces)
			ss.AdjustDefault( Invader.Town, 1 ); // !! if we had access to the Ravage Action, we should .Bind() to that, and call .AddDefault
	}
}
