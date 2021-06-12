using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	// GetOptions => takes (GameState)


	// FAST
		// test 2 fast powers

	// !!! Invader Board
	// Fear Card
	// Ravage
	// Build
	// Explore

	// Slow
		// test 2 slow powers
		// 1 slow innate powers show up
	public class RiverSurges_GrowthTests : GrowthTests{

		public RiverSurges_GrowthTests():base( new RiverSurges() ){}

		#region growth

		[Fact]
		public void Reclaim_DrawCard_Energy() {

			// Given: using power pregression

			//   And: all cards played
			spirit.PlayedCards.AddRange(spirit.AvailableCards);
			spirit.AvailableCards.Clear();

			//  And: energy track is at 1
			Assert.Equal(1,spirit.EnergyPerTurn);

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay(5);
			Assert_HasCardAvailable( "Uncanny Melting" ); // gains 1st card in power progression
			Assert_HasEnergy( 1+1 ); // 1 Growth energy + 1 from energy track

		}

		[Fact]
		public void TwoPresence() {
			// reclaim, +1 power card, +1 energy
			// +1 presense withing 1, +1 presense range 1
			// +1 power card, +1 presense range 2

			Given_HasPresence( board[3] );
			Assert.Equal(1,spirit.RevealedEnergySpaces);

			When_Growing( 1
				, Resolve_PlacePresence("A2;A3;A4", 0, Track.Energy)
				, Resolve_PlacePresence("A1;A2;A3;A4", 0, Track.Energy) // original 0 will already be remoed
			);

			Assert_GainPowercard( 0 );
			Assert.Equal(2,spirit.EnergyPerTurn);
			Assert_HasEnergy( 2 ); // 2 from energy track
			Assert.Equal(3,spirit.RevealedEnergySpaces); // # of spaces revealed, not energy per turn
		}

		[Fact]
		public void Power_Presence() {
			// +1 power card, 
			// +1 presense range 2

			Assert.Equal(1,spirit.RevealedEnergySpaces);
			Given_HasPresence( board[3] );

			When_Growing( 2, Resolve_PlacePresence("A1;A2;A3;A4;A5", 0, Track.Card) );

			Assert_HasCardAvailable( "Uncanny Melting" ); // gains 1st card in power progression
			Assert_GainPowercard( 0 );
			Assert_HasEnergy( 1 ); // didn't increase energy track.
			Assert.Equal(1,spirit.RevealedEnergySpaces);
			Assert.Equal(2,spirit.RevealedCardSpaces);
		}

		#endregion

		#region presence tracks

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,2)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,3)]
		[InlineDataAttribute(5,4)]
		[InlineDataAttribute(6,4)]
		[InlineDataAttribute(7,5)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth ){
			// energy:	1 2 2 3 4 4 5

			spirit.RevealedEnergySpaces = revealedSpaces;
			Assert_PresenceTracksAre(expectedEnergyGrowth,1);
		}

		[Theory]
		[InlineDataAttribute(1,1,false)]
		[InlineDataAttribute(2,2,false)]
		[InlineDataAttribute(3,2,false)]
		[InlineDataAttribute(4,3,false)]
		[InlineDataAttribute(5,3,true)]
		[InlineDataAttribute(6,4,true)]
		[InlineDataAttribute(7,5,true)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, bool canReclaim1 ){
			// cards:	1 2 2 3 reclaim-1 4 5

			Given_HasPresence( board[3] );
			Given_HalfOfPowercardsPlayed();

			spirit.RevealedCardSpaces = revealedSpaces;
			Assert_PresenceTracksAre(1,expectedCardPlayCount);

			var resolvers = new List<IResolver>{Resolve_PlacePresence("A1;A2;A3;A4;A5")};

			if(canReclaim1)
				resolvers.Add(Resolve_Reclaim(0));

			When_Growing(2,resolvers.ToArray());

			// !!! for this test to work, we also need a test shows too many or too few resolvers, throw exception

		}

		#endregion

		[Theory]
		[InlineData(1,"Uncanny Melting")]
		[InlineData(2,"Nature's Resilience")]
		[InlineData(3,"Pull Beneath the Hungry Earth")]
		[InlineData(4,"Accelerated Rot")]
		[InlineData(5,"Song of Sanctity")]
		[InlineData(6,"Tsunami")]
		[InlineData(7,"Encompassing Ward")]
		public void PowerProgressionCards( int count, string lastPowerCard ){
			while(count--!=0)
				spirit.AddAction(new DrawPowerCard());

			Assert_HasCardAvailable( lastPowerCard );
		}

		#region Buying Cards

		//	Boon of Vigor => 0 => fast,any spirit		=> sun, water, plant	=> If you target yourself, gain 1 energy.  If you target another spirit, they gain 1 energy per power card they played this turn
		//	River's Bounty => 0 => slow, range 0, any	=> sun, water, animal	=> gather up to 2 dahan.  If ther are now at least 2 dahan, add 1 dahan and gain +1 energy
		//	Wash Away => 1 => slow, range 1, any		=> water mountain		=> Push up to 3 explorers / towns
		//	Flash Floods => 2 => fast, range 1, any		=> sun, water			=> 1 Damange.  If target land is costal +1 damage.

		[Theory]
		[InlineData("Boon of Vigor")]
		[InlineData("River's Bounty")]
		[InlineData("Wash Away")]
		[InlineData("Flash Floods")]
		public void SufficientEnergyToBuy(string cardName) {

			var card = FindSpiritsAvailableCard(cardName);
			spirit.Energy = card.Cost;

			// When:
			spirit.BuyAvailableCards(card);

			// Then: card is in Active/play list
			Assert.Contains(spirit.ActiveCards, c => c == card);

			//  And: card is not in Available list
			Assert.DoesNotContain(spirit.AvailableCards, c => c == card);

			Assert_CardInActionListIf(card, Speed.Fast);

			Assert_InnateInActionListIf(Speed.Fast);

			// Money is spent
			Assert.Equal(0, spirit.Energy);

		}

		void Assert_InnateInActionListIf(Speed currentSpeed) {
			var unresolvedInnates = spirit.UnresolvedActions
				.OfType<InnateAction>()
				.Select(a => a.Innate)
				.ToArray();

			var innate = spirit.InnatePowers[0];

			if (innate.Speed == currentSpeed)
				//  And: card is in Unresolved Action list
				Assert.Contains(innate, unresolvedInnates);
			else
				//  And: card is NOT in Unresolved Action list
				Assert.DoesNotContain(innate, unresolvedInnates);
		}

		protected void Assert_CardInActionListIf(PowerCard card, Speed currentSpeed) {

			var unresolvedCards = spirit.UnresolvedActions
				.OfType<PowerCard>()
				.ToArray();

			if (card.Speed == currentSpeed)
				//  And: card is in Unresolved Action list
				Assert.Contains(card, unresolvedCards);
			else
				//  And: card is NOT in Unresolved Action list
				Assert.DoesNotContain(card, unresolvedCards);
		}

		[Theory]
		[InlineData("Wash Away")]
		[InlineData("Flash Floods")]
		public void InsufficientEnergyToBuy(string cardName){
			var card = spirit.AvailableCards.VerboseSingle(c=>c.Name == cardName);
			spirit.Energy = card.Cost - 1;

			// When:
			void Purchase() => spirit.BuyAvailableCards( card );

			Assert.Throws<InsufficientEnergyException>( Purchase );
		}

		[Fact]
		public void InsufficientCardCountToBuy(){
			
			// Given: has 2 cards they want to play
			var card1 = spirit.AvailableCards[0];
			var card2 = spirit.AvailableCards[1];

			//  And: lots of energy
			spirit.Energy = 200;

			//  But: can only play 1 card
			Assert.Equal(1,spirit.NumberOfCardsPlayablePerTurn);

			// When:
			void Purchase() => spirit.BuyAvailableCards( card1, card2 );

			Assert.Throws<InsufficientCardPlaysException>(Purchase);
		}

		[Fact]
		public void CantActivateDiscardedCards() {
			// Given: Boon of vigor already played
			PowerCard card = FindSpiritsAvailableCard("Boon of Vigor");
			Discard(card);

			// When
			void Purchase() => spirit.BuyAvailableCards( card );

			Assert.Throws<CardNotAvailableException>( Purchase );
		}

		#region Innates

		[Fact]
		public void Innate(){
			// ! should always add to unresolved list - since elements might change and make it active or not
			
		}

		#endregion Innates

		void Discard(PowerCard card) {
			spirit.AvailableCards.Remove(card);
			spirit.PlayedCards.Add(card);
		}

		PowerCard FindSpiritsAvailableCard(string cardName) => spirit.AvailableCards.VerboseSingle(c => c.Name == cardName);

		#endregion

		#region Fast

		[Fact]
		public void BooneOfVigor_PlayOnSelf(){

		}

		#endregion Fast

	}

}
