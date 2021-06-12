using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		// immutable
		readonly PowerCard flashFloodsCard = PowerCard.For<FlashFloods>();

		FlashFloods FlashFloods => (FlashFloods)flashFloodsCard.Bind(null,null);

		#region BoonOfVigor

		[Fact]
		public void BoonOfVigor_TargetSelf() {

			// Given: River
			var spirit = new RiverSurges();
			//   And: a game
			var gameState = new GameState();
			//   And: Purchased Boon of Vigor
			var card = spirit.AvailableCards.Single(c=>c.Name == BoonOfVigor.Name);
			spirit.BuyAvailableCards(card);
			//   And: card is fast (ready to play now)
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList());

			// When: targetting self
			var action = (BoonOfVigor)card.Bind(spirit,gameState);
			action.Target = spirit;
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(1,spirit.Energy);

		}

		[Theory]
		[InlineData( 0 )]
		[InlineData( 3 )]
		[InlineData( 10 )]
		public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

			// Given: River
			var spirit = new RiverSurges();

			//   And: a second spirit
			var other = new Lightning();
			//  That: purchase N cards
			for(int i=0;i<expectedEnergyBonus;++i){
				var otherCard = new PowerCard("Fake-"+i,0,Speed.Slow);
				other.ActiveCards.Add(otherCard);
				other.UnresolvedActions.Add(otherCard);
			}

			//   And: a game
			var gameState = new GameState();

			//   And: Purchased Boon of Vigor
			var card = spirit.AvailableCards.Single(c=>c.Name == BoonOfVigor.Name);
			spirit.BuyAvailableCards(card);

			//   And: card is fast (ready to play now)
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList());

			// When: targetting other
			var action = (BoonOfVigor)card.Bind(spirit,gameState);
			action.Target = other;
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(expectedEnergyBonus,other.Energy);

		}

		[Fact]
		public void BoonOfVigor_Stats() {
			AssertCardStatus( PowerCard.For<BoonOfVigor>(), 0, Speed.Fast, "SWP" );
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
			var card = PowerCard.For<RiversBounty>();
			AssertCardStatus( card, 0, Speed.Slow, "SWB" );
		}


		void AssertCardStatus( PowerCard card, int expectedCost, Speed expectedSpeed, string expectedElements ) {
			Assert.Equal( expectedCost, card.Cost );
			Assert.Equal( expectedSpeed, card.Speed );

			var cardElements = card.Elements
				.Select(x=> Growth.GrowthTests.ElementChars[x]);
			Assert.Equal( expectedElements, string.Join("",cardElements));

		}

	}

}



