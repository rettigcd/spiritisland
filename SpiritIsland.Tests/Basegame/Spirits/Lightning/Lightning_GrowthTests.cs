using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.Lightning {

	public class Lightning_GrowthTests : GrowthTests{

		public Lightning_GrowthTests()
			:base(new LightningsSwiftStrike().UsePowerProgression())
		{}

		[Fact]
		public void Reclaim_Power_Energy() {
			// * reclaim, +1 power card, +1 energy

			Given_HalfOfPowercardsPlayed();
			When_StartingGrowth();

			User.SelectsGrowthOption( "ReclaimAll / DrawPowerCard / GainEnergy(1)" );
			User.ReclaimsAll();
			User.GainsEnergy();
			User.DrawsPowerCard();

			Assert_AllCardsAvailableToPlay( 5 ); // drew a power card
			Assert_HasEnergy( 1 + 1 ); // 1 from energy track

		}

		[Fact]
		public void Presense_Energy() {
			// +1 presense range 1, +3 energy

			Given_HasPresence( board[1] );

			When_StartingGrowth();

			User.SelectsGrowthOption( "GainEnergy(3) / PlacePresence(1)" );
			User.GainsEnergy();
			User.PlacesEnergyPresence( "A1;A2;A4;A5;A6" );

			Assert.Equal(1,spirit.EnergyPerTurn);
			Assert_HasEnergy( 3 + 1 ); // 1 from energy track
		}

		[Fact]
		public void TwoPresence(){
			// +1 presense range 2, +1 prsense range 0
			Given_HasPresence( board[3] ); 

			When_Growing( 1 );

			User.PlacesEnergyPresence( "A1;A2;A3;A4;A5" );

			Assert_HasEnergy( 0 );

		}

		[Trait("Presence","EnergyTrack")]
		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,1)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,2)]
		[InlineDataAttribute(5,3)]
		[InlineDataAttribute(6,4)]
		[InlineDataAttribute(7,4)]
		[InlineDataAttribute(8,5)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth) {
			// energy: 1 1 2 2 3 4 4 5
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
		}

		[Trait("Presence","CardTrack")]
		[Theory]
		[InlineDataAttribute(1,2)]
		[InlineDataAttribute(2,3)]
		[InlineDataAttribute(3,4)]
		[InlineDataAttribute(4,5)]
		[InlineDataAttribute(5,6)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount){
			// card:   2 3 4 5 6
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_CardTrackIs( expectedCardPlayCount );
		}


	}

}
