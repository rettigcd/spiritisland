
namespace SpiritIsland {

	namespace Decision {

		public class PickPowerCard : TypedDecision<SpiritIsland.PowerCard> {

			public CardUse Use { get; }

			public PickPowerCard(string prompt, PowerCard[] cardOptions, CardUse cardUse,  Present present ) : base( prompt, cardOptions, present ) {
				Use = cardUse;
				this.CardOptions = cardOptions;
			}

			public PowerCard[] CardOptions { get; }

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