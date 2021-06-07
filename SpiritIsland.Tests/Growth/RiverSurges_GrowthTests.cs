using System;
using System.Collections.Generic;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	// !!! Purchasing Power Cards
	// restricted by card track
	// restricted by energy in bank
	// purchased cards generate active power card list - 
	// purchased card combos that do/don't trigger innate powers
	// innate power + active card list => Available Power List

	// FAST

	// !!! Invader Board
	// Fear Card
	// Ravage
	// Build
	// Explore

	// Slow

	public class RiverSurges_GrowthTests : GrowthTests{

		public RiverSurges_GrowthTests():base( new RiverSurges() ){}

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
				spirit.AddAction(new DrawPowerCard(spirit));

			Assert_HasCardAvailable( lastPowerCard );
		}

	}

}
