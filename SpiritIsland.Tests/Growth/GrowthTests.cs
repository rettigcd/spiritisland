using SpiritIsland.PowerCards;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class GrowthTests {

		public static Dictionary<Element,char> ElementChars = new Dictionary<Element, char>{
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

		protected Spirit spirit;
		protected GameState gameState;
		protected Board board;

		protected GrowthTests(Spirit spirit){
			// PlayerState requires Spirit to be known because Spirit creates playerState.
			this.spirit = spirit;
			gameState = new GameState(spirit) {
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

		protected void Given_HalfOfPowercardsPlayed() {
			Discard(spirit.AvailableCards.Count-1);
			Discard(spirit.AvailableCards.Count-1);
		}
		void Discard(int idx){
			spirit.PlayedCards.Add( spirit.AvailableCards[idx] );
			spirit.AvailableCards.RemoveAt( idx );
		}

		#endregion

		protected void When_Growing( int option, params IResolver[] resolvers ) {
			spirit.Grow(gameState, option);

			// modify the growth option to resolve incomplete states

			foreach (var resolver in resolvers)
				resolver.Apply( 
					spirit.UnresolvedActions
						.Select(f=>f.Bind(this.spirit,this.gameState))
						.ToList()
			);

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

		protected void Assert_HasCardAvailable( string name ){
			bool nameMatches( PowerCard card ) => string.Compare(name,card.Name,true) == 0;
			Assert.True(spirit.AvailableCards.Any( nameMatches ),$"Hand does not contain {name}");
		}

		protected void Assert_AllCardsAvailableToPlay(int expectedAvailableCount = 4) {
			// Then: all cards reclaimed (including unplayed)
			Assert.Empty( spirit.PlayedCards ); // , "Should not be any cards in 'played' pile" );
			Assert.Equal( expectedAvailableCount, spirit.AvailableCards.Count );
		}

		protected void Assert_HasEnergy( int expectedChange ) {
			Assert.Equal( expectedChange, spirit.Energy ); // , $"Expected {expectedChange} energy change" );
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

		//protected SpyOnPlacePresence Resolve_PlacePresence(
		//	string placeOptions,
		//	Track source = Track.Energy
		//) => Resolve_PlacePresence(placeOptions,0,source);

		protected SpyOnPlacePresence Resolve_PlacePresence( 
			string placeOptions, 
			int focus=0, 
			Track source = Track.Energy
		) {
			return new SpyOnPlacePresence( placeOptions, focus, source );
		}

		protected void Assert_PresenceTracksAre(int expectedEnergy,int expectedCards) {
			Assert_EnergyTrackIs( expectedEnergy );
			Assert_CardTrackIs( expectedCards );
		}

		public void Assert_CardTrackIs( int expectedCards ) {
			Assert.Equal( expectedCards, spirit.NumberOfCardsPlayablePerTurn );
		}

		public void Assert_EnergyTrackIs( int expectedEnergy ) {
			Assert.Equal( expectedEnergy, spirit.EnergyPerTurn );
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
