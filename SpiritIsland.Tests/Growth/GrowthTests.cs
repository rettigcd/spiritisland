using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {

	[TestFixture]
	public class GrowthTests {

		const int initEnergy = 3;
		protected Spirit spirit;
		protected GameState gameState;
		protected Board board;

		[SetUp]
		public void SetUp() {
			board = Board.GetBoardA();
			gameState = new GameState();
		}

		protected void Given_SpiritIs(Spirit spirit) {

			// PlayerState requires Spirit to be known because Spirit creates playerState.
			this.spirit = spirit;
			this.spirit.Energy = initEnergy;
			Given_HalfOfPowercardsPlayed( this.spirit); // CANT use Property because not init yet.
		}

		void Given_HalfOfPowercardsPlayed(Spirit ps) {
			// Given: multiple cards played
			ps.PlayedCards.Add( new PowerCard( "A", 0, Speed.Fast, "A" ) );
			ps.PlayedCards.Add( new PowerCard( "B", 0, Speed.Fast, "A" ) );
			//   And: some available cards
			ps.AvailableCards.Add( new PowerCard( "C", 0, Speed.Fast, "A" ) );
			ps.AvailableCards.Add( new PowerCard( "D", 0, Speed.Fast, "A" ) );
		}

		protected void When_Growing( int option, params IResolver[] resolvers ) {
			spirit.Grow(gameState, option, resolvers);
		}

		protected void When_Growing( int option, string presenceOptions, params IResolver[] resolvers ) {
			this.expectedPlacementOptionString  = presenceOptions;

			var list = resolvers.ToList();
			list.Add(PlacePresence.Place(presenceOptions.Split(';')[0])); // !!! only testing the first one

			spirit.Grow(gameState, option, list.ToArray());
		}
		protected string expectedPlacementOptionString ;

		#region Asserts Presence

		protected void Assert_NewPresenceOptions() {

			var presenceRules = spirit.PresenceToPlace.ToArray();

			if(presenceRules.Length > 1){
				var optionStrings = PresenceCalculator.PresenseToPlaceOptions(
					spirit.CanPlacePresenceFrom,presenceRules,gameState
				)
					.Select( o => string.Join( "", o.Select( bs => bs.Label ).OrderBy( l => l ) ) )
					.OrderBy( s => s );

				string optionStr = string.Join( ";", optionStrings );
				Assert.That( optionStr, Is.EqualTo( expectedPlacementOptionString  ) );
			}
		}

		#endregion

		#region Asserts (Other)

		protected void Assert_GainPowercard( int expected ) {
			Assert.That( spirit.PowerCardsToDraw, Is.EqualTo( expected ), $"Expected to gain {expected} power card" );
		}

		protected void Assert_AllCardsAvailableToPlay() {
			// Then: all cards reclaimed (including unplayed)
			Assert.That( spirit.PlayedCards.Count, Is.EqualTo( 0 ), "Should not be any cards in 'played' pile" );
			Assert.That( string.Join( "", spirit.AvailableCards.Select( c => c.Name ).OrderBy( n => n ) ), Is.EquivalentTo( "ABCD" ) );
		}

		protected void Assert_GainEnergy( int expectedChange ) {
			Assert.That( spirit.Energy - initEnergy, Is.EqualTo( expectedChange ), $"Expected {expectedChange} energy change" );
		}

		#endregion

	}
}
