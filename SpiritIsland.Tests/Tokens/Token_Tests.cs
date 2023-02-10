namespace SpiritIsland.Tests;

public class Token_Tests {

	static bool IsInPlay(Space space) => !space.IsOcean;

	[Fact]
	public void SummariesAreUnique() {
		var tokens = new ISpaceEntity[] {
			StdTokens.Explorer,
			StdTokens.Town1,StdTokens.Town,
			StdTokens.City1,StdTokens.City2,StdTokens.City,
			StdTokens.Dahan1,StdTokens.Dahan,
			StdTokens.Disease,
			Token.Blight, // conflict with Beast
			Token.Defend,
			Token.Beast,
			Token.Wilds
		};

		var conflicts = tokens
			.GroupBy( t => t.ToString() )
			.Where( grp => grp.Count() > 1 )
			.Select( grp => grp.Key + " is used for:" + grp.Select( t => t.Class.Label + ":" + (t is HumanToken ht ? ht.RemainingHealth : 0) ).Join( ", " ) )
			.Join( "\r\n" );

		conflicts.ShouldBe( "" );
	}

	[Trait("Token","Wilds")]
	[Fact]
	public void Wilds_Stops_Explore() {
		var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC());

		// Given: a space with no invaders
		SpaceState spaceState = gs.AllSpaces.First( s=>IsInPlay(s.Space) && !s.HasInvaders() );
		//   And: 1 neighboring town
		var neighbor = spaceState.Adjacent.First();
		neighbor.AdjustDefault( Human.Town, 1 );
		//   And: 1 wilds there
		spaceState.Wilds.Init(1);

		//  When: we explore there
		_ = spaceState.Space.DoAnExplore( gs );

		//  Then: still no invaders
		spaceState.HasInvaders().ShouldBeFalse("there should be no explorers in "+spaceState.Space.Label);
		//   And: no wilds here
		(spaceState.Wilds.Count>0).ShouldBeFalse("wilds should be used up");

	}

	[Trait( "Token", "Disease" )]
	[Fact]
	public async Task Disease_Stops_Build() {
		var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC() );

		// Given: a space with ONLY 1 explorer
		SpaceState space = gs.AllSpaces.First( s => IsInPlay(s.Space) && !s.HasInvaders() ); // 0 invaders
		space.AdjustDefault( Human.Explorer, 1 ); // add explorer
		//   And: 1 diseases there
		_ = gs.StartAction( ActionCategory.Default ); // !!! dispoose
		await space.Disease.Add(1);

		//  When: we build there
		await space.Space.DoABuild( gs );

		//  Then: still no towns (just original explorer)
		gs.Assert_Invaders(space.Space, "1E@1" ); // "should be no town on "+space.Label
		//   And: no disease here
		space.Disease.Any.ShouldBeFalse( "disease should be used up" );

	}

}