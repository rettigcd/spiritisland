namespace SpiritIsland;

public class InvaderPhase {

	public static async Task ActAsync( GameState gs ) {

		// Blighted Island effect
		await gs.RunPreInvaderActions();

		// Fear
		await gs.Fear.ResolveActivatedCards();
		gs.CheckWinLoss();

		// Invaders Actions
		foreach(var slot in gs.InvaderDeck.ActiveSlots) {
			await slot.Execute( gs );
			gs.CheckWinLoss();
		}

		await gs.InvaderDeck.AdvanceAsync();

		await gs.RunPostInvaderActions();
	}

}
