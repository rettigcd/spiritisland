using SpiritIsland;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class MemoryFadesToDust : IBlightCard {

		// +4 more blight
		public void OnBlightDepleated( GameState gs ) {
			if(IslandIsBlighted) GameOverException.Lose();
			IslandIsBlighted = true;
			gs.blightOnCard += 4 * gs.Spirits.Length;
		}

		public async Task OnStartOfInvaders( GameState gs ) {
			if(!IslandIsBlighted) return;
			// Forgets a Power or destorys a presence
			foreach(var spirit in gs.Spirits)
				await new SpiritGameStateCtx( spirit, gs, Cause.Blight ).SelectActionOption(
					"BLIGHT: Memory Fades to Dust",
					new ActionOption("Destroy Presence",()=>gs.Destroy1PresenceFromBlightCard(spirit)),
					new ActionOption("Forget Power card", () => spirit.ForgetPowerCard() )
				);

		}

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard = 2 * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
		}

		public bool IslandIsBlighted {get;set;}
	}
}
