
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	using Type = SpiritIsland.PowerCard; // avoid name conflict

	namespace Select {

		public class PowerCard : TypedDecision<Type> {

			public CardUse Use(Type card) => cardUses[card];
			readonly Dictionary<Type,CardUse> cardUses = new Dictionary<Type, CardUse>();

			public PowerCard(string prompt, CardUse cardUse, Type[] cardOptions, Present present ) 
				: base( prompt, cardOptions, present
			) {
				foreach(var option in cardOptions)
					cardUses.Add(option,cardUse);
			}

			public PowerCard(string prompt, IEnumerable<SingleCardUse> cardOptions, Present present ) 
				: base( prompt, cardOptions.Select(x=>x.Card), present
			) {
				foreach(var option in cardOptions)
					cardUses.Add(option.Card,option.Use);
			}

			public Type[] CardOptions => cardUses.Keys.ToArray();

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