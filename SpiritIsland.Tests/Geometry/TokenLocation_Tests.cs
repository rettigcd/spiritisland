using System.Drawing;

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
		var gs = new GameState(new RiverSurges(),board);
		var ss = gs.Tokens[board[5]];
		var layout = new ManageInternalPoints( ss );

		var explorer = ss.GetDefault(Invader.Explorer);
		var town = ss.GetDefault(Invader.Town);
		var city = ss.GetDefault(Invader.City);
		ss.InitTokens("1E@1,1T@2,1C@3");

		var ePoint = layout.Init( ss ).GetPointFor( explorer );
		var tPoint = layout.GetPointFor( town );
		var cPoint = layout.GetPointFor( city );

		// When: individual invaders are present
		// Then: each invader should be in its old location
		ss.Clear().InitTokens("1E@1");
		layout.Init( ss ).GetPointFor( explorer ).ShouldBe(ePoint);

		ss.Clear().InitTokens( "2T@2" );
		layout.Init( ss ).GetPointFor( town ).ShouldBe( tPoint );

		ss.Clear().InitTokens( "2C@3" );
		layout.Init( ss ).GetPointFor( city ).ShouldBe( cPoint );

		// When: town is damaged
		ss.Clear().InitTokens("1T@1,1T@2");
		// Then: Damaged town gets primary spot
		layout.Init( ss ).GetPointFor( town.AddDamage(1) ).ShouldBe( tPoint );
		//  And: healthy town gets some otherspot
		layout.GetPointFor( town ).ShouldNotBe( tPoint );

		// When: town heals and everything is healthy, it goes back to original spot
		ss.Clear().InitTokens("2T@2");
		layout.Init( ss ).GetPointFor( town ).ShouldBe( tPoint );

		// When: city is damaged once
		ss.Clear().InitTokens( "1C@2,2C@3" );
		layout.Init( ss ).GetPointFor( city.AddDamage( 1 ) ).ShouldBe( cPoint );
		var c2Point = layout.GetPointFor( city );

		ss.Clear().InitTokens( "1C@1,2C@3" );
		layout.Init(ss).GetPointFor( city.AddDamage( 2 ) ).ShouldBe( cPoint );
		layout.GetPointFor( city ).ShouldBe( c2Point );

	}



}
