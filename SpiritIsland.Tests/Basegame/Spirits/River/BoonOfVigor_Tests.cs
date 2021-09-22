using SpiritIsland.Basegame;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class BoonOfVigor_Tests : SpiritCards_Tests {

		public BoonOfVigor_Tests():base( new RiverSurges() ) { }

		[Fact]
		public void BoonOfVigor_TargetSelf() {

			Given_GameWithSpirits( spirit );

			var card = Given_PurchasedCard( BoonOfVigor.Name );
			Assert_CardIsReady( card, Speed.Fast );

			// When: targetting self
			card.ActivateAsync( spirit, gameState );

			User.TargetsSpirit( RiverSurges.Name );

			User.Assert_Done();

			// Then: received 1 energy
			Assert.Equal( 1, spirit.Energy );

		}

		[Theory]
		[InlineData( 0 )]
		[InlineData( 3 )]
		[InlineData( 10 )]
		public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

			Given_GameWithSpirits(spirit, new LightningsSwiftStrike());

			//  That: purchase N cards
			var otherSpirit = gameState.Spirits[1];
			Given_PurchasedFakePowercards(otherSpirit, expectedEnergyBonus);

			//   And: Purchased Boon of Vigor
			PowerCard card = Given_PurchasedCard(BoonOfVigor.Name);
			Assert_CardIsReady(card,Speed.Fast);

			// When: targetting other spirit
			card.ActivateAsync( spirit, gameState );
			
			User.TargetsSpirit("River Surges in Sunlight,(Lightning's Swift Strike)");

			User.Assert_Done();

			// Then: received 1 energy
			Assert.Equal(expectedEnergyBonus, otherSpirit.Energy);

		}

		[Fact]
		public void BoonOfVigor_Stats() {
			Assert_CardStatus( PowerCard.For<BoonOfVigor>(), 0, Speed.Fast, "SWP" );
		}

	}

}



