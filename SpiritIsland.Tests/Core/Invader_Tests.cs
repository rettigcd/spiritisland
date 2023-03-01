namespace SpiritIsland.Tests.Core;

public class Invader_Tests {

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

	public Invader_Tests(){
		ActionScope.Initialize();
		board = Board.BuildBoardA();
	}

	readonly Board board;
	GameState gameState;

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
	public void AdvanceCards(){

		var sut = InvaderDeckBuilder.Default.Build();

		// Advance the cards 12 times
		for(int i=0;i<11;++i){

			var explore = sut.Explore.Cards.ToArray();
			var build = sut.Build.Cards.ToArray();
			var ravage = sut.Ravage.Cards.ToArray();
			int discardCount = sut.Discards.Count;

			// When: advance the cards
			sut.Advance();

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
		InvaderCard sut = InvaderDeckBuilder.Level1Cards.Single(c=>c.Text==cardText);
		var targets = board.Spaces.Where(((InvaderCard)sut).MatchesCard).Select(x=>x.Label).ToArray();
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
		var cards = InvaderDeckBuilder.Level2Cards.Where(c=>c.Text==cardText);
		var sut = Assert.Single(cards);
		var targets = board.Spaces.Where(sut.MatchesCard).Select(x=>x.Label).ToArray();
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
		var cards = InvaderDeckBuilder.Level3Cards.Where(c=>c.Text==cardText);
		var sut = Assert.Single(cards);
		var targets = board.Spaces.Where(sut.MatchesCard).Select(x=>x.Label).ToArray();
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
	public void NoTownsOrCities_HasStartingExplorer_ExploreCoast() {
		// Given: game on Board A
		var board = Board.BuildBoardA();
		var gameState = new GameState( new RiverSurges(), board );
		//   And: explorer on target space
		gameState.Tokens[ board[5] ].AdjustDefault(Human.Explorer,1);

		// When: exploring (wet lands
		new ExploreSlot().ActivateCard( InvaderDeckBuilder.Level1Cards.Single(c=>c.Text=="W"), gameState )
			.FinishUp("Explore Phase");

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
	public void BuildInSpaceWithAnyInvader(string preInvaders,string endingInvaderCount) {
		// Given: game on Board A
		gameState = new GameState( new RiverSurges(), board );
		//   And: invader on every space
		var startingInvader = Parse(preInvaders);
		foreach(var space in board.Spaces)
			gameState.Tokens[space].Adjust( startingInvader, 1 );

		// When: build in Sand
		new BuildSlot().ActivateCard( InvaderDeckBuilder.Level1Cards.Single( c => c.Text == "S" ), gameState)
			.FinishUp("Build Phase");

		// Then: 2 Sand spaces should have ending Invader Count
		gameState.Assert_Invaders( board[4], endingInvaderCount );
		gameState.Assert_Invaders( board[7], endingInvaderCount );
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
		gameState.Assert_Invaders(space,origSummary);
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
	[InlineData("3D@2,1T@2,1E@1","1D@2,1D@1")]
	public async Task Ravage(string startingUnits,string endingUnits) {
		gameState = new GameState( new RiverSurges(), board );
		gameState.IslandWontBlight();
		// Disable destroying presence
		gameState.DisableBlightEffect();

		// Given: Invaders on a Mountain space
		var space = board[1];
		Assert.True(space.IsMountain);
		Given_InitUnits( startingUnits, space );
		Assert_UnitsAre( startingUnits, space );

		// When: Ravaging in Mountains
		InvaderCard.Stage1( Terrain.Mountain ).When_Ravaging();
		// await new RavageEngine().ActivateCard( InvaderCard.Stage1( Terrain.Mountain ), gameState );

		Assert_UnitsAre( endingUnits, space );
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

	void Assert_UnitsAre( string startingUnits, Space space ) {
		List<string> items = new();

		var tokens = gameState.Tokens[space];

		foreach(var token in tokens.Dahan.NormalKeys.OrderByDescending( x => x.RemainingHealth ))
			items.Add( $"{tokens[token]}D@{token.RemainingHealth}" );

		string actualInvaders = tokens.InvaderSummary();
		if(actualInvaders.Length>0)
			items.Add(actualInvaders);

		items.Join( "," ).ShouldBe( startingUnits );
	}

	void Given_InitUnits( string startingUnits, Space space ) {
		foreach(var unit in startingUnits.Split( ',' )) {
			int count = unit[0] - '0';
			string itemSummary = unit[1..];
			if(itemSummary=="D@2"){
				space.Tokens.Dahan.Init(count);
			} else {
				var invader = Parse(itemSummary);
				gameState.Tokens[space].Adjust(invader,count);
			}
		}
	}

}
