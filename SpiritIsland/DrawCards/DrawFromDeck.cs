using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class DrawFromDeck : IPowerCardDrawer {

		public async Task<DrawCardResult> Draw( Spirit spirit, GameState gs ) {
			PowerType powerType = await SelectPowerCardType( spirit );
			return powerType == PowerType.Minor
				? await DrawMinor( spirit, gs, 4, 1 )
				: await DrawMajor( spirit, gs, 4, 1 );
		}

		public static async Task<PowerType> SelectPowerCardType( Spirit spirit ) {
			return await spirit.Action.Decision( new Decision.DeckToDrawFrom( PowerType.Minor, PowerType.Major ) );
		}

		public Task<DrawCardResult> DrawMajor( Spirit spirit, GameState gameState, int numberToDraw, int numberToKeep ) {
			return DrawInner(spirit, gameState.MajorCards, numberToDraw, numberToKeep );
		}

		public Task<DrawCardResult> DrawMinor( Spirit spirit, GameState gameState, int numberToDraw, int numberToKeep )
			=> DrawInner( spirit, gameState.MinorCards, numberToDraw, numberToKeep );

		static async Task<DrawCardResult> DrawInner( Spirit spirit, PowerCardDeck deck, int numberToDraw, int numberToKeep ) {
			List<PowerCard> candidates = deck.Flip(numberToDraw);

			var selectedCards = new List<PowerCard>();
			while(numberToKeep-- > 0) {
				var selected = await TakeCard( spirit, candidates );
				selectedCards.Add( selected );
			}

			deck.Discard( candidates );
			return new DrawCardResult( selectedCards[0].PowerType ){
				 SelectedCards = selectedCards.ToArray(),
				 Rejected = candidates
			};
		}

		public static async Task<PowerCard> TakeCard( Spirit spirit, List<PowerCard> flipped ) {
			string powerType = flipped.Select(x=>x.PowerType.Text ).Distinct().Join("/");
			var selectedCard = await spirit.SelectPowerCard( $"Select {powerType} Power Card", flipped, CardUse.AddToHand, Present.Always );
			spirit.Hand.Add( selectedCard );
			flipped.Remove( selectedCard );
			return selectedCard;
		}

	}

}