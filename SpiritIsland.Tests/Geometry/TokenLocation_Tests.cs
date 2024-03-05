namespace SpiritIsland.Tests.Geometry;

public class TokenLocation_Tests {

	// healthy-only stay in slot
	// T@2 > T@1
	// C@3 > C@2 > C@1
	// CS3,CSS2,CS1

	// When you need a new one,
	//	cancel old
	//  

	//  Each Invader/Strife gets a progression
	//  slot-0 of progression gets most damaged

	[Fact]
	public void RemembersInvaders() {
		var board = Board.BuildBoardA();

		GameState gs = new GameState(new RiverSurges(),board);
		Space a5 = board[5];
		var layout = new ManageInternalPoints( BoardLayout.Get( "A" ).ForSpace( a5 ) );

		SpaceState ss = gs.Tokens[a5];
		var explorer = ss.GetDefault(Human.Explorer);
		var town = ss.GetDefault(Human.Town);
		var city = ss.GetDefault(Human.City);
		ss.Given_HasTokens("1E@1,1T@2,1C@3");

		var ePoint = layout.Init( ss ).GetPointFor( explorer );
		var tPoint = layout.GetPointFor( town );
		var cPoint = layout.GetPointFor( city );

		// When: individual invaders are present
		// Then: each invader should be in its old location
		ss.Given_ClearAll().Given_HasTokens("1E@1");
		layout.Init( ss ).GetPointFor( explorer ).ShouldBe(ePoint);

		ss.Given_ClearAll().Given_HasTokens( "2T@2" );
		layout.Init( ss ).GetPointFor( town ).ShouldBe( tPoint );

		ss.Given_ClearAll().Given_HasTokens( "2C@3" );
		layout.Init( ss ).GetPointFor( city ).ShouldBe( cPoint );

		// When: town is damaged
		ss.Given_ClearAll().Given_HasTokens("1T@1,1T@2");
		// Then: Damaged town gets primary spot
		layout.Init( ss ).GetPointFor( town.AddDamage(1) ).ShouldBe( tPoint );
		//  And: healthy town gets some otherspot
		layout.GetPointFor( town ).ShouldNotBe( tPoint );

		// When: town heals and everything is healthy, it goes back to original spot
		ss.Given_ClearAll().Given_HasTokens("2T@2");
		layout.Init( ss ).GetPointFor( town ).ShouldBe( tPoint );

		// When: city is damaged once
		ss.Given_ClearAll().Given_HasTokens( "1C@2,2C@3" );
		layout.Init( ss ).GetPointFor( city.AddDamage( 1 ) ).ShouldBe( cPoint );
		var c2Point = layout.GetPointFor( city );

		ss.Given_ClearAll().Given_HasTokens( "1C@1,2C@3" );
		layout.Init(ss).GetPointFor( city.AddDamage( 2 ) ).ShouldBe( cPoint );
		layout.GetPointFor( city ).ShouldBe( c2Point );

	}



}
