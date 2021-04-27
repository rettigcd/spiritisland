using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {

	[TestFixture]
	public class Thunderspeaker_GrowthTests : GrowthTests{

		[SetUp]
		public void SetUp_ThunderSpeaker(){
			Given_SpiritIs( new ThunderSpeaker() );
		}

		[Test]
		public void ReclaimAnd2PowerCards() {
			// Growth Option 1 - Reclaim All, +2 Power cards

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert.That( playerState.PowerCardsToDraw, Is.EqualTo( 2 ) );
			Assert_GainEnergy(0);
		}

		[TestCase( "3,5,8", "A3A3;A3A5;A5A5;A5A8" )]
		[TestCase( "3,4,8", "A3A3;A3A4;A4A4;A4A8" )]
		[TestCase( "4,8", "A4A4;A4A8" )]
		[TestCase( "1,4,8", "A1A1;A1A4;A4A4;A4A8" )]
		public void TwoPresence( string initialDahanSquares, string expectedPresenseOptions ) {
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan

			When_Growing( 1 );

			// Then: 
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan
			playerState.Presence.Add( board.spaces[3] );
			//	 And: dahan on initial spot
			foreach(string s in initialDahanSquares.Split( ',' ))
				gameState.AddDahan( board.spaces[int.Parse( s )] );
			Assert_NewPresenceOptions( expectedPresenseOptions );

			//  And: Energy didn't change
			Assert_GainEnergy( 0 );

		}

		[Test]
		public void PresenseAndEnergy() {
			// +1 presense within 1, +4 energy

			When_Growing( 2 );

			Assert_Add1Presence_Range1();
			Assert_GainEnergy( 4 );

		}

	}

}
