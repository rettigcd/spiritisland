using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		protected Spirit spirit;
		protected GameState gameState;
		protected BaseAction action;
		protected PowerCard card;

		protected PowerCard Given_PurchasedCard(string cardName) {
			var card = spirit.Hand.Single(c => c.Name == cardName);
			spirit.PurchaseAvailableCards(card);
			return card;
		}

		protected void Given_GameWithSpirits(params Spirit[] spirits) {
			spirit = spirits[0];
			gameState = new GameState(spirits);
		}

		protected static void Given_PurchasedFakePowercards(Spirit otherSpirit, int expectedEnergyBonus) {
			for (int i = 0; i < expectedEnergyBonus; ++i) {
				var otherCard = new NullPowerCard("Fake-" + i, 0, Speed.Slow);
				otherSpirit.PurchasedCards.Add(otherCard);
				otherSpirit.AddActionFactory(otherCard);
			}
		}

		static protected void Assert_CardStatus( PowerCard card, int expectedCost, Speed expectedSpeed, string expectedElements ) {
			Assert.Equal( expectedCost, card.Cost );
			Assert.Equal( expectedSpeed, card.Speed );

			var cardElements = card.Elements
				.Select(x=> GrowthTests.ElementChars[x]);
			Assert.Equal( expectedElements, string.Join("",cardElements));

		}

		protected void Assert_CardIsReady( PowerCard card, Speed speed ) {
			Assert.Contains(card, spirit.GetAvailableActions(speed).OfType<PowerCard>().ToList());
		}

		protected void When_PlayingCard() {
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;
		}

		protected void Assert_Options( params string[] expected ) {
			if(action==null) throw new System.InvalidOperationException("action is null");
			var current = action.GetCurrent();
			Assert.Equal(
				expected.OrderBy(x=>x).Join(",")
				,current.Options.Select(s=>s.Text).OrderBy(x=>x).Join(",")
			);
		}

		protected void Assert_Options( IEnumerable<IOption> expected, params IOption[] plus ){
			expected = expected.Union(plus);
			string expectedStr = expected.Select(s=>s.Text).OrderBy(x=>x).Join(",");
			string actualOptions = action.GetCurrent().Options.Select(s=>s.Text).OrderBy(x=>x).Join(",");
			Assert.Equal( expectedStr, actualOptions);
		}

	}

}
