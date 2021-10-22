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
					new ActionOption("Destroy Presence",()=>gs.Destroy1PresenceFromBlight(spirit)),
					new ActionOption("Forget Power card", async () => {
						var card = await spirit.Select<PowerCard>( 
							"Select card to forget.",
							spirit.Hand.Union( spirit.DiscardPile ).ToArray(),
							Present.Always
						);
						spirit.Forget( card );
					} )
				);

		}

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard = 2 * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
		}

		public bool IslandIsBlighted {get;set;}
	}
}
