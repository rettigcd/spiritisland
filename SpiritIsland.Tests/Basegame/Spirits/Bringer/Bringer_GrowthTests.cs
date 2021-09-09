using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.BringerNS {

	public class Bringer_GrowthTests : GrowthTests {

		static Spirit InitSpirit() {
			return new Bringer {
				CardDrawer = new PowerProgression(
					PowerCard.For<VeilTheNightsHunt>(),
					PowerCard.For<ReachingGrasp>()
				),
			};
		}

		public Bringer_GrowthTests():base( InitSpirit() ) {}

		[Fact] 
		public void ReclaimAll_PowerCard(){
			// reclaim, +1 power card
			Given_HalfOfPowercardsPlayed();

			When_Growing(0);
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_DrawPowerCard();
			spirit.Activate_ReclaimAll();

			// Then:
			Assert_AllCardsAvailableToPlay(4+1);
			Assert_HasCardAvailable( "Veil the Night's Hunt" );

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
			spirit.Hand.Count.ShouldBe( 3 );
		}

		[Fact] 
		public void PowerCard_Presence() {
			// +1 power card, +1 pressence range 1
			Given_HasPresence( board[1] );

			When_Growing( 2 );
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			spirit.Activate_DrawPowerCard();
			Resolve_PlacePresence( "A1;A2;A4;A5;A6", spirit.Presence.Energy.Next );
			Assert_GainsFirstPowerProgressionCard(); // gains 1st card in power progression
			Assert_BoardPresenceIs( "A1A1" );
		}

		void Assert_GainsFirstPowerProgressionCard() {
			Assert_HasCardAvailable( "Veil the Night's Hunt" );
		}

		[Fact] 
		public void PresenseOnPieces_Energy(){

			board = LineBoard.MakeBoard();
			Given_HasPresence(board[5]);
			gameState.DahanAdjust(board[6]);
			gameState.Tokens[board[7]].Adjust(Invader.Explorer.Default,1);
			gameState.Tokens[board[8]].Adjust(Invader.Town.Default,1);
			gameState.Tokens[board[0]].Adjust(Invader.City.Default,1);

			// add presense range 4 Dahan or Invadors, +2 energy
			When_StartingGrowth();
			spirit.Action.Choose( "GainEnergy(2) / PlacePresence(4,dahan or invaders)" );
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
		[InlineDataAttribute(6,4,"AM*")]
		[InlineDataAttribute(7,5,"AM*")]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	2 air 3 moon 4 any 5
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );

			spirit.TriggerEnergyElementsAndReclaims();

			if(elements.Contains( '*' ))
				spirit.GetAvailableActions(Speed.Growth).Single().Name.ShouldBe("Select elements (1)");

			//When_Growing(0); // triggers elements
			//_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			//spirit.Activate_DrawPowerCard();
			//spirit.Activate_ReclaimAll();

			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute(1,2,"")]
		[InlineDataAttribute(2,2,"")]
		[InlineDataAttribute(3,2,"")]
		[InlineDataAttribute(4,3,"")]
		[InlineDataAttribute(5,3,"")]
		[InlineDataAttribute(6,3,"*")]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, string elements){
			// card:	2 2 2 3 3 any
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_CardTrackIs(expectedCardPlayCount);

			spirit.TriggerEnergyElementsAndReclaims();

			if(elements.Contains( '*' ))
				spirit.GetAvailableActions( Speed.Growth ).Single().Name.ShouldBe( "Select elements (1)" );

			Assert_BonusElements( elements );
		}

	}

}
