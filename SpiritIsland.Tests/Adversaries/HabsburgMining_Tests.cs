using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Adversaries;

public class HabsburgMining_Tests {

	public class UntappedSaltDeposits {

		[Fact]
		public async Task Explore_InLandsWith_FewerThan3(){
			// Given: playing Habsburg Mining Expetition - Level 4
			var board = Board.BuildBoardA();
			var gs = new GameState(new RiverSurges(),board);
			var adversary = new HabsburgMiningExpedition{ Level = 4 };
			// (1) Invader Deck
			gs.InvaderDeck = adversary.InvaderDeckBuilder.Build( 0 );
			gs.Initialize();

			//  And: the Untapped Salt Deposits is the next Explore Card (skipping over "11-2" levels )
			gs.Phase = Phase.Invaders;
			for(int i=0;i<3;++i){
				await InvaderPhase.ActAsync( gs ); // just do Invader phase 3 times
				gs.Given_InvadersDisappear(); // wiping away all the invaders each time
			}
			var sut = gs.InvaderDeck.Explore.Cards[0];
			sut.Text.ShouldBe("Salt Deposits");

			//  And: A4 is "almost" a mining land (but isn't)
			board[4].Given_HasTokens("2E@1");

			//  And: A5 is a mining land
			board[5].Given_HasTokens("3E@1");

			// Then: card should match every space but A5
			ActionScope.Current.Tokens.Where( s=>sut.MatchesCard(s.Space)).Select(ss=>ss.Space.Text).Join(",").ShouldBe("A1,A2,A3,A4,A6,A7,A8");
		}

		[Fact]
		public async Task Build_InLandsWith_FewerThan3(){
			// Given: playing Habsburg Mining Expetition - Level 4
			var board = Board.BuildBoardA();
			var gs = new GameState(new RiverSurges(),board);
			var adversary = new HabsburgMiningExpedition{ Level = 4 };
			// (1) Invader Deck
			gs.InvaderDeck = adversary.InvaderDeckBuilder.Build( 0 );
			gs.Initialize();

			//  And: the Untapped Salt Deposits is in the Build Slot (skipping over "11-2S" levels )
			gs.Phase = Phase.Invaders;
			for(int i=0;i<4;++i){
				await InvaderPhase.ActAsync( gs ); // just do Invader phase 3 times
				gs.Given_InvadersDisappear(); // wiping away all the invaders each time
			}
			var sut = gs.InvaderDeck.Build.Cards[0];
			sut.Text.ShouldBe("Salt Deposits");

			//  And: A4 is "almost" a mining land (but isn't)
			board[4].Given_HasTokens("2T@1");

			//  And: A5 is a mining land
			board[5].Given_HasTokens("3E@1");

			// Then: card should match every space but A5
			ActionScope.Current.Tokens.Where( s=>sut.MatchesCard(s.Space)).Select(ss=>ss.Space.Text).Join(",").ShouldBe("A1,A2,A3,A4,A6,A7,A8");
		}

		[Fact]
		public async Task Ravage_InLandsWith_3OrMore(){
			// Given: playing Habsburg Mining Expetition - Level 4
			var board = Board.BuildBoardA();
			var gs = new GameState(new RiverSurges(),board);
			var adversary = new HabsburgMiningExpedition{ Level = 4 };
			// (1) Invader Deck
			gs.InvaderDeck = adversary.InvaderDeckBuilder.Build( 0 );
			gs.Initialize();

			//  And: the Untapped Salt Deposits is in the Ravage Slot (skipping over "11-2S2" levels )
			gs.Phase = Phase.Invaders;
			for(int i=0;i<5;++i){
				await InvaderPhase.ActAsync( gs ); // just do Invader phase 3 times
				gs.Given_InvadersDisappear(); // wiping away all the invaders each time
			}
			var sut = gs.InvaderDeck.Ravage.Cards[0];
			sut.Text.ShouldBe("Salt Deposits");

			//  And: A4 is "almost" a mining land (but isn't)
			board[4].Given_HasTokens("2T@1");

			//  And: A5 is a mining land
			board[5].Given_HasTokens("3E@1");

			// Then: A5 should match, but nothing else
			bool matches = sut.MatchesCard(board[5]);
			ActionScope.Current.Tokens.Where( s=>sut.MatchesCard(s.Space)).Select(ss=>ss.Space.Text).Join(",").ShouldBe("A5");
		}


	}

}