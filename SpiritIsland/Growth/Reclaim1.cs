using System.Linq;

namespace SpiritIsland {

	public class Reclaim1 : GrowthAction {

		public PowerCard Card {get; set;}

		public override void Apply( Spirit ps ) {
			if( ps.PlayedCards.Contains(Card) ){
				ps.PlayedCards.Remove(Card);
				ps.AvailableCards.Add(Card);
			}
			Card = null; // ensure it must be set each time.
		}

		static public IResolver Resolve(PowerCard card) => new Resolver(card);

		class Resolver : IResolver {

			readonly PowerCard card;

			public Resolver(PowerCard card){
				this.card = card;
			}

			public void Apply( GrowthOption option ) {
				var action = option.GrowthActions.OfType<Reclaim1>().SingleOrDefault();
				if(action != null)
					action.Card = card;
			}

		}


	}

}
