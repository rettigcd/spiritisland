
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	namespace Decision {

		public class PickPowerCard : TypedDecision<SpiritIsland.PowerCard> {

			public CardUse Use(PowerCard card) => cardUses[card];
			readonly Dictionary<PowerCard,CardUse> cardUses = new Dictionary<PowerCard, CardUse>();

			public PickPowerCard(string prompt, CardUse cardUse, PowerCard[] cardOptions, Present present ) 
				: base( prompt, cardOptions, present
			) {
				AddCards(cardUse, cardOptions);
			}

			public void AddCards(CardUse cardUse, IEnumerable<PowerCard> cardOptions ) {
				foreach(var card in cardOptions)
					cardUses.Add(card,cardUse);
			}

			public PowerCard[] CardOptions => cardUses.Keys.ToArray();

		}

	}

	public enum CardUse { 
		AddToHand, 
		Buy, 
		Discard, // This is for Events
		Forget, 
		Gift, 
		Reclaim, 
		Replay, 
		Other
	};

}