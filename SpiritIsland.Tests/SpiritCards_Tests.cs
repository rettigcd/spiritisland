using NUnit.Framework;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		// immutable
		readonly BoonOfVigor boonOfVigor = new BoonOfVigor();
		readonly FlashFloods flashFloods = new FlashFloods();


		[SetUp]
		public void Setup() {
		}

		#region BoonOfVigor

		[Test]
		public void BoonOfVigor_TargetSelf() {
			int energyBonus = boonOfVigor.TargetSelf();
			Assert.That( energyBonus, Is.EqualTo(1) );
		}

		[TestCase(0)]
		[TestCase(3)]
		[TestCase(10)]
		public void BoonOfVigor_TargetOther(int powerCardsPlayed) {
			int energyBonus = boonOfVigor.TargetOther(powerCardsPlayed);
			Assert.That( energyBonus, Is.EqualTo(powerCardsPlayed) );
		}

		[Test]
		public void BoonOfVigor_Stats(){
			AssertCardStatus( boonOfVigor, 0, Speed.Fast, "SWP");
		}

		#endregion BoonOfVigor

		#region FlashFloods

		[Test]
		public void FlashFloods_Inland(){
			var land = new BoardSpace{ IsCostal = false };
			int damage = flashFloods.GetDamage(land);
			Assert.That(damage, Is.EqualTo(1));
		}

		[Test]
		public void FlashFloods_Costal(){
			var land = new BoardSpace{ IsCostal = true };
			int damage = flashFloods.GetDamage(land);
			Assert.That(damage, Is.EqualTo(2));
		}

		[Test]
		public void FlashFloods_Stats() {
			AssertCardStatus( flashFloods, 2, Speed.Fast, "SW");
		}

		#endregion FlashFloods


		[Test]
		public void RiversBounty_Stats(){
			var card = new RiversBounty();
			AssertCardStatus(card, 0, Speed.Slow, "SWB");
		}


		void AssertCardStatus( PowerCard card, int expectedCost, Speed expectedSpeed, string expectedElements ) {
			Assert.That( card.Cost, Is.EqualTo( expectedCost ) );
			Assert.That( card.Speed, Is.EqualTo( expectedSpeed ) );
			Assert.That( card.Elements, Is.EqualTo( expectedElements ) );
		}

	}

	public class RiversBounty : PowerCard {
		public override Speed Speed => Speed.Slow;
		public override string Elements => "SWB";
	}

}



