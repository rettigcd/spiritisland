using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SpiritIsland.Basegame;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class Invader_Tests {

		public Invader_Tests(){
			board = Board.BuildBoardA();
		}

		readonly Board board;
		GameState gameState;

		[Fact]
		public void StartsWithExplorer(){
			var deck = new InvaderDeck();
			Assert.NotNull(deck.Explore);
			Assert.Null(deck.Build);
			Assert.Null(deck.Ravage);
		}

		[Fact]
		public void AdvanceCards(){

			var deck = new InvaderDeck();

			// Advance the cards 12 times
			for(int i=0;i<11;++i){

				var explore = deck.Explore;
				var build = deck.Build;
				var ravage = deck.Ravage;
				int discardCount = deck.CountInDiscard;

				// When: advance the cards
				deck.Advance();

				// Then cards advance
				Assert.NotEqual(explore,deck.Explore);
				Assert.Equal(explore,deck.Build);
				Assert.Equal(build,deck.Ravage);
				Assert.Equal(discardCount+(ravage==null?0:1),deck.CountInDiscard);

			}
		}

		[Fact]
		public void CardsUsedAre_3L1_4L2_5L3() {
			var deck = new InvaderDeck();
			Assert_NextNCardsFromDeck( deck, InvaderDeck.Level1Cards, 3 );
			Assert_NextNCardsFromDeck( deck, InvaderDeck.Level2Cards, 4 );
			Assert_NextNCardsFromDeck( deck, InvaderDeck.Level3Cards, 5 );
		}

		[Theory]
		[InlineDataAttribute("M","A1,A6")]
		[InlineDataAttribute("J","A3,A8")]
		[InlineDataAttribute("W","A2,A5")]
		[InlineDataAttribute("S","A4,A7")]
		public void Level1CardTargets(string cardText,string expectedTargets){
			var card = InvaderDeck.Level1Cards.Single(c=>c.Text==cardText);
			var targets = board.Spaces.Where(card.Matches).Select(x=>x.Label).ToArray();
			Assert.Equal(expectedTargets,targets.Join(","));
		}

		[Theory]
		[InlineDataAttribute("M","A1,A6")]
		[InlineDataAttribute("J","A3,A8")]
		[InlineDataAttribute("W","A2,A5")]
		[InlineDataAttribute("S","A4,A7")]
		[InlineDataAttribute("Costal","A1,A2,A3")]
		public void Level2CardTargets(string cardText,string expectedTargets){
			var cards = InvaderDeck.Level2Cards.Where(c=>c.Text==cardText);
			var card = Assert.Single(cards);
			var targets = board.Spaces.Where(card.Matches).Select(x=>x.Label).ToArray();
			Assert.Equal(expectedTargets,targets.Join(","));
		}

		[Theory]
		[InlineDataAttribute("J+M","A1,A3,A6,A8")]
		[InlineDataAttribute("J+S","A3,A4,A7,A8")]
		[InlineDataAttribute("J+W","A2,A3,A5,A8")]
		[InlineDataAttribute("M+S","A1,A4,A6,A7")]
		[InlineDataAttribute("M+W","A1,A2,A5,A6")]
		[InlineDataAttribute("S+W","A2,A4,A5,A7")]
		public void Level3CardTargets(string cardText,string expectedTargets){
			var cards = InvaderDeck.Level3Cards.Where(c=>c.Text==cardText);
			var card = Assert.Single(cards);
			var targets = board.Spaces.Where(card.Matches).Select(x=>x.Label).ToArray();
			Assert.Equal(expectedTargets,targets.Join(","));
		}

		[Fact]
		public void DeckIsShuffled(){
			var origCards = NewDeckCards();
			var indxToCheck = new HashSet<int>{ 0,1,2,3,4,5,6,7,8,9,10,11};
			
			// try up to 10 different test decks
			for(int attempt=0;attempt<10;++attempt){
				var testDeck = NewDeckCards();
				// if test deck has a different card in the slot, that slot is shuffled
				for(int idx=0;idx<12;++idx)
					if(testDeck[idx] != origCards[idx])
						indxToCheck.Remove(idx);
				// if all slots are found to be shuffled, we are done
				if(indxToCheck.Count==0)
					break;
			}
			Assert.Empty(indxToCheck);
		}
		
		[Fact]
		public void NoTownsOrCities_HasStartingExplorer_ExploreCoast() {
			// Given: game on Board A
			var board = Board.BuildBoardA();
			var gameState = new GameState( new RiverSurges() ) { Island = new Island(board)	};
			//   And: explorer on target space
			gameState.Adjust(board[5],InvaderSpecific.Explorer,1);

			// When: exploring (wet lands
			gameState.Explore(InvaderDeck.Level1Cards.Single(c=>c.Text=="W"));

			// Then: 1 Explorer on A2 (new explored)
			//  and A5 (original) - proves explorers aren't reference types like towns
			foreach(var space in board.Spaces){
				var invaders = gameState.InvadersOn(space);
				Assert.Equal(space == board[5] || space == board[2]?1:0,invaders[InvaderSpecific.Explorer]);
			}
		}

		[Theory]
		[InlineData("A1","T@2")]
		[InlineData("A4","T@1")]
		[InlineData("A5","T@2")]
		[InlineData("A6","T@1")]
		[InlineData("A7","T@2")]
		[InlineData("A8","T@1")]
		[InlineData("A1","C@3")]
		[InlineData("A4","C@2")]
		[InlineData("A5","C@1")]
		[InlineData("A6","C@3")]
		[InlineData("A7","C@2")]
		[InlineData("A8","C@1")]
		public void InOrNextToTown_ExploresTownSpace(string townSpaceLabel,string invaderKey) {
			// Given: game on Board A
			var board = Board.BuildBoardA();
			var gameState = new GameState( new RiverSurges() ) { Island = new Island(board)	};
			//   And: Town on or next to wet land
			var sourceSpace = board.Spaces.Single(s=>s.Label==townSpaceLabel);
			var sourceInvader = InvaderSpecific.Lookup[invaderKey];
			gameState.Adjust(sourceSpace,sourceInvader,1);

			// When: exploring (wet lands
			gameState.Explore(InvaderDeck.Level1Cards.Single(c=>c.Text=="W"));

			// Then: Explores A2 and other space only
			foreach(var space in board.Spaces){
				var invaders = gameState.InvadersOn(space);
				Assert.Equal(
					space.Terrain == Terrain.Wetland?1:0
					,invaders[InvaderSpecific.Explorer]
				);
			}
		}

		[Theory]
		[InlineData("E@1","1T@2,1E@1")]
		[InlineData("T@2","1C@3,1T@2")]
		[InlineData("T@1","1C@3,1T@1")]
		[InlineData("C@3","1C@3,1T@2")]
		[InlineData("C@2","1C@2,1T@2")]
		[InlineData("C@1","1C@1,1T@2")]
		public void BuildInSpaceWithAnyInvader(string preInvaders,string endingInvaderCount) {
			// Given: game on Board A
			gameState = new GameState( new RiverSurges() ) { Island = new Island( board ) };
			//   And: invader on every space
			var startingInvader = InvaderSpecific.Lookup[preInvaders];
			foreach(var space in board.Spaces)
				gameState.Adjust( space, startingInvader, 1 );

			// When: build in Sand
			gameState.Build( InvaderDeck.Level1Cards.Single( c => c.Text == "S" ) );

			// Then: 2 Sand spaces should have ending Invader Count
			Assert.Equal( endingInvaderCount, gameState.InvadersOn( board[4] ).ToString() );
			Assert.Equal( endingInvaderCount, gameState.InvadersOn( board[7] ).ToString() );
			//  And: the other spaces have what they started with
			string origSummary = "1" + preInvaders;
			Assert_SpaceHasInvaders( board[1], origSummary );
			Assert_SpaceHasInvaders( board[2], origSummary );
			Assert_SpaceHasInvaders( board[3], origSummary );
			Assert_SpaceHasInvaders( board[5], origSummary );
			Assert_SpaceHasInvaders( board[6], origSummary );
			Assert_SpaceHasInvaders( board[8], origSummary );
		}

		void Assert_SpaceHasInvaders( Space space, string origSummary ) {
			Assert.Equal( origSummary, gameState.InvadersOn( space ).ToString() );
		}

		// Ravage
		// 1E@1 => 1E@1
		// 1D@2, 1E@1 => 1D@1       Dahan kills explorer
		// 1D@1, 1E@1 =>  1E@1      Explorer kills injured Dahan
		// 1D@2, 2E@1 => 2E@1       2 explorers kill dahan
		// 1D@2, 1T@2 => 1T@2       Town kills dahan
		// 3D@2, 2T@1 => 1D@2       2 towns kill 2 dahan, remaining dahan kills both towns.

		// given 1 point of damage,  Prefer C@1  ---  ---  T@1  ---  E@1 >>> T@2  C@2  C@3
		// given 2 points of damage, Prefer C@1  C@2  ---  T@1  T@2  E@1 >>> C@3
		// given 3 points of damage, Prefer C@1  C@2  C@3  T@1  T@2  E@1

		// Make sure Invaders Kill Dahan efficiently

		//// 3D@1, 1D@2 1C@3  => 1C@1,1T@2
		[Theory]
		[InlineData("3D@2,1T@2,1E@1","2D@2")] // !!! WRONG it should be 1D@2,1D@1 (1 dahan is damaged) - not implemented parital yet
		public async Task Ravage(string startingUnits,string endingUnits) {
			gameState = new GameState(new RiverSurges()) { Island = new Island( board ) };

			// Given: Invaders on a Mountain space
			var space = board[1];
			Assert.Equal(Terrain.Mountain,space.Terrain);
			Given_InitUnits( startingUnits, space );
			Assert_UnitsAre( startingUnits, space );

			// When: Ravaging in Mountains
			await gameState.Ravage(new InvaderCard(Terrain.Mountain));

			Assert_UnitsAre( endingUnits, space );
		}

		static InvaderCard[] NewDeckCards() {
			var deck = new InvaderDeck();
			var cards = new InvaderCard[12];
			for(int i = 0; i < 12; ++i) {
				cards[i] = deck.Explore;
				deck.Advance();
			}
			return cards;
		}

		static void Assert_NextNCardsFromDeck( InvaderDeck deck, ImmutableList<InvaderCard> cardSet, int count ) {
			for(int i = 0; i < count; ++i) {
				Assert.Contains( deck.Explore, cardSet );
				deck.Advance();
			}
		}

		void Assert_UnitsAre( string startingUnits, Space space ) {
			List<string> items = new();

			int dahanCount = gameState.DahanCount(space);
			if(dahanCount>0)
				items.Add($"{dahanCount}D@2");
			string actualInvaders = gameState.InvadersOn(space).ToString();

			if(actualInvaders.Length>0)
				items.Add(actualInvaders);

			items.Join( "," ).ShouldBe( startingUnits );
		}

		void Given_InitUnits( string startingUnits, Space space ) {
			foreach(var unit in startingUnits.Split( ',' )) {
				int count = unit[0] - '0';
				string itemSummary = unit[1..];
				if(itemSummary=="D@2"){
					gameState.AdjustDahan(space,count);
				} else {
					var invader = InvaderSpecific.Lookup[itemSummary];
					gameState.Adjust(space,invader,count);
				}
			}
		}

	}
}
