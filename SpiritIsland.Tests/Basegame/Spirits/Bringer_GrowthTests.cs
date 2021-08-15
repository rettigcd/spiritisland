using SpiritIsland.Basegame;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits {

	public class Bringer_GrowthTests : GrowthTests {

		public Bringer_GrowthTests():base( new Bringer() ){}

		[Fact] 
		public void ReclaimAll_PowerCard(){
			// reclaim, +1 power card
			Given_HalfOfPowercardsPlayed();
			When_Growing(0);
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Fact] 
		public void Reclaim1_Presence() {
			// reclaim 1, add presense range 0
			Given_HalfOfPowercardsPlayed();
			Given_HasPresence( board[4] );

			//			When_Growing( 1, Resolve_PlacePresence( "A4"), Resolve_Reclaim( 0 ) );
			When_Growing( 1 );
			Resolve_PlacePresence( "A4" );

			AndWhen_ReclaimingFirstCard();

			Assert.Equal( 3, spirit.Hand.Count );

		}

		[Fact] 
		public void PowerCard_Presence(){
			// +1 power card, +1 pressence range 1
			Given_HasPresence( board[1] );
			When_Growing(2);
			Resolve_PlacePresence( "A1;A2;A4;A5;A6");
			Assert_GainPowercard(1);
			Assert_BoardPresenceIs("A1A1");
		}

		[Fact] 
		public void PresenseOnPieces_Energy(){

			board = LineBoard.MakeBoard();
			Given_HasPresence(board[5]);
			gameState.AdjustDahan(board[6]);
			gameState.Adjust(board[7],InvaderSpecific.Explorer,1);
			gameState.Adjust(board[8],InvaderSpecific.Town,1);
			gameState.Adjust(board[9],InvaderSpecific.City,1);

			// add presense range 4 Dahan or Invadors, +2 energy
			When_Growing(3);
			Resolve_PlacePresence( "T6;T7;T8;T9",spirit.Presence.Energy.Next);

			Assert.Equal(2,spirit.EnergyPerTurn);
			Assert_HasEnergy(2+2);
			Assert_BoardPresenceIs("T5T6");
		}

		[Theory]
		[InlineDataAttribute(1,2,"")]
		[InlineDataAttribute(2,2,"A")]
		[InlineDataAttribute(3,3,"A")]
		[InlineDataAttribute(4,3,"AM")]
		[InlineDataAttribute(5,4,"AM")]
		[InlineDataAttribute(6,4,"AM*")]
		[InlineDataAttribute(7,5,"AM*")]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	2 air 3 moon 4 any 5
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
			When_Growing(0); // triggers elements
			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute(1,2,"")]
		[InlineDataAttribute(2,2,"")]
		[InlineDataAttribute(3,2,"")]
		[InlineDataAttribute(4,3,"")]
		[InlineDataAttribute(5,3,"")]
		[InlineDataAttribute(6,3,"*")] // will fail until we convert cards over also
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, string elements){
			// card:	2 2 2 3 3 any
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_CardTrackIs(expectedCardPlayCount);
			When_Growing(0);
			Assert_BonusElements( elements );
		}

	}

}
