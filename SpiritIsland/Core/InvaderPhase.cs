namespace SpiritIsland;

public class InvaderPhase {

	public static async Task ActAsync( GameState gs ) {

		// Blighted Island effect
		await gs.StartOfInvaderPhase.InvokeAsync(gs);

		// Fear
		await gs.Fear.Apply();

		// Ravage
		foreach( var card in gs.InvaderDeck.Ravage )
			await card.Ravage( gs );
		gs.CheckWinLoss();

		// Building
		foreach( var card in gs.InvaderDeck.Build)
			await card.Build( gs );

		// Exploring
		foreach(var card in gs.InvaderDeck.Explore)
			await card.Explore( gs );

		gs.InvaderDeck.Advance();
	}

}