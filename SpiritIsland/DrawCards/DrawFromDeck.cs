using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class DrawFromDeck : IPowerCardDrawer {

		public async Task<PowerCard> Draw( Spirit spirit, GameState gs, Func<List<PowerCard>, Task> handleNotUsed ) {
			return await spirit.UserSelectsFirstText( "Which type do you wish to draw", "minor", "major" )
				? await DrawMinor( spirit, gs, handleNotUsed )
				: await DrawMajor( spirit, gs, handleNotUsed );
		}

		public async Task<PowerCard> DrawMajor( Spirit spirit, GameState gameState, Func<List<PowerCard>, Task> handleNotUsed, bool forgetCard = true, int numberToDraw = 4 ) {
			var card = await DrawInner(spirit, gameState.MajorCards, numberToDraw, handleNotUsed );
			if(forgetCard)
				await spirit.ForgetPowerCard();
			return card;
		}

		public Task<PowerCard> DrawMinor( Spirit spirit, GameState gameState, Func<List<PowerCard>, Task> handleNotUsed, int numberToDraw = 4 )
			=> DrawInner( spirit, gameState.MinorCards, numberToDraw, handleNotUsed );

		static async Task<PowerCard> DrawInner( Spirit spirit, PowerCardDeck deck, int numberToDraw, Func<List<PowerCard>, Task> handleNotUsed ) {
			List<PowerCard> flipped = deck.Flip(numberToDraw);

			var selected = await TakeCard( spirit, flipped );

			if(handleNotUsed != null)
				await handleNotUsed( flipped );

			deck.Discard( flipped );
			return selected;
		}

		public static async Task<PowerCard> TakeCard( Spirit spirit, List<PowerCard> flipped ) {
			string powerType = flipped.Select(x=>x.PowerType.ToString() ).Distinct().Join("/");
			var selectedCard = await spirit.SelectPowerCard( $"Select {powerType} Power Card", flipped, CardUse.AddToHand, Present.Always );
			spirit.Hand.Add( selectedCard );
			flipped.Remove( selectedCard );
			return selectedCard;
		}

	}

}