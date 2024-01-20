namespace SpiritIsland.Tests.Invaders;

public class InvaderDeck_Tests {

	static ISpaceEntity Parse(string s ) {
		return s switch {
			"C@3" => StdTokens.City,
			"C@2" => StdTokens.City2,
			"C@1" => StdTokens.City1,
			"T@2" => StdTokens.Town,
			"T@1" => StdTokens.Town1,
			"E@1" => StdTokens.Explorer,
			_=>throw new FormatException("unknown invader format: "+s),
		};
	}

	public InvaderDeck_Tests(){
		_board = Board.BuildBoardA();
	}

	readonly Board _board;
	GameState _gameState;

	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void StartsWithExplorer(){
		var sut = InvaderDeckBuilder.Default.Build();
		Assert.NotNull(sut.Explore.Cards);
		Assert.Empty(sut.Build.Cards);
		Assert.Empty(sut.Ravage.Cards);
	}

	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task CanAdvanceCards(){

		var sut = InvaderDeckBuilder.Default.Build();

		// Advance the cards 12 times
		for(int i=0;i<11;++i){

			var explore = sut.Explore.Cards.ToArray();
			var build = sut.Build.Cards.ToArray();
			var ravage = sut.Ravage.Cards.ToArray();
			int discardCount = sut.Discards.Count;

			// When: advance the cards
			await sut.AdvanceAsync();

			// Then cards advance
			Assert.NotEqual(explore,sut.Explore.Cards);
			Assert.Equal(explore,sut.Build.Cards);
			Assert.Equal(build,sut.Ravage.Cards);
			Assert.Equal(discardCount+(ravage.Length==0?0:1),sut.Discards.Count);

		}
	}

	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void CardsUsedAre_3L1_4L2_5L3() {
		var sut = InvaderDeckBuilder.Default.Build().UnrevealedCards.ToList();
		Assert_NextNCardsFromDeck( sut, InvaderDeckBuilder.Level1Cards, 3 );
		Assert_NextNCardsFromDeck( sut, InvaderDeckBuilder.Level2Cards, 4 );
		Assert_NextNCardsFromDeck( sut, InvaderDeckBuilder.Level3Cards, 5 );
	}

	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineDataAttribute("M","A1,A6")]
	[InlineDataAttribute("J","A3,A8")]
	[InlineDataAttribute("W","A2,A5")]
	[InlineDataAttribute("S","A4,A7")]
	public void Level1CardTargets(string cardText,string expectedTargets){
		var gs = new GameState(new RiverSurges(),_board);
		InvaderCard sut = InvaderDeckBuilder.Level1Cards.Single(c=>c.Text==cardText);
		string[] targets = _board.Spaces.Where(sut.MatchesCard).Select(x=>x.Label).ToArray();
		Assert.Equal(expectedTargets,targets.Join(","));
	}

	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineDataAttribute("2M","A1,A6")]
	[InlineDataAttribute("2J","A3,A8")]
	[InlineDataAttribute("2W","A2,A5")]
	[InlineDataAttribute("2S","A4,A7")]
	[InlineDataAttribute("Coastal","A1,A2,A3")]
	public void Level2CardTargets(string cardText,string expectedTargets){
		var gs = new GameState( new RiverSurges(), _board ); // Init Scope and GameBoard
		var cards = InvaderDeckBuilder.Level2Cards.Where(c=>c.Text==cardText);
		var sut = Assert.Single(cards);
		var targets = _board.Spaces.Where(sut.MatchesCard).Select(x=>x.Label).ToArray();
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
		var gs = new GameState( new RiverSurges(), _board ); // Init Scope and GameBoard
		var cards = InvaderDeckBuilder.Level3Cards.Where(c=>c.Text==cardText);
		var sut = Assert.Single(cards);
		var targets = _board.Spaces.Where(sut.MatchesCard).Select(x=>x.Label).ToArray();
		Assert.Equal(expectedTargets,targets.Join(","));
	}

	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void DeckIsShuffled(){
		var origCards = NewDeckCards(0);
		var indxToCheck = new HashSet<int>{ 0,1,2,3,4,5,6,7,8,9,10,11};
			
		// try up to 6 different test decks
		var random = new Random();
		for(int attempt=0;attempt<6;++attempt){
			var testDeck = NewDeckCards( random.Next() );
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

	[Trait( "Invaders", "Explore" )]
	[Fact]
	public async Task NoTownsOrCities_HasStartingExplorer_ExploreCoast() {
		// Given: game on Board A
		var board = Board.BuildBoardA();
		var gameState = new GameState( new RiverSurges(), board );
		//   And: explorer on target space
		gameState.Tokens[ board[5] ].AdjustDefault(Human.Explorer,1);

		// When: exploring (wet lands
		await new ExploreSlot().ActivateCard( InvaderDeckBuilder.Level1Cards.Single(c=>c.Text=="W"), gameState )
			.ShouldComplete( "Explore Phase");

		// Then: 1 Explorer on A2 (new explored)
		//  and A5 (original) - proves explorers aren't reference types like towns
		foreach(var space in board.Spaces){
			var invaders = gameState.Tokens[space];
			Assert.Equal(space == board[5] || space == board[2]?1:0,invaders[StdTokens.Explorer ] );
		}
	}

	[Trait( "Invaders", "Explore" )]
	[Theory]
	[InlineData( "A1", "T@2" )]
	[InlineData( "A1", "C@3" )]
	[InlineData( "A4", "C@2" )]
	[InlineData( "A4", "T@1" )]
	[InlineData( "A5", "C@1" )]
	[InlineData( "A5", "T@2" )]
	[InlineData( "A6", "C@3" )]
	[InlineData( "A6", "T@1" )]
	[InlineData( "A7", "T@2" )]
	[InlineData( "A7", "C@2" )]
	[InlineData( "A8", "T@1" )]
	[InlineData( "A8", "C@1" )]
	public async Task InOrNextToTown_ExploresTownSpace(string townSpaceLabel,string invaderKey) {
		// Given: game on Board A
		var board = Board.BuildBoardA();
		var gameState = new GameState( new RiverSurges(), board );

		//   And: Town on or next to A5 (a wet land)
		var sourceSpace = board.Spaces.Single(s=>s.Label==townSpaceLabel);
		var sourceInvader = Parse(invaderKey);
		gameState.Tokens[sourceSpace].Adjust(sourceInvader,1);

		var log = new List<string>();
		gameState.NewLogEntry += (e) => log.Add(e.Msg(Log.LogLevel.Info));

		// When: exploring (wet lands
		InvaderCard card = InvaderDeckBuilder.Level1Cards.Single( c => c.Text == "W" );
		await new ExploreSlot().ActivateCard( card, gameState );

		// Then: Explores A2 and other space only
		foreach(var space in board.Spaces){
			var invaders = gameState.Tokens[space];
			invaders[StdTokens.Explorer].ShouldBe( space.IsWetland ? 1 : 0, space.Text );
		}
	}

	[Trait( "Invaders", "Build" )]
	[Theory]
	[InlineData("E@1","1T@2,1E@1")]
	[InlineData("T@2","1C@3,1T@2")]
	[InlineData("T@1","1C@3,1T@1")]
	[InlineData("C@3","1C@3,1T@2")]
	[InlineData("C@2","1C@2,1T@2")]
	[InlineData("C@1","1C@1,1T@2")]
	public async Task BuildInSpaceWithAnyInvader(string preInvaders,string endingInvaderCount) {
		// Given: game on Board A
		_gameState = new GameState( new RiverSurges(), _board );
		//   And: invader on every space
		var startingInvader = Parse(preInvaders);
		foreach(var space in _board.Spaces)
			_gameState.Tokens[space].Adjust( startingInvader, 1 );

		// When: build in Sand
		await new BuildSlot().ActivateCard( InvaderDeckBuilder.Level1Cards.Single( c => c.Text == "S" ), _gameState)
			.ShouldComplete( "Build Phase");

		// Then: 2 Sand spaces should have ending Invader Count
		_board[4].Assert_HasInvaders( endingInvaderCount );
		_board[7].Assert_HasInvaders( endingInvaderCount );
		//  And: the other spaces have what they started with
		string origSummary = "1" + preInvaders;
		_board[1].Assert_HasInvaders( origSummary );
		_board[2].Assert_HasInvaders( origSummary );
		_board[3].Assert_HasInvaders( origSummary );
		_board[5].Assert_HasInvaders( origSummary );
		_board[6].Assert_HasInvaders( origSummary );
		_board[8].Assert_HasInvaders( origSummary );
	}

	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Memento_RoundTrip() {

		// Given: a deck in some advanced state
		var sut = InvaderDeckBuilder.Default.Build();
		await AdvanceAsync( sut );

		//   And: we have saved the desired state
		var memento = sut.Memento;
		string expected = TakeSnapShot( sut );

		//   And: advanced beyond the saved state
		await AdvanceAsync( sut );

		//  When: we restore the saved state
		sut.Memento = memento;

		// Then: we should get back the expted state
		TakeSnapShot( sut ).ShouldBe( expected );

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
	[Trait( "Invaders", "Ravage" )]
	[Theory]
	[InlineData("3D@2,1E@1,1T@2","1B,1D@1,1D@2")]
	public async Task Ravage(string startingUnits,string endingUnits) {
		_gameState = new GameState( new RiverSurges(), _board );
		_gameState.IslandWontBlight();
		// Disable destroying presence
		_gameState.DisableBlightEffect();

		// Given: Invaders on a Mountain space
		var space = _board[1];
		Assert.True(space.IsMountain);
		space.Given_HasTokens(startingUnits);
		space.Tokens.Summary.ShouldBe( startingUnits );

		// When: Ravaging in Mountains
		await InvaderCard.Stage1( Terrain.Mountain ).When_Ravaging();
		// await new RavageEngine().ActivateCard( InvaderCard.Stage1( Terrain.Mountain ), gameState );

		space.Tokens.Summary.ShouldBe( endingUnits );
	}

	static async Task AdvanceAsync( InvaderDeck sut ) {
		await sut.AdvanceAsync();
		await sut.AdvanceAsync();
		await sut.AdvanceAsync();
	}

	static string TakeSnapShot( InvaderDeck sut ) {
		//   And: record cards
		return sut.Ravage.Cards[0].Text + " : " + sut.Build.Cards[0].Text + " : " + sut.Explore.Cards[0].Text;
	}
	
	static InvaderCard[] NewDeckCards(int seed) {
		return InvaderDeckBuilder.Default.Build( seed ).UnrevealedCards.ToArray();
	}

	static void Assert_NextNCardsFromDeck( List<InvaderCard> deck, ImmutableList<InvaderCard> expected, int count ) {
		var expectedTitles = expected.Select( x => x.Text ).ToArray();
		for(int i = 0; i < count; ++i) {
			Assert.Contains( deck[0].Text, expectedTitles ); // because new cards are generated each time.
			deck.RemoveAt(0);
		}
	}

}
