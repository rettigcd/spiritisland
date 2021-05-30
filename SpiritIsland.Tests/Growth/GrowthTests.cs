using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class GrowthTests {

		protected static Dictionary<Element,char> ElementChars = new Dictionary<Element, char>{
				[Element.Air] = 'A',	
				[Element.Animal] = 'B',	
				[Element.Earth] = 'E',  
				[Element.Fire] = 'F',	
				[Element.Moon] = 'M',	
				[Element.Plant] = 'P',	
				[Element.Sun] = 'S',	
				[Element.Water] = 'W',
				[Element.Any] = '*',
			};

		const int initEnergy = 3;
		protected Spirit spirit;
		protected GameState gameState;
		protected Board board;

		protected GrowthTests(Spirit spirit){
			Given_SpiritIs(spirit);
			gameState = new GameState {
				Island = new Island( BoardA )
			};
			board = gameState.Island.Boards[0];
		}

		#region Board factories

		static protected Board BoardA => Board.BuildBoardA();
		static protected Board BoardB => Board.BuildBoardB();
		static protected Board BoardC => Board.BuildBoardC();
		static protected Board BoardD => Board.BuildBoardD();

		#endregion

		#region Given

		protected void Given_HasWilds( Space space ) {
			gameState.AddWilds( space );
		}

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

		#endregion

		protected void When_Growing( int option, params IResolver[] resolvers ) {
			spirit.Grow(gameState, option, resolvers);
		}

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

		#region Resolve_

		protected static IResolver[] Resolve_PushPresence( string select ) {
			return select.Split( ',' )
				.Where( x => !string.IsNullOrEmpty( x ) )
				.Select( x => {
					var p = x.Split( '>' );
					return new PushPresence.Resolve( p[0], p[1] );
				} )
				.ToArray();
		}

		protected static IResolver[] Resolve_GatherPresence( string select ) {
			return select.Split( ',' )
				.Where( x => !string.IsNullOrEmpty( x ) )
				.Select( x => {
					var p = x.Split( '>' );
					return new GatherPresence.Resolve( p[0], p[1] );
				} )
				.ToArray();
		}

		protected IResolver Resolve_Reclaim( int index ) {
			return Reclaim1.Resolve( spirit.PlayedCards[index] );
		}

		protected SpyOnPlacePresence Resolve_PlacePresence( string placeOptions, params Track[] source ) {
			if( source == null || source.Length==0 ) source = new Track[]{ Track.Energy }; // default to energy
			return new SpyOnPlacePresence( placeOptions, source );
		}

		protected void Assert_PresenceTracksAre(int expectedEnergy,int expectedCards) {
			Assert_EnergyTrackIs( expectedEnergy );
			Assert_CardTrackIs( expectedCards );
		}

		public void Assert_CardTrackIs( int expectedCards ) {
			Assert.Equal( expectedCards, spirit.NumberOfCardsPlayablePerTurn );
		}

		public void Assert_EnergyTrackIs( int expectedEnergy ) {
			Assert.Equal( expectedEnergy, spirit.EnergyGrowth );
		}

		protected void Assert_BonusElements( string elements ) {
			foreach(var pair in GrowthTests.ElementChars) {
				int expected = elements.Count( x => x == pair.Value );
				Assert.Equal( expected, spirit.Elements( pair.Key ) );
			}
		}

		#endregion

	}

}
