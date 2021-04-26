using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests {

	// Growth that needs resolved:
		// - where to place presence
		// - which track to take presence from
		// ? With .ResolvePresence(...)

	// !!! Test Initial Sprit Player State
	// !!! Test Presence Tracks
	// !!! Test Placing Initial Presence on different boards

	[TestFixture]
	public class Growth_Tests {

		const int initEnergy = 3;
		PlayerState playerState;
		GameState gameState;
		BoardTile tile;

		[SetUp]
		public void SetUp() {
			tile = BoardTile.GetBoardA();
			gameState = new GameState();
		}

		#region Thunderspeaker
		void Given_Thunderspeaker() => Given_SpiritIs( new ThunderSpeaker() );

		[Test]
		public void Thunderspeaker_ReclaimAnd2PowerCards() {
			// Growth Option 1 - Reclaim All, +2 Power cards

			Given_Thunderspeaker();
			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert.That( playerState.PowerCardsToDraw, Is.EqualTo( 2 ) );
			Assert_GainEnergy(0);
		}

		[TestCase( "3,5,8", "A3A3;A3A5;A5A5;A5A8" )]
		[TestCase( "3,4,8", "A3A3;A3A4;A4A4;A4A8" )]
		[TestCase( "4,8", "A4A4;A4A8" )]
		[TestCase( "1,4,8", "A1A1;A1A4;A4A4;A4A8" )]
		public void Thunderspeaker_2Presence( string initialDahanSquares, string expectedPresenseOptions ) {
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan

			Given_Thunderspeaker();
			When_Growing( 1 );

			// Then: 
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan
			playerState.Presence.Add( tile.spaces[3] );
			//	 And: dahan on initial spot
			foreach(string s in initialDahanSquares.Split( ',' ))
				gameState.AddDahanToSpace( tile.spaces[int.Parse( s )] );
			Assert_NewPresenceOptions( expectedPresenseOptions );

			//  And: Energy didn't change
			Assert_GainEnergy( 0 );

		}

		[Test]
		public void Thunderspeaker_Growth3() {
			// +1 presense within 1, +4 energy

			Given_Thunderspeaker();
			When_Growing( 2 );

			Assert_Add1Presence_Range1();
			Assert_GainEnergy( 4 );

		}

		#endregion

		#region River Surges in Sunlight
		void Given_RiverSurges() => Given_SpiritIs( new RiverSurges() );

		[Test]
		public void RiverSurges_ReclaimGrowth() {

			Given_RiverSurges();
			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 1 );

		}

		[Test]
		public void RiverSurges_TwoPresence() {

			Given_RiverSurges();
			When_Growing( 1 );

			Assert_GainPowercard( 0 );
			Assert_GainEnergy( 0 );

			playerState.Presence.Add( tile.spaces[1] );
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

			Given_RiverSurges();
			When_Growing( 2 );

			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 0 );

			playerState.Presence.Add( tile.spaces[3] );
			Assert_NewPresenceOptions( "A1;A2;A3;A4;A5");

		}

		#endregion

		#region Lightning's Swift Strike

		void Given_Lightning() => Given_SpiritIs(new Lightning());

		[Test]
		public void Lightning_Reclaim(){
			// * reclaim, +1 power card, +1 energy

			Given_Lightning();
			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
			Assert_GainEnergy(1);

		}

		[Test]
		public void Lightning_PresenseAndEnergy() {
			// +1 presense range 1, +3 energy

			Given_Lightning();
			When_Growing( 2 );

			Assert_GainEnergy( 3 );
			Assert_Add1Presence_Range1();
		}

		[Test]
		public void Lightning_2Presence(){
			// +1 presense range 2, +1 prsense range 0

			Given_Lightning();
			When_Growing( 1 );

			Assert_GainEnergy( 0 );

			playerState.Presence.Add( tile.spaces[3] ); 
			Assert_NewPresenceOptions( "A1A1;A1A3;A2A2;A2A3;A3A3;A3A4;A3A5;A4A4;A5A5" );

		}

		#endregion

		#region Vital Strength of Earth

		void Given_VitalStrength() => Given_SpiritIs(new VitalStrength());

		[Test]
		public void VitalStrength_ReclaimAndPresence(){
			// reclaim, +1 presense range 2
			Given_VitalStrength();
			When_Growing( 0 );

			this.Assert_AllCardsAvailableToPlay();
			Assert_AddPresense_Range2();

		}

		[Test]
		public void VitalStrength_PowercardAndPresence() {
			// +1 power card, +1 presense range 0
			Given_VitalStrength();
			When_Growing( 1 );

			Assert_GainPowercard( 1 );
			Assert_Add1PresenceRange0();
		}

		[Test]
		public void VitalStrength_PresenseAndEnergy(){
			// +1 presence range 1, +2 energy
			Given_VitalStrength();
			When_Growing( 2 );
			Assert_Add1Presence_Range1();
			Assert_GainEnergy(2);
		}

		#endregion

		#region Shadows Flicker

		void Given_Shadows() => Given_SpiritIs(new Shadows());

		[Test]
		public void ShadowsFlickerG0_Reclaim(){
			// reclaim, gain power Card
			Given_Shadows();
			When_Growing( 0 );
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Test]
		public void ShadowsFlickerG1_PowerAndPresence(){
			// gain power card, add a presense range 1
			Given_Shadows();
			When_Growing( 1 );
			Assert_GainPowercard(1);
			Assert_Add1Presence_Range1();
		}

		[Test]
		public void ShadowsFlickerG2_PresenceAndEnergy(){
			// add a presence withing 3, +3 energy
			Given_Shadows();
			When_Growing( 2 );
			Assert_GainEnergy(3);
			Assert_AddPresense_Range3();
		}


		#endregion

		#region Spread of Rampant Green

		void Given_RampantGreen() => Given_SpiritIs( new RampantGreen() );

		// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
		// reclaim, +1 power card
		// +1 presense range 1, play +1 extra card this turn
		// +1 power card, +3 energy
		[Test]
		public void RampantGreenG0_() {
			// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
			// reclaim, +1 power card

			Given_RampantGreen();
			When_Growing( 0 );

			Assert_AddPresenseInJungleOrWetland_Range2();
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
		}

		[Test]
		[Ignore("until we refactor growth to accomoddate this")]
		public void RampantGreenG1_(){
			// +1 presense to jungle or wetland - range 2
			// +1 presense range 1, play +1 extra card this turn
			Given_RampantGreen();
			When_Growing( 1 );
			Assert_AddPresenseInJungleOrWetland_Range2();
			Assert_Add1Presence_Range1();
		}

		[Test]
		public void RampantGreenG2_(){
			// +1 presense to jungle or wetland - range 2
			// +1 power card, +3 energy
			Given_RampantGreen();
			When_Growing( 2 );
			Assert_AddPresenseInJungleOrWetland_Range2();
			Assert_GainEnergy(3);
			Assert_GainPowercard(1);
		}


		#endregion

		void Given_SpiritIs(Spirit spirit) {

			// PlayerState requires Spirit to be known because Spirit creates playerState.
			playerState = spirit.CreateInitialPlayerState();
			playerState.Energy = initEnergy;
			Given_HalfOfPowercardsPlayed(playerState); // CANT use Property because not init yet.
		}

		void Given_HalfOfPowercardsPlayed(PlayerState ps) {
			// Given: multiple cards played
			ps.PlayedCards.Add( new PowerCard( "A", 0, Speed.Fast, "A" ) );
			ps.PlayedCards.Add( new PowerCard( "B", 0, Speed.Fast, "A" ) );
			//   And: some available cards
			ps.AvailableCards.Add( new PowerCard( "C", 0, Speed.Fast, "A" ) );
			ps.AvailableCards.Add( new PowerCard( "D", 0, Speed.Fast, "A" ) );
		}

		void When_Growing( int option ) {
			playerState.Grow(option);
		}

		#region Asserts Presence

		void Assert_Add1PresenceRange0() {
			playerState.Presence.Add( tile.spaces[4] );
			Assert_NewPresenceOptions( "A4" );
		}

		void Assert_Add1Presence_Range1() {
			playerState.Presence.Add( tile.spaces[1] );
			Assert_NewPresenceOptions( "A1;A2;A4;A5;A6" ); // connected land, but not ocean
		}

		void Assert_AddPresense_Range2() {
			playerState.Presence.Add( tile.spaces[3] ); 
			Assert_NewPresenceOptions( "A1;A2;A3;A4;A5" );
		}

		void Assert_AddPresense_Range3() {
			playerState.Presence.Add( tile.spaces[3] ); 
			Assert_NewPresenceOptions( "A1;A2;A3;A4;A5;A6;A7;A8" );
		}

		void Assert_AddPresenseInJungleOrWetland_Range2() {
			playerState.Presence.Add( tile.spaces[2] );
			Assert_NewPresenceOptions( "A2;A3;A5" );
		}

		void Assert_NewPresenceOptions( string expectedPlacementOptionString ) {

			var optionStrings = PresenceCalculator.PresenseToPlaceOptions(playerState,gameState)
				.Select( o => string.Join( "", o.Select( bs => bs.Label ).OrderBy( l => l ) ) )
				.OrderBy( s => s );

			string optionStr = string.Join( ";", optionStrings );
			Assert.That( optionStr, Is.EqualTo( expectedPlacementOptionString ) );
		}

		#endregion

		#region Asserts (Other)

		void Assert_GainPowercard( int expected ) {
			Assert.That( playerState.PowerCardsToDraw, Is.EqualTo( expected ), $"Expected to gain {expected} power card" );
		}

		void Assert_AllCardsAvailableToPlay() {
			// Then: all cards reclaimed (including unplayed)
			Assert.That( playerState.PlayedCards.Count, Is.EqualTo( 0 ), "Should not be any cards in 'played' pile" );
			Assert.That( string.Join( "", playerState.AvailableCards.Select( c => c.Name ).OrderBy( n => n ) ), Is.EquivalentTo( "ABCD" ) );
		}

		void Assert_GainEnergy( int expectedChange ) {
			Assert.That( playerState.Energy - initEnergy, Is.EqualTo( expectedChange ), $"Expected {expectedChange} energy change" );
		}

		#endregion

	}

}
