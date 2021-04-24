using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests {


	[TestFixture]
	public class Growth_Tests {

		const int initEnergy = 3;
		PlayerState playerState;
		GameState gameState;
		BoardTile tile;

		[SetUp]
		public void SetUp() {
			tile = BoardTile.GetBoardA();
			playerState = new PlayerState {
				Energy = initEnergy
			};
			Given_HalfOfPowercardsPlayed();
			gameState = new GameState();
		}

		#region Thunderspeaker

		static GrowthOption GetThunderSpeakerGrowthOption( int index ) {
			var character = new ThunderSpeaker();
			var growthOptions = character.GetGrowthOptions();
			Assert.That( growthOptions.Length, Is.EqualTo( 3 ) );
			return growthOptions[index];
		}

		[Test]
		public void Thunderspeaker_ReclaimAnd2PowerCards() {
			// Growth Option 1 - Reclaim All, +2 Power cards

			When_Growing( GetThunderSpeakerGrowthOption( 0 ) );

			Assert_AllCardsAvailableToPlay();
			Assert.That( playerState.PowerCardsToDraw, Is.EqualTo( 2 ) );
			Assert_NoEnergyChange();
		}

		[TestCase( "3,5,8", "A3A3;A3A5;A5A5;A5A8" )]
		[TestCase( "3,4,8", "A3A3;A3A4;A4A4;A4A8" )]
		[TestCase( "4,8", "A4A4;A4A8" )]
		[TestCase( "1,4,8", "A1A1;A1A4;A4A4;A4A8" )]
		public void Thunderspeaker_2Presence( string initialDahanSquares, string expectedPresenseOptions ) {
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan

			// Given: presense on 3 of board A
			playerState.Presence.Add( tile.spaces[3] );

			//	 And: dahan on initial spot
			foreach(string s in initialDahanSquares.Split( ',' ))
				gameState.AddDahanToSpace( tile.spaces[int.Parse( s )] );

			When_Growing( GetThunderSpeakerGrowthOption( 1 ) );

			// Then: 
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan
			Assert_NewPresenceOptions( expectedPresenseOptions );
			//  And: Energy didn't change
			Assert_GainEnergy( 0 );

		}

		[Test]
		public void Thunderspeaker_Growth3() {
			// +1 presense within 1, +4 energy

			// Given: presense on 1 of board A
			playerState.Presence.Add( tile.spaces[1] );

			When_Growing( GetThunderSpeakerGrowthOption( 2 ) );

			Assert_NewPresenceOptions( "A1;A2;A4;A5;A6" ); // connected land, but not ocean
			Assert_GainEnergy( 4 );

		}

		#endregion

		#region River Surges in Sunlight
		static GrowthOption GetRiverSurgesGrowthOption( int index ) {
			var character = new RiverSurges();
			var growthOptions = character.GetGrowthOptions();
			Assert.That( growthOptions.Length, Is.EqualTo( 3 ) );
			return growthOptions[index];
		}

		[Test]
		public void RiverSurges_ReclaimGrowth() {

			When_Growing( GetRiverSurgesGrowthOption( 0 ) );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 1 );

		}

		[Test]
		public void RiverSurges_TwoPresence() {

			playerState.Presence.Add( tile.spaces[1] );

			When_Growing( GetRiverSurgesGrowthOption( 1 ) );

			Assert_GainPowercard( 0 );
			Assert_GainEnergy( 0 );
			Assert_NewPresenceOptions( 
				"A1A1;A1A2;A1A4;A1A5;A1A6;"
				+"A2A2;A2A3;A2A4;A2A5;A2A6;"
				+"A3A4;"
				+"A4A4;A4A5;A4A6;"
				+"A5A5;A5A6;A5A7;A5A8;"
				+"A6A6;A6A8" ); // connected land, but not ocean


		}

		[Test]
		public void RiverSurges_PowerAndPresence() {

			// +1 power card, 
			// +1 presense range 2

			playerState.Presence.Add( tile.spaces[3] );

			When_Growing( GetRiverSurgesGrowthOption( 2 ) );

			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 0 );
			Assert_NewPresenceOptions( "A1;A2;A3;A4;A5");

		}

		#endregion

		#region Lightning's Swift Strike

		static GrowthOption GetLightningGrowthOption( int index ) {
			var character = new Ligntning();
			var growthOptions = character.GetGrowthOptions();
			Assert.That( growthOptions.Length, Is.EqualTo( 3 ) );
			return growthOptions[index];
		}

		[Test]
		public void Lightning_Reclaim(){
			// * reclaim, +1 power card, +1 energy

			var x = GetLightningGrowthOption(0);
			When_Growing( GetLightningGrowthOption(0) );

			this.Assert_AllCardsAvailableToPlay();
			this.Assert_GainPowercard(1);
			this.Assert_GainEnergy(1);

		}

		[Test]
		public void Lightning_PresenseAndEnergy(){
			// +1 presense range 1, +3 energy

			playerState.Presence.Add( tile.spaces[3] );

			When_Growing( GetLightningGrowthOption(2) );

			this.Assert_GainEnergy(3);
			Assert_NewPresenceOptions( "A2;A3;A4");
		}


		#endregion

		void When_Growing( GrowthOption growth ) {
			growth.Apply( playerState, gameState );
		}
		void Assert_GainPowercard( int expected ) {
			Assert.That( playerState.PowerCardsToDraw, Is.EqualTo( expected ), $"Expected to gain {expected} power card" );
		}

		void Assert_AllCardsAvailableToPlay() {
			// Then: all cards reclaimed (including unplayed)
			Assert.That( playerState.PlayedCards.Count, Is.EqualTo( 0 ), "Should not be any cards in 'played' pile" );
			Assert.That( string.Join( "", playerState.AvailableCards.Select( c => c.Name ).OrderBy( n => n ) ), Is.EquivalentTo( "ABCD" ) );
		}

		void Given_HalfOfPowercardsPlayed() {
			// Given: multiple cards played
			playerState.PlayedCards.Add( new PowerCard( "A", 0, Speed.Fast, "A" ) );
			playerState.PlayedCards.Add( new PowerCard( "B", 0, Speed.Fast, "A" ) );
			//   And: some available cards
			playerState.AvailableCards.Add( new PowerCard( "C", 0, Speed.Fast, "A" ) );
			playerState.AvailableCards.Add( new PowerCard( "D", 0, Speed.Fast, "A" ) );
		}

		#region Asserts

		void Assert_NewPresenceOptions( string expectedPlacementOptionString ) {

			

			var optionStrings = PresenceCalculator.PresenseToPlaceOptions(playerState)
				.Select( o => string.Join( "", o.Select( bs => bs.Label ).OrderBy( l => l ) ) )
				.OrderBy( s => s );

			string optionStr = string.Join( ";", optionStrings );
			Assert.That( optionStr, Is.EqualTo( expectedPlacementOptionString ) );
		}

		void Assert_NoEnergyChange() => Assert_GainEnergy( 0 );

		void Assert_GainEnergy( int expectedChange ) {
			Assert.That( playerState.Energy - initEnergy, Is.EqualTo( expectedChange ), $"Expected {expectedChange} energy change" );
		}

		#endregion

	}

}
