using SpiritIsland.Basegame.Adversaries;

namespace SpiritIsland.Tests.Adversaries;

public class BrandonburgPrussia_Tests {

	readonly ConfigurableTestFixture fxt = new ConfigurableTestFixture();

	[Fact]
	public void Level0() {
		// When: game starts
		_ = fxt.GameState;

		// Then: no town on space 3
		Assert_Level1TownAdded( false );

		Assert_FearLevels( 3, 3, 3 );
		Assert_InvaderDeck( defaultInvaderDeck );
	}

	[Fact]
	public void Level1() {
		// 3,3,3, "Fast Start", "During Setup, on each board add 1 town to land #3"

		// Given: BrandenBurg Prussia
		SetAdversary( 1 );

		// When: game starts
		_ = fxt.GameState;

		// Then: 1 town on space 3
		Assert_Level1TownAdded( true );

		Assert_FearLevels( 3, 3, 3 );
		Assert_InvaderDeck( defaultInvaderDeck );

	}

	[Fact]
	public void Level2() {
		// (3/3/3)	Surge of Colonists:	(111-3-2222-3333)

		// Given: BrandenBurg Prussia
		SetAdversary(2);

		// When: game starts

		Assert_Level1TownAdded( true );
		Assert_FearLevels( 3, 3, 3 );
		Assert_InvaderDeck( "111-3-2222-3333" );

	}

	[Trait( "Feature", "Fear" )]
	[Fact]
	public void Level3() {
		// 10 (3/4/3)	Efficient:				(11-3-2222-3333)

		// Given: BrandenBurg Prussia
		SetAdversary( 3 );

		// When: game starts

		Assert_Level1TownAdded( true );
		Assert_FearLevels( 3, 4, 3 );
		Assert_InvaderDeck( "11-3-2222-3333" );
	}

	[Trait( "Feature", "Fear" )]
	[Fact]
	public void Level4() {
		// 11 (4/4/3)	Agressive Timetable:	(11-3-222-3333)

		// Given: BrandenBurg Prussia
		SetAdversary( 4 );

		// When: game starts

		Assert_Level1TownAdded( true );
		Assert_FearLevels( 4, 4, 3 );
		Assert_InvaderDeck( "11-3-222-3333" );
	}

	[Trait( "Feature", "Fear" )]
	[Fact]
	public void Level5() {
		// 11 (4/4/3)	Ruthlessly Efficent:	(1-3-222-3333)

		// Given: BrandenBurg Prussia
		SetAdversary( 5 );

		// When: game starts

		Assert_Level1TownAdded( true );
		Assert_FearLevels( 4, 4, 3 );
		Assert_InvaderDeck( "1-3-222-3333" );
	}

	[Trait( "Feature", "Fear" )]
	[Fact]
	public void Level6() {
		// 12 (4/4/4)	Terrifying Efficient:	(3-222-3333)

		// Given: BrandenBurg Prussia
		SetAdversary( 6 );

		// When: game starts

		Assert_Level1TownAdded( true );
		Assert_FearLevels( 4, 4, 4 );
		Assert_InvaderDeck( "3-222-3333" );
	}


	void SetAdversary(int level) => fxt.InitConfiguration( cfg => { 
		cfg.AdversaryType = typeof( BrandenburgPrussia ); 
		cfg.AdversaryLevel = level; 
	} );

	void Assert_Level1TownAdded( bool added ) 
		=> fxt.GameState.Tokens[fxt.Board[3]].Summary.ShouldBe( added ? "2D@2,1T@2" : "[none]" );

	void Assert_FearLevels( int v1, int v2, int v3 ) {
		// level 1
		for(int i = 0; i < v1; ++i) {
			fxt.GameState.Fear.TerrorLevel.ShouldBe(1);
			fxt.GameState.Fear.Deck.Pop();
		}
		// level 2
		for(int i = 0; i < v2; ++i) {
			fxt.GameState.Fear.TerrorLevel.ShouldBe( 2 );
			fxt.GameState.Fear.Deck.Pop();
		}
		// level 3
		for(int i = 0; i < v3; ++i) {
			fxt.GameState.Fear.TerrorLevel.ShouldBe( 3 );
			fxt.GameState.Fear.Deck.Pop();
		}
		// last card
		fxt.GameState.Fear.Deck.Count.ShouldBe( 0 );
	}

	const string defaultInvaderDeck = "111-2222-33333";
	void Assert_InvaderDeck( string expectedLevelsString ) {
		string actualInvaderLevels = GetActualInvaderLevels( fxt.GameState.InvaderDeck );
		actualInvaderLevels.ShouldBe( expectedLevelsString );
	}

	static string GetActualInvaderLevels( InvaderDeck deck ) { // make extension method??
		var buf = new System.Text.StringBuilder();
		char last = ' '; // deck.Explore.Single().InvaderStage.ToString()[0];

		var cards = deck.Slots.SelectMany(s=>s.Cards).Union( deck.UnrevealedCards );

		foreach(var card in cards) {
			var next = card.InvaderStage.ToString()[0];
			if(next != last && last != ' ') buf.Append( '-' );
			last = next;
			buf.Append( last );
		}
		return buf.ToString();
	}
}

