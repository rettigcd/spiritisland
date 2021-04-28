using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {

	[TestFixture]
	public class VitalStrength_GrowthTests : GrowthTests {

		[SetUp]
		public void SetUp_VitalStrength() => Given_SpiritIs(new VitalStrength());

		[Test]
		public void ReclaimAndPresence(){
			// reclaim, +1 presense range 2
			When_Growing( 0 );

			this.Assert_AllCardsAvailableToPlay();
			Assert_AddPresense_Range2();

		}

		[Test]
		public void PowercardAndPresence() {
			// +1 power card, +1 presense range 0
			spirit.InitPresence( board[4] );

			When_Growing( 1, "A4" );

			Assert_GainPowercard( 1 );
			Assert_NewPresenceOptions();
		}

		[Test]
		public void PresenseAndEnergy(){
			// +1 presence range 1, +2 energy
			When_Growing( 2 );
			Assert_Add1Presence_Range1();
			Assert_GainEnergy(2);
		}

	}
}
