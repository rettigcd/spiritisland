namespace SpiritIsland.Tests.BranchAndClaw;

public class Strife_Tests {

	readonly HealthToken city;
	readonly HealthToken strifedCity;

	public Strife_Tests() {
		city = Tokens.City;
		strifedCity = Tokens.City.HavingStrife( 1 );
	}

	[Fact]
	public void ReuseStrifeTokens() {
		city.HavingStrife( 1 ).ShouldBe( city.HavingStrife( 1 ), "Tokens for city with 1 strife should be reused for each instance." );
	}

	[Fact]
	public void WithStrifeOnlyUsesNonStrifedAsBase() {

		var strifed1 = city.HavingStrife( 2 );
		var strifed2 = strifedCity.HavingStrife( 2 );

		strifed1.ShouldBe( strifed2, "Make sure we don't use the strifed token as the base for generating." );
	}

	[Fact]
	public void LevelTokensAreDifferent() {
		strifedCity.ShouldNotBe( city, "they should be distinct" );
	}

	[Fact]
	public void LabelShowsStrife() {
		strifedCity.Summary.ShouldBe("C@3^");
	}

	[Fact]
	public void With0StrifeReturnsOriginal() {
		city.HavingStrife( 0).ShouldBe(city,"original should return original");
		strifedCity.HavingStrife( 0 ).ShouldBe( city, "strifed should return original" );
	}

	[Fact]
	public void CountStrife() {
		city.StrifeCount.ShouldBe(0,"orig has no strife");
		strifedCity.StrifeCount.ShouldBe(1,"strifed should have 1");
		city.HavingStrife( 2).StrifeCount.ShouldBe( 2, "strifed should have 2" );
	}

	[Fact]
	public void AddingStrife() {
		int expected = 0;
		HealthToken inv = city;
		void Add(int delta ) {
			expected+=delta;
			inv = inv.AddStrife(delta);
			inv.StrifeCount.ShouldBe( expected );
		}
		Add(1);
		Add(3);
		Add(-2);
	}

	[Fact]
	public void NegativeThrows() {
		Should.Throw<Exception>(()=>city.HavingStrife( -1));
	}

	[Fact]
	public void MoveStrife() {
		var board = Board.BuildBoardB();
		var gs = new GameState( new Shadows(), board );
		var space = board.Spaces.Skip( 1 ).First( x => !gs.Tokens[x].HasAny() );
		var counts = gs.Tokens[space];

		// Given: 1 town and 1 strifed town
		counts.Init( Tokens.Town, 2);
		counts.AddStrifeTo( Tokens.Town );
		var strifedTown = counts.OfType(Invader.Town).Single( k => k != Tokens.Town );

		// When: move
		var destination = space.Adjacent.First( x => !x.IsOcean );
		_ = gs.Tokens[space].MoveTo( strifedTown, destination ); // _ = ??

		// Then:
		counts.InvaderSummary.ShouldBe( "1T@2" );
		gs.Tokens[destination].InvaderSummary.ShouldBe( "1T@2^" );

	}

	// Add Strife to:
	// City
	// Town
	// Explorer
	[Theory]
	[InlineData( "2C@2", "C@2", "1C@2,1C@2^" )] // 2 cities, 1 gets strife
	[InlineData( "1C@2^", "C@2^", "1C@2^^" )] // strifed city gets 2nd strife
	[InlineData( "1C@3,1T@2", "1C@3,1T@2", "1C@3^,1T@2^" )] // strifed city gets 2nd strife
	public void Add_Strife( string startingInvaders, string addTo, string endingInvaders ) {
		var board = Board.BuildBoardB();
		var gs = new GameState( new Shadows(), board );
		var space = board.Spaces.Skip( 1 ).First( x => !gs.Tokens[x].HasAny() );
		var counts = gs.Tokens[space];

		var city2 = Tokens.City2;

		// Given: staring invaders
		switch(startingInvaders) {
			case "2C@2":  counts.Init( city2, 2); break;
			case "1C@2^": counts.Init( city2, 1); counts.AddStrifeTo( city2 ); break;
			case "1C@3,1T@2":
				counts.InitDefault(Invader.City, 1);
				counts.InitDefault(Invader.Town, 1);
				break;
			default: throw new Exception( "staring invaders [" + startingInvaders + "] not in list" );
		}

		// When: add strife
		switch(addTo) {
			case "C@2": counts.AddStrifeTo( city2 ); break;
			case "C@2^": counts.AddStrifeTo( city2.HavingStrife( 1 ) ); break;
			case "1C@3,1T@2":
				counts.AddStrifeTo( Tokens.City );
				counts.AddStrifeTo( Tokens.Town );
				break;
			default: throw new Exception( "add to not in list" );
		}

		// Then:
		counts.InvaderSummary.ShouldBe( endingInvaders );

	}


	[Fact]
	public async Task Strife_Stops_Ravage() {
		var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC() );
		Space space = gs.Island.AllSpaces
			.First( s => !s.IsOcean
				&& !gs.Tokens[s].HasInvaders() // 0 invaders
			);

		// Given: 1 strifed city
		var counts = gs.Tokens[space];
		counts.Init(Tokens.City.HavingStrife( 1 ), 1);
		counts.InvaderSummary.ShouldBe( "1C@3^", "strife should be used up" );

		//   and: 1 dahan
		gs.DahanOn( space ).Init( 1 );

		//  When: we ravage there
		await InvaderEngine1.RavageCard( InvaderCardEx.For( space ), gs );

		//  Then: dahan survives
		gs.DahanOn( space ).Count.ShouldBe( 1, "dahan should survive due to strife on town" );

		//   and so does city, but strife is gone
		counts.InvaderSummary.ShouldBe( "1C@1", "strife should be used up" );

	}

}