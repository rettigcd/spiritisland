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
			spirit.Action.Choose( "ReclaimAll / DrawPowerCard / DrawPowerCard" );
			spirit.Activate_ReclaimAll();
			spirit.Activate_DrawPowerCard();
			spirit.Activate_DrawPowerCard();

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
				gameState.DahanAdjust( board[int.Parse( s )] );

			When_Growing( 1 );
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			Resolve_PlacePresence( expectedPresenseOptions, spirit.Presence.Energy.Next );
			// PlacePresence(2,dahan)

			Assert_HasEnergy( 0 );

		}

		[Fact]
		public void PresenseAndEnergy() {
			// +1 presense within 1, +4 energy
			Given_HasPresence( board[1] );

			When_StartingGrowth();
			spirit.Action.Choose( "PlacePresence(1) / GainEnergy(4)" );
			spirit.Activate_GainEnergy();
			Resolve_PlacePresence( "A1;A2;A4;A5;A6", spirit.Presence.Energy.Next );

			Assert.Equal(1,spirit.EnergyPerTurn);
			Assert_HasEnergy( 4+1 );

		}

		[Theory]
		[InlineDataAttribute(1,1,"")]
		[InlineDataAttribute(2,1,"A")]
		[InlineDataAttribute(3,2,"A")]
		[InlineDataAttribute(4,2,"AF")]
		[InlineDataAttribute(5,2,"AFS")]
		[InlineDataAttribute(6,3,"AFS")]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	1 air 2 fire sun 3
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_PresenceTracksAre( expectedEnergyGrowth, 1 );

			_ = spirit.TriggerEnergyElementsAndReclaims();

			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute(1,1,false)]
		[InlineDataAttribute(2,2,false)]
		[InlineDataAttribute(3,2,false)]
		[InlineDataAttribute(4,3,false)]
		[InlineDataAttribute(5,3,true)]
		[InlineDataAttribute(6,3,true)]
		[InlineDataAttribute(7,4,true)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, bool canReclaim1 ){
			// card:	1 2 2 3 reclaim-1 3 4
			Given_HalfOfPowercardsPlayed();

			Given_HasPresence( board[3] );

			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_PresenceTracksAre(1,expectedCardPlayCount);

			When_StartingGrowth();
			spirit.Action.Choose( "PlacePresence(1) / GainEnergy(4)" );

			spirit.Activate_GainEnergy();
			Resolve_PlacePresence( "A2;A3;A4", spirit.Presence.Energy.Next );

			if( canReclaim1 )
				AndWhen_ReclaimingFirstCard();

		}

	}

}
