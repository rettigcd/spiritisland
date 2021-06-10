using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		// immutable
		readonly PowerCard boonOfVigorCard = PowerCard.For<BoonOfVigor>();
		readonly PowerCard flashFloodsCard = PowerCard.For<FlashFloods>();

		FlashFloods FlashFloods => (FlashFloods)flashFloodsCard.GetAction(null).InnerAction;
		BoonOfVigor BoonOfVigor => (BoonOfVigor)boonOfVigorCard.GetAction(null).InnerAction;

		#region BoonOfVigor

		[Fact]
		public void BoonOfVigor_TargetSelf() {
			int energyBonus = BoonOfVigor.TargetSelf();
			Assert.Equal( 1, energyBonus );
		}

		[Theory]
		[InlineData( 0 )]
		[InlineData( 3 )]
		[InlineData( 10 )]
		public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {
			int energyBonus = BoonOfVigor.TargetOther( expectedEnergyBonus );
			Assert.Equal( expectedEnergyBonus, energyBonus );
		}

		[Fact]
		public void BoonOfVigor_Stats() {
			AssertCardStatus( boonOfVigorCard, 0, Speed.Fast, "SWP" );
		}

		#endregion BoonOfVigor

		#region FlashFloods

		[Fact]
		public void FlashFloods_Inland() {
			var land = new Space { IsCostal = false };
			int damage = FlashFloods.GetDamage( land );
			Assert.Equal( 1, damage );
		}

		[Fact]
		public void FlashFloods_Costal() {
			var land = new Space { IsCostal = true };
			int damage = FlashFloods.GetDamage( land );
			Assert.Equal( 2, damage );
		}

		[Fact]
		public void FlashFloods_Stats() {
			AssertCardStatus( flashFloodsCard, 2, Speed.Fast, "SW" );
		}

		#endregion FlashFloods


		[Fact]
		public void RiversBounty_Stats() {
			var card = new PowerCard(typeof(RiversBounty));
			AssertCardStatus( card, 0, Speed.Slow, "SWB" );
		}


		void AssertCardStatus( PowerCard card, int expectedCost, Speed expectedSpeed, string expectedElements ) {
			Assert.Equal( expectedCost, card.Cost );
			Assert.Equal( expectedSpeed, card.Speed );

//			Assert.Equal( expectedElements, card.Elements );

			var cardElements = card.Elements
				.Select(x=> Growth.GrowthTests.ElementChars[x]);
//				.OrderBy(x=>x);
			Assert.Equal( expectedElements, string.Join("",cardElements));

		}

	}

}



