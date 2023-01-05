namespace SpiritIsland;

public class InvaderPhase {

	public static async Task ActAsync( GameState gs ) {

		// Blighted Island effect
		await gs.StartOfInvaderPhase.InvokeAsync( gs );

		// Fear
		await gs.Fear.Apply();
		gs.CheckWinLoss();

		foreach(var slot in gs.InvaderDeck.ActiveSlots) {
			await slot.Execute(gs);
			gs.CheckWinLoss();
		}

		gs.InvaderDeck.Advance();
	}
}