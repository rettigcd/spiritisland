using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {

	[TestFixture]
	public class GrowthTests {

		const int initEnergy = 3;
		protected PlayerState playerState;
		protected GameState gameState;
		protected Board board;

		[SetUp]
		public void SetUp() {
			board = Board.GetBoardA();
			gameState = new GameState();
		}

		protected void Given_SpiritIs(Spirit spirit) {

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

		protected void When_Growing( int option, params IResolver[] resolvers ) {
			playerState.Grow(option, resolvers);
		}

		#region Asserts Presence

		protected void Assert_Add1Presence_Range0() {
			playerState.Presence.Add( board.spaces[4] );
			Assert_NewPresenceOptions( "A4" );
		}

		protected void Assert_Add1Presence_Range1() {
			playerState.Presence.Add( board.spaces[1] );
			Assert_NewPresenceOptions( "A1;A2;A4;A5;A6" ); // connected land, but not ocean
		}

		protected void Assert_AddPresense_Range2() {
			playerState.Presence.Add( board.spaces[3] ); 
			Assert_NewPresenceOptions( "A1;A2;A3;A4;A5" );
		}

		protected void Assert_AddPresense_Range3() {
			playerState.Presence.Add( board.spaces[3] ); 
			Assert_NewPresenceOptions( "A1;A2;A3;A4;A5;A6;A7;A8" );
		}

		protected void Assert_AddPresenseInJungleOrWetland_Range2() {
			playerState.Presence.Add( board.spaces[2] );
			Assert_NewPresenceOptions( "A2;A3;A5" );
		}

		protected void Assert_NewPresenceOptions( string expectedPlacementOptionString ) {

			var optionStrings = PresenceCalculator.PresenseToPlaceOptions(playerState,gameState)
				.Select( o => string.Join( "", o.Select( bs => bs.Label ).OrderBy( l => l ) ) )
				.OrderBy( s => s );

			string optionStr = string.Join( ";", optionStrings );
			Assert.That( optionStr, Is.EqualTo( expectedPlacementOptionString ) );
		}

		#endregion

		#region Asserts (Other)

		protected void Assert_GainPowercard( int expected ) {
			Assert.That( playerState.PowerCardsToDraw, Is.EqualTo( expected ), $"Expected to gain {expected} power card" );
		}

		protected void Assert_AllCardsAvailableToPlay() {
			// Then: all cards reclaimed (including unplayed)
			Assert.That( playerState.PlayedCards.Count, Is.EqualTo( 0 ), "Should not be any cards in 'played' pile" );
			Assert.That( string.Join( "", playerState.AvailableCards.Select( c => c.Name ).OrderBy( n => n ) ), Is.EquivalentTo( "ABCD" ) );
		}

		protected void Assert_GainEnergy( int expectedChange ) {
			Assert.That( playerState.Energy - initEnergy, Is.EqualTo( expectedChange ), $"Expected {expectedChange} energy change" );
		}

		#endregion

	}
}
