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
			foreach(var spirit in gs.Spirits) {
				IOption[] options = spirit.Presence.Spaces.Cast<IOption>()
					.Union( spirit.Hand.Union( spirit.DiscardPile ).Cast<IOption>() )
					.ToArray();
				IOption option = await spirit.Select("Select Power card to forget or presence to destroy.",options);
				if(option is Space space)
					spirit.Presence.Destroy(space);
				else if(option is PowerCard card)
					spirit.Forget(card);
				else
					throw new ArgumentException("WTH? "+option.ToString()) ;
			}
		}

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard = 2 * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
		}

		public bool IslandIsBlighted {get;set;}
	}
}
