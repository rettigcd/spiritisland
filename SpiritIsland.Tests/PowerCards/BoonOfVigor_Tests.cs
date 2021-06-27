using SpiritIsland.Base;
using SpiritIsland.Core;
using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.PowerCards {
	public class BoonOfVigor_Tests : SpiritCards_Tests {
 		#region BoonOfVigor

		[Fact]
		public void BoonOfVigor_TargetSelf() {

			Given_GameWithSpirits( new RiverSurges() );

			var card = Given_PurchasedCard(BoonOfVigor.Name);
			Assert_CardIsReady(card);

			// When: targetting self
			action = (BoonOfVigor)card.Bind(spirit, gameState);
			Assert.True(action.IsResolved);
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(1, spirit.Energy);

		}

		[Theory]
		[InlineData( 0 )]
		[InlineData( 3 )]
		[InlineData( 10 )]
		public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

			Given_GameWithSpirits(new RiverSurges(), new Lightning());

			//  That: purchase N cards
			var otherSpirit = gameState.Spirits[1];
			Given_PurchasedFakePowercards(otherSpirit, expectedEnergyBonus);

			//   And: Purchased Boon of Vigor
			PowerCard card = Given_PurchasedCard(BoonOfVigor.Name);
			Assert_CardIsReady(card);

			// When: targetting other spirit
			action = card.Bind(spirit, gameState);
			When_TargettingSpirit( otherSpirit );

			Assert.True(action.IsResolved);
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(expectedEnergyBonus, otherSpirit.Energy);

		}

		void When_TargettingSpirit(Spirit otherSpirit) {
			Assert.False(action.IsResolved);
			Assert.Equal(gameState.Spirits.Select(x => x.Text).OrderBy(x => x).Join(",")
				, action.Options.Select(x => x.Text).OrderBy(x => x).Join(",")
			);
			action.Select(otherSpirit);
		}

		[Fact]
		public void BoonOfVigor_Stats() {
			Assert_CardStatus( PowerCard.For<BoonOfVigor>(), 0, Speed.Fast, "SWP" );
		}

		#endregion BoonOfVigor

	}

}



