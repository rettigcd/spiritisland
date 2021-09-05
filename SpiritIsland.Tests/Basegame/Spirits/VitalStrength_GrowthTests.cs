using System.Collections.Generic;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.VitalStrengthNS {

	public class VitalStrength_GrowthTests : GrowthTests {

		public VitalStrength_GrowthTests():base( new VitalStrength().UsePowerProgression() ){}

		[Fact]
		public void ReclaimAndPresence(){
			// reclaim, +1 presense range 2
			Given_HalfOfPowercardsPlayed();
			Given_HasPresence( board[3] );

			When_Growing( 0 );
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_ReclaimAll();

			Resolve_PlacePresence( "A1;A2;A3;A4;A5", spirit.Presence.Energy.Next );

			this.Assert_AllCardsAvailableToPlay();

		}

		[Fact]
		public void PowercardAndPresence() {
			// +1 power card, +1 presense range 0
			Given_HasPresence( board[4] );

			When_Growing( 1 );
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_DrawPowerCard();

			Resolve_PlacePresence( "A4", spirit.Presence.Energy.Next );

			Assert.Equal(5,spirit.Hand.Count);
		}

		[Fact]
		public void PresenseAndEnergy(){
			// +1 presence range 1, +2 energy
			Given_HasPresence( board[1] );

			When_StartingGrowth();
			spirit.Action.Choose( "GainEnergy(2) / PlacePresence(1)" );

			spirit.Activate_GainEnergy();

			Resolve_PlacePresence( "A1;A2;A4;A5;A6",spirit.Presence.Energy.Next);
			Assert.Equal(3,spirit.EnergyPerTurn);
			Assert_HasEnergy(3+2);
		}

		[Theory]
		[InlineDataAttribute(1,2)]
		[InlineDataAttribute(2,3)]
		[InlineDataAttribute(3,4)]
		[InlineDataAttribute(4,6)]
		[InlineDataAttribute(5,7)]
		[InlineDataAttribute(6,8)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth) {
			// energy:	2 3 4 6 7 8
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
		}

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,1)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,2)]
		[InlineDataAttribute(5,3)]
		[InlineDataAttribute(6,4)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount){
			//	card:  1 1 2 2 3 4
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_CardTrackIs( expectedCardPlayCount );
		}




	}

}
