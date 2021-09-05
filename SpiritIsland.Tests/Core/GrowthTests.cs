using Shouldly;
using SpiritIsland.SinglePlayer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests {

	public class GrowthTests {

		public static readonly Dictionary<Element,char> ElementChars = new() {
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
			board = BoardA;
			gameState = new GameState(spirit, board );
		}

		#region Board factories

		static protected Board BoardA => Board.BuildBoardA();
		static protected Board BoardB => Board.BuildBoardB();
		static protected Board BoardC => Board.BuildBoardC();
		static protected Board BoardD => Board.BuildBoardD();

		#endregion

		#region Given

		protected void Given_HasPresence( params Space[] spaces ) {
			foreach(var x in spaces)
				spirit.Presence.PlaceOn( x );
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
			Discard(spirit.Hand.Count-1);
			Discard(spirit.Hand.Count-1);
		}
		void Discard(int idx){
			spirit.DiscardPile.Add( spirit.Hand[idx] );
			spirit.Hand.RemoveAt( idx );
		}

		#endregion

		protected void When_Growing( int option) {
			spirit.Grow(gameState, option);
		}

		#region Asserts Presence

		protected void Assert_BoardPresenceIs( string expected ) {
			var actual = spirit.Presence.Placed.Select(s=>s.Label).OrderBy(l=>l).Join();
			Assert.Equal(expected, actual); // , Is.EqualTo(expected),"Presence in wrong place");
		}

		#endregion

		#region Asserts (Other)

		protected void Assert_HasCardAvailable( string name ){
			bool nameMatches( PowerCard card ) => string.Compare(name,card.Name,true) == 0;
			Assert.True(spirit.Hand.Any( nameMatches ),$"Hand does not contain {name}");
		}

		protected void Assert_HasPowerProgressionCard( int index ) {
			Assert_HasCardAvailable( (spirit.CardDrawer as PowerProgression).Cards[index].Name );
		}

		protected void Assert_AllCardsAvailableToPlay(int expectedAvailableCount = 4) {
			// Then: all cards reclaimed (including unplayed)
			Assert.Empty( spirit.DiscardPile ); // , "Should not be any cards in 'played' pile" );
			spirit.Hand.Count.ShouldBe( expectedAvailableCount );
		}

		protected void Assert_HasEnergy( int expectedChange ) {
			spirit.Energy.ShouldBe( expectedChange );
		}

		#endregion

		#region Resolve_

		protected void Resolve_PlacePresence(string placeOptions, Track source ) {
			spirit.Activate_PlacePresence(placeOptions,source);
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

		protected void Assert_BonusElements( string expectedElements ) {
			var spiritElements = spirit.Elements;
			foreach(var pair in GrowthTests.ElementChars) {
				int expected = expectedElements.Count( x => x == pair.Value );
				spiritElements[pair.Key].ShouldBe( expected, pair.Key.ToString() );
			}
		}

		protected void AndWhen_ReclaimingFirstCard() {
			var reclaim = spirit.Action;
			if(reclaim.GetCurrent().Options.Length>0)
				reclaim.Choose( reclaim.GetCurrent().Options[0] );
		}

		protected void When_StartingGrowth() {
			_ = new SelectGrowth( spirit, gameState ).ActAsync();
		}

		#endregion

	}

	static public class Spirit_Activate_Extensions {
		static public void Activate_DrawPowerCard(this Spirit spirit) {
			spirit.Action.Choose( "DrawPowerCard" );
		}

		static public void Activate_GainEnergy( this Spirit spirit ) {
			var current = spirit.Action.GetCurrent();
			var selection = current.Options.First(x=>x.Text.StartsWith("GainEnergy"));
			spirit.Action.Choose( selection );
		}

		static public void Activate_PlayExtraCard( this Spirit spirit ) {
			var current = spirit.Action.GetCurrent();
			var selection = current.Options.First( x => x.Text.StartsWith( "PlayExtra" ) );
			spirit.Action.Choose( selection );
		}


		static public void Activate_ReclaimAll( this Spirit spirit ) {
			// var current = spirit.Action.GetCurrent();
			spirit.Action.Choose( "ReclaimAll" );
		}

		static public void Activate_Reclaim1( this Spirit spirit ) {
			spirit.Action.Choose( "Reclaim(1)" );
		}

		static public void Activate_PlacePresence( this Spirit spirit, string placeOptions, Track source )
			=> spirit.Activate_PlacePresence( "PlacePre", placeOptions, source);

		static public void Activate_PlacePresence( this Spirit spirit, string promptStartsWith, string placeOptions, Track source ) {

			var current = spirit.Action.GetCurrent();

			var op = current.Options.First( o => o.Text.StartsWith( promptStartsWith ) );
			spirit.Action.Choose( op );

			// Resolve Power
			if(spirit.Presence.CardPlays.HasMore && spirit.Presence.Energy.HasMore) { // there are 2 option available
				var cardDecision = spirit.Action.GetCurrent();
				cardDecision.Options.Select( x => x.Text ).Join( "," ).ShouldContain( source.Text );
				// take from precense track
				spirit.Action.Choose( source );
			}

			// place on board - first option
			if(!spirit.Action.IsResolved) {
				string[] expectedOptions = placeOptions.Split( ';' );
				var destinationDecision = spirit.Action.GetCurrent();
				var actualOptions = destinationDecision.Options;
				var choice = actualOptions.SingleOrDefault( o => o.Text == expectedOptions[0] );
				if(choice==null)
					throw new System.ArgumentOutOfRangeException(nameof(placeOptions), $"'{expectedOptions[0]}' not found in "+ actualOptions.Select(o=>o.Text).Join("," ));
				spirit.Action.Choose( choice );
			}

		}

	}



}
