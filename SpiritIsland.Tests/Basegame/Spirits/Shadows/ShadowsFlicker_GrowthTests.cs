using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.ShadowsNS {

	public class ShadowsFlicker_GrowthTests : GrowthTests {

		public ShadowsFlicker_GrowthTests():base( new Shadows().UsePowerProgression() ){
		}

		[Fact]
		public void Reclaim_PowerCard(){
			// reclaim, gain power Card
			Given_HalfOfPowercardsPlayed();

			_ = When_Growing( 0 );

			User.Growth_DrawsPowerCard();
			User.Growth_ReclaimsAll();

			Assert.Equal(5,this.spirit.Hand.Count); // drew 1 card
		}

		[Fact]
		public void PowerAndPresence(){
			// gain power card, add a presense range 1
			Given_HasPresence( board[1] );

			_ = When_Growing(1);

			User.Growth_DrawsPowerCard();
			User.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );

			Assert.Equal(5,this.spirit.Hand.Count); // drew 1 card
		}

		[Fact]
		public void PresenceAndEnergy(){
			// add a presence withing 3, +3 energy
			Given_HasPresence( board[3] );

			When_StartingGrowth();

			User.Growth_SelectsOption( "PlacePresence(3) / GainEnergy(3)" );
			User.Growth_GainsEnergy();
			User.Growth_PlacesEnergyPresence( "A1;A2;A3;A4;A5;A6;A7;A8" );

			Assert_HasEnergy(3+1); // 1 from energy track
		}

		[Trait("Presence","EnergyTrack")]
		[Theory]
		[InlineDataAttribute(1,0)]
		[InlineDataAttribute(2,1)]
		[InlineDataAttribute(3,3)]
		[InlineDataAttribute(4,4)]
		[InlineDataAttribute(5,5)]
		[InlineDataAttribute(6,6)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth ){
			// energy:	0 1 3 4 5 6
			spirit.Presence.Energy.SetRevealedCount( revealedSpaces );
			Assert_PresenceTracksAre(expectedEnergyGrowth,1);
		}

		[Trait("Presence","CardTrack")]
		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,2)]
		[InlineDataAttribute(3,3)]
		[InlineDataAttribute(4,3)]
		[InlineDataAttribute(5,4)]
		[InlineDataAttribute(6,5)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount){
			// cards:	1 2 3 3 4 5
			spirit.Presence.CardPlays.SetRevealedCount( revealedSpaces );
			Assert_PresenceTracksAre(0,expectedCardPlayCount);

		}


	}
}
