
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
				foreach(var option in cardOptions)
					cardUses.Add(option,cardUse);
			}

			public PickPowerCard(string prompt, IEnumerable<SingleCardUse> cardOptions, Present present ) 
				: base( prompt, cardOptions.Select(x=>x.Card), present
			) {
				foreach(var option in cardOptions)
					cardUses.Add(option.Card,option.Use);
			}

			public PowerCard[] CardOptions => cardUses.Keys.ToArray();

		}

	}

	public class SingleCardUse {
		public CardUse Use { get; set; }
		public PowerCard Card { get; set; }
		static public IEnumerable<SingleCardUse> GenerateUses(CardUse use, IEnumerable<PowerCard> cards ) {
			foreach(var card in cards)
				yield return new SingleCardUse {  Card = card, Use = use };
		}

	}

	public enum CardUse { 
		AddToHand, 
		Discard, // This is for Events
		Forget, 
		Gift, 
		Play, 
		Reclaim, 
		Repeat, 
		Other
	};

}