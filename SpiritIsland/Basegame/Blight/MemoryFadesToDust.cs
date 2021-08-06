using SpiritIsland.Core;
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
				var engine = new ActionEngine(spirit,gs);
				IOption[] options = spirit.Presence.Distinct().Cast<IOption>()
					.Union( spirit.Hand.Union( spirit.DiscardPile ).Cast<IOption>() )
					.ToArray();
				var option = await engine.SelectOption("Select Power card to forget or presence to destroy.",options);
				if(option is Space space)
					spirit.Presence.Remove(space);
				else if(option is PowerCard card)
					spirit.Forget(card);
				else
					throw new ArgumentException("WTH? "+option.ToString()) ;
			}
		}

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard = 2 * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
		}

		public bool IslandIsBlighted {get;private set;}
	}
}
