using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// Plug into the DrawPowerCard API on the spirit
	public class PowerProgression : IPowerCardDrawer {
		readonly List<PowerCard> cards;
		public PowerProgression( params PowerCard[] cards ) {
			this.cards = cards.ToList();
		}

		public Task Draw( ActionEngine engine ) {
			return Take( engine, cards.First() );
		}

		public Task DrawMajor( ActionEngine engine ) {
			return Take( engine, cards.First( c => c.PowerType == PowerType.Major ) );
		}

		public Task DrawMinor( ActionEngine engine ) {
			return Take( engine, cards.First( c => c.PowerType == PowerType.Minor ) );
		}

		async Task Take( ActionEngine engine, PowerCard newCard ) {
			var (spirit,_)=engine;
			cards.Remove( newCard );

			spirit.RegisterNewCard( newCard );
			if(newCard.PowerType == PowerType.Major)
				await engine.ForgetPowerCard();
//				spirit.AddActionFactory( new ForgetPowerCard() ); // !!! do this right now, don't make it another factory
		}

	}


}
