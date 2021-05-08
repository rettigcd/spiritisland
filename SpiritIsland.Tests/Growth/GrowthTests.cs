using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	public partial class GrowthTests {

		const int initEnergy = 3;
		protected Spirit spirit;
		protected GameState gameState;
		protected Board board;

		public GrowthTests(){
			gameState = new GameState {
				Island = new Island( BoardA )
			};
			board = gameState.Island.Boards[0];
		}

		protected Board BoardA => Board.BuildBoardA();
		protected Board BoardB => Board.BuildBoardB();
		protected Board BoardC => Board.BuildBoardC();
		protected Board BoardD => Board.BuildBoardD();

		protected void Given_HasPresence( params Space[] spaces ) {
			spirit.Presence.AddRange( spaces );
		}

		protected void Given_HasPresence( string presenceString ) {
			Dictionary<string,Space> spaceLookup = gameState.Island.Boards
				.SelectMany(b=>b.Spaces)
				.ToDictionary(s=>s.Label,s=>s);
			var spaces = new Space[presenceString.Length/2];
			for(int i=0;i*2<presenceString.Length;i++)
				spaces[i] = spaceLookup[presenceString.Substring(i*2,2)];
			Given_HasPresence(spaces);
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
			list.Add(new SpyOnPlacePresence( presenceOptions ));

			spirit.Grow(gameState, option, list.ToArray());
		}
		protected string expectedPlacementOptionString ;

		#region Asserts Presence

		protected void Assert_BoardPresenceIs( string expected ) {
			var actual = spirit.Presence.Select(s=>s.Label).OrderBy(l=>l).Join();
			Assert.Equal(expected, actual); // , Is.EqualTo(expected),"Presence in wrong place");
		}

		#endregion

		#region Asserts (Other)

		protected void Assert_GainPowercard( int expected ) {
			Assert.Equal( expected, spirit.PowerCardsToDraw ); // , $"Expected to gain {expected} power card" );
		}

		protected void Assert_AllCardsAvailableToPlay() {
			// Then: all cards reclaimed (including unplayed)
			Assert.Empty( spirit.PlayedCards ); // , "Should not be any cards in 'played' pile" );
			Assert.Equal( "ABCD", spirit.AvailableCards.Select( c => c.Name ).OrderBy( n => n ).Join("") );
		}

		protected void Assert_GainEnergy( int expectedChange ) {
			Assert.Equal( expectedChange, spirit.Energy - initEnergy ); // , $"Expected {expectedChange} energy change" );
		}

		#endregion

	}

}
