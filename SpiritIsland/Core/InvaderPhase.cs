﻿namespace SpiritIsland;

public class InvaderPhase {

	public static async Task ActAsync( GameState gs ) {

		// Blighted Island effect
		await gs.StartOfInvaderPhase.InvokeAsync( gs );

		// Fear
		await gs.Fear.Apply();

		foreach(var slot in gs.InvaderDeck.Slots) {
			await slot.Execute(gs);
			gs.CheckWinLoss();
		}

		gs.InvaderDeck.Advance();
	}
}