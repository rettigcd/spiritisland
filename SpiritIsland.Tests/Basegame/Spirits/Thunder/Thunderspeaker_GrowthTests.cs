using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class Thunderspeaker_GrowthTests : GrowthTests{

		static Spirit InitSpirit() {
			return new Thunderspeaker {
				CardDrawer = new PowerProgression(
					PowerCard.For<VeilTheNightsHunt>(),
					PowerCard.For<ReachingGrasp>(),
					//PowerCard.For<WrapInWingsOfSunlight>(),      // Major
					PowerCard.For<Drought>(),
					PowerCard.For<ElementalBoon>()
				),
			};
		}

		public Thunderspeaker_GrowthTests():base( InitSpirit() ){}

		[Fact]
		public void ReclaimAnd2PowerCards() {
			// Growth Option 1 - Reclaim All, +2 Power cards
			Given_HalfOfPowercardsPlayed();

			When_StartingGrowth();

			User.Growth_SelectsOption("ReclaimAll / DrawPowerCard / DrawPowerCard");
//			User.Growth_ReclaimsAll();
			User.Growth_DrawsPowerCard();
			User.Growth_DrawsPowerCard();

			Assert_AllCardsAvailableToPlay( 6);
			Assert_HasEnergy(1);
		}

		[Theory]
		[InlineData( "3,5,8", "A3;A5" )]
		[InlineData( "3,4,8", "A3;A4" )]
		[InlineData( "4,8", "A4" )]
		[InlineData( "1,4,8", "A1;A4" )]
		public void TwoPresence( string initialDahanSquares, string expectedPresenseOptions ) {
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan
			Given_HasPresence( board[3] );
			//	 And: dahan on initial spot
			foreach(string s in initialDahanSquares.Split( ',' ))
				gameState.DahanOn( board[int.Parse( s )] ).Init(1);

			_ = When_Growing( 1 );

			User.Growth_PlacesEnergyPresence( expectedPresenseOptions );

			Assert_HasEnergy( 0 );

		}

		[Fact]
		public void PresenseAndEnergy() {
			// +1 presense within 1, +4 energy
			Given_HasPresence( board[1] );

			When_StartingGrowth();

			User.Growth_SelectsOption( "PlacePresence(1) / GainEnergy(4)" );
//			User.Growth_GainsEnergy();
			User.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );

			Assert.Equal(1,spirit.EnergyPerTurn);
			Assert_HasEnergy( 4+1 );

		}

		[Trait("Presence","EnergyTrack")]
		[Theory]
		[InlineDataAttribute(1,1,"")]
		[InlineDataAttribute(2,1,"1 air")]
		[InlineDataAttribute(3,2,"1 air")]
		[InlineDataAttribute(4,2, "1 fire,1 air" )]
		[InlineDataAttribute(5,2, "1 sun,1 fire,1 air" )]
		[InlineDataAttribute(6,3, "1 sun,1 fire,1 air" )]
		public Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			var fix = new ConfigurableTestFixture { Spirit = new Thunderspeaker() };
			return fix.VerifyEnergyTrack( revealedSpaces, expectedEnergyGrowth, elements );
		}

		[Trait("Presence","CardTrack")]
		[Theory]
		[InlineDataAttribute(1,1,0)]
		[InlineDataAttribute(2,2,0)]
		[InlineDataAttribute(3,2,0)]
		[InlineDataAttribute(4,3,0)]
		[InlineDataAttribute(5,3,1)]
		[InlineDataAttribute(6,3,1)]
		[InlineDataAttribute(7,4,1)]
		public async Task CardTrack(int revealedSpaces, int expectedCardPlayCount, int reclaimCount ){
			var fix = new ConfigurableTestFixture { Spirit = new Thunderspeaker() };
			await fix.VerifyCardTrack( revealedSpaces, expectedCardPlayCount, "" );
			fix.VerifyReclaim1Count( reclaimCount );
		}

	}

}
