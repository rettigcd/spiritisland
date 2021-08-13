﻿using System.Collections.Generic;
using System.Linq;
using Shouldly;
using SpiritIsland.Core;
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
			Assert.True(spirit.Hand.Any( nameMatches ),$"Hand does not contain {name}");
		}

		protected void Assert_AllCardsAvailableToPlay(int expectedAvailableCount = 4) {
			// Then: all cards reclaimed (including unplayed)
			Assert.Empty( spirit.DiscardPile ); // , "Should not be any cards in 'played' pile" );
			spirit.Hand.Count.ShouldBe( expectedAvailableCount );
		}

		protected void Assert_HasEnergy( int expectedChange ) {
			Assert.Equal( expectedChange, spirit.Energy ); // , $"Expected {expectedChange} energy change" );
		}

		#endregion

		#region Resolve_

		protected void Resolve_PlacePresence( string placeOptions, string factory=null) {
			Resolve_PlacePresence( placeOptions, spirit.EnergyTrack.Next, factory );
		}

		protected void Resolve_PlacePresence(string placeOptions, Track source, string factoryDescription=null ) {
			var ppFactory = spirit.GetUnresolvedActionFactories(Speed.Growth).OfType<PlacePresence>()
				.Where(f=> factoryDescription==null || factoryDescription == f.ShortDescription)
				.First();
			var engine = new ActionEngine( spirit, gameState );
			ppFactory.Activate( engine );
			var ppAction = new BaseAction(engine);

			if(engine.Self.CardTrack.HasMore && engine.Self.EnergyTrack.HasMore) // there are 2 option available
				// take from precense track
				ppAction.Select(source);

			// place on board - first option
			string[] options = placeOptions.Split(';');
			if(options.Length>1) // not auto selecting
				ppAction.Select(ppAction.Options.Single(o=>o.Text==options[0]));

			spirit.RemoveUnresolvedFactory(ppFactory);
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
				spiritElements[pair.Key].ShouldBe( expected );
			}
		}

		protected void AndWhen_ReclaimingFirstCard() {
			var engine = new ActionEngine( spirit, gameState );
			_ = spirit.GetUnresolvedActionFactories(Speed.Growth).OfType<Reclaim1>().First().Activate( engine );
			var reclaim = new BaseAction(engine);
			if(reclaim.Options.Length>0)
				reclaim.Select( reclaim.Options[0] );
		}

		#endregion

	}

}
