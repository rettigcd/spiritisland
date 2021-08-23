using System.Linq;
using SpiritIsland.Basegame;
using SpiritIsland;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {
	public class BoonOfVigor_Tests : SpiritCards_Tests {

		[Fact]
		public void BoonOfVigor_TargetSelf() {

			Given_GameWithSpirits( new RiverSurges() );

			var card = Given_PurchasedCard(BoonOfVigor.Name);
			Assert_CardIsReady(card,Speed.Fast);

			// When: targetting self
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;
			Assert.True(action.IsResolved);

			// Then: received 1 energy
			Assert.Equal(1, spirit.Energy);

		}

		[Theory]
		[InlineData( 0 )]
		[InlineData( 3 )]
		[InlineData( 10 )]
		public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

			Given_GameWithSpirits(new RiverSurges(), new LightningsSwiftStrike());

			//  That: purchase N cards
			var otherSpirit = gameState.Spirits[1];
			Given_PurchasedFakePowercards(otherSpirit, expectedEnergyBonus);

			//   And: Purchased Boon of Vigor
			PowerCard card = Given_PurchasedCard(BoonOfVigor.Name);
			Assert_CardIsReady(card,Speed.Fast);

			// When: targetting other spirit
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;
			When_TargettingSpirit( otherSpirit );

			Assert.True(action.IsResolved);

			// Then: received 1 energy
			Assert.Equal(expectedEnergyBonus, otherSpirit.Energy);

		}

		void When_TargettingSpirit(Spirit otherSpirit) {
			Assert.False(action.IsResolved);
			Assert.Equal(gameState.Spirits.Select(x => x.Text).OrderBy(x => x).Join(",")
				, action.Current.Options.Select(x => x.Text).OrderBy(x => x).Join(",")
			);
			action.Choose(otherSpirit);
		}

		[Fact]
		public void BoonOfVigor_Stats() {
			Assert_CardStatus( PowerCard.For<BoonOfVigor>(), 0, Speed.Fast, "SWP" );
		}

	}

}



