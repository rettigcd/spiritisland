using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		protected SpiritCards_Tests(Spirit spirit ) {
			this.spirit = spirit;
			this.User = new VirtualUser( spirit );
		}

		protected GameState gameState;
		protected PowerCard card;
		protected Spirit spirit;
		protected VirtualUser User { get; }

		protected void Given_PurchasedCard(string cardName) {
			card = spirit.Hand.Single( c => c.Name == cardName );
			PlayCard();
		}

		protected void PlayCard() {
			spirit.PlayCard( card );
		}


		protected void Given_GameWithSpirits(params Spirit[] spirits) {
			gameState = new GameState( spirits ) {
				Island = spirits.Length == 0
					? new Island( Board.BuildBoardA() )
					: new Island( Board.BuildBoardA(), Board.BuildBoardB() )
			};
		}

		protected static void Given_PurchasedFakePowercards(Spirit otherSpirit, int expectedEnergyBonus) {
			for (int i = 0; i < expectedEnergyBonus; ++i) {
				var otherCard = PowerCard.For<SpiritIsland.Basegame.GiftOfLivingEnergy>();
				otherSpirit.InPlay.Add(otherCard);
				otherSpirit.AddActionFactory(otherCard);
			}
		}

		static protected void Assert_CardStatus( PowerCard card, int expectedCost, Phase expectedSpeed, string expectedElements ) {
			Assert.Equal( expectedCost, card.Cost );
			Assert.Equal( expectedSpeed, card.DisplaySpeed );

			var cardElements = card.Elements.BuildElementString();
			Assert.Equal( expectedElements, string.Join("",cardElements));

		}

		protected void Assert_CardIsReady( PowerCard card, Phase speed ) {
			Assert.Contains(card, spirit.GetAvailableActions(speed).OfType<PowerCard>().ToList());
		}

		protected void When_PlayingCard() {
			_ = card.ActivateAsync( spirit.BindMyPower( gameState ) );
		}

	}

}
