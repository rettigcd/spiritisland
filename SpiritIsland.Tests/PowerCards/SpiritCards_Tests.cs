using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		protected Spirit spirit;
		protected GameState gameState;
		protected IAction action;
		protected PowerCard card;

		protected PowerCard Given_PurchasedCard(string cardName) {
			var card = spirit.AvailableCards.Single(c => c.Name == cardName);
			spirit.BuyAvailableCards(card);
			return card;
		}

		protected void Given_GameWithSpirits(params Spirit[] spirits) {
			spirit = spirits[0];
			gameState = new GameState(spirits);
		}

		protected void Given_PurchasedFakePowercards(Spirit otherSpirit, int expectedEnergyBonus) {
			for (int i = 0; i < expectedEnergyBonus; ++i) {
				var otherCard = new PowerCard("Fake-" + i, 0, Speed.Slow);
				otherSpirit.ActiveCards.Add(otherCard);
				otherSpirit.UnresolvedActions.Add(otherCard);
			}
		}

		protected void Assert_CardStatus( PowerCard card, int expectedCost, Speed expectedSpeed, string expectedElements ) {
			Assert.Equal( expectedCost, card.Cost );
			Assert.Equal( expectedSpeed, card.Speed );

			var cardElements = card.Elements
				.Select(x=> Growth.GrowthTests.ElementChars[x]);
			Assert.Equal( expectedElements, string.Join("",cardElements));

		}

		protected void Assert_CardIsReady( PowerCard card ) {
			Assert.Contains(card, spirit.UnresolvedActions.OfType<PowerCard>().ToList());
		}

		protected void When_PlayingCard() {
			action = card.Bind( spirit, gameState );
		}

		protected void Assert_Options( params string[] expected ) {
			if(action==null) throw new System.InvalidOperationException("action is null");
			Assert.Equal(
				expected.OrderBy(x=>x).Join(",")
				,action.Options.Select(s=>s.Text).OrderBy(x=>x).Join(",")
			);
		}

		protected void Assert_Options( IEnumerable<IOption> expected ){
			string expectedStr = expected.Select(s=>s.Text).OrderBy(x=>x).Join(",");
			string actualOptions = action.Options.Select(s=>s.Text).OrderBy(x=>x).Join(",");
			Assert.Equal( expectedStr, actualOptions);
		}

	}

}
