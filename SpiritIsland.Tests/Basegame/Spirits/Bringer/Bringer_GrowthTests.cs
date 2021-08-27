using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.BringerNS {

	public class Bringer_GrowthTests : GrowthTests {

		public Bringer_GrowthTests():base( new Bringer { CardDrawer = new IncrementCountCardDrawer() } ) {}

		[Fact] 
		public void ReclaimAll_PowerCard(){
			// reclaim, +1 power card
			Given_HalfOfPowercardsPlayed();
			When_Growing(0);
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_DrawPowerCard();
			spirit.Activate_ReclaimAll();
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Fact] 
		public void Reclaim1_Presence() {
			// reclaim 1, add presense range 0
			Given_HalfOfPowercardsPlayed();
			Given_HasPresence( board[4] );

			When_Growing( 1 );
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_Reclaim1();
			spirit.Action.AssertDecision( "Select card to reclaim.", "Predatory Nightmares $2 (Slow),Dreams of the Dahan $0 (Fast)", "Dreams of the Dahan $0 (Fast)" );
			Resolve_PlacePresence( "A4", spirit.Presence.Energy.Next );

//			AndWhen_ReclaimingFirstCard();

			spirit.Hand.Count.ShouldBe( 3 );
		}

		[Fact] 
		public void PowerCard_Presence(){
			// +1 power card, +1 pressence range 1
			Given_HasPresence( board[1] );
			When_Growing(2);
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_DrawPowerCard();
			Resolve_PlacePresence( "A1;A2;A4;A5;A6", spirit.Presence.Energy.Next );
			Assert_GainPowercard(1);
			Assert_BoardPresenceIs("A1A1");
		}

		[Fact] 
		public void PresenseOnPieces_Energy(){

			board = LineBoard.MakeBoard();
			Given_HasPresence(board[5]);
			gameState.Dahan.Adjust(board[6]);
			gameState.Adjust(board[7],InvaderSpecific.Explorer,1);
			gameState.Adjust(board[8],InvaderSpecific.Town,1);
			gameState.Adjust(board[9],InvaderSpecific.City,1);

			// add presense range 4 Dahan or Invadors, +2 energy
			When_Growing(3);
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_GainEnergy();
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
		[InlineDataAttribute(6,4,"AM")] // !!! Test SelectAnyElement() growth action is in the list
		[InlineDataAttribute(7,5,"AM")] // !!! same
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	2 air 3 moon 4 any 5
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
			When_Growing(0); // triggers elements
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_DrawPowerCard();
			spirit.Activate_ReclaimAll();

			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute(1,2,"")]
		[InlineDataAttribute(2,2,"")]
		[InlineDataAttribute(3,2,"")]
		[InlineDataAttribute(4,3,"")]
		[InlineDataAttribute(5,3,"")]
		[InlineDataAttribute(6,3,"")] // !!! need way to test this 'Any' element
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, string elements){
			// card:	2 2 2 3 3 any
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_CardTrackIs(expectedCardPlayCount);
			When_Growing(0);
			Assert_BonusElements( elements );
		}

	}

}
