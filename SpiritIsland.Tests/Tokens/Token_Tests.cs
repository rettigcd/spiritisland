namespace SpiritIsland.Tests;

public class Token_Tests {

	static bool IsInPlay(SpaceSpec space) => !space.IsOcean;

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
			.Select( grp => grp.Key + " is used for:" + grp.OfType<IToken>().Select( t => t.Class.Label + ":" + (t is HumanToken ht ? ht.RemainingHealth : 0) ).Join( ", " ) )
			.Join( "\r\n" );

		conflicts.ShouldBe( "" );
	}

	[Trait("Token","Wilds")]
	[Fact]
	public async Task Wilds_Stops_Explore() {
		var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC());

		// Given: a space with no invaders
		Space space = ActionScope.Current.Spaces_Unfiltered.First( s=>IsInPlay(s.SpaceSpec) && !s.HasInvaders() );
		//   And: 1 neighboring town
		var neighbor = space.Adjacent.First();
		neighbor.AdjustDefault( Human.Town, 1 );
		//   And: 1 wilds there
		space.Wilds.Init(1);

		//  When: we explore there
		await space.SpaceSpec.When_Exploring();

		//  Then: still no invaders
		space.HasInvaders().ShouldBeFalse("there should be no explorers in "+space.SpaceSpec.Label);
		//   And: no wilds here
		(space.Wilds.Count>0).ShouldBeFalse("wilds should be used up");

	}

	[Trait( "Token", "Disease" )]
	[Fact]
	public async Task Disease_Stops_Build() {
		var gs = new GameState( new Thunderspeaker(), Board.BuildBoardC() );

		// Given: a space with ONLY 1 explorer
		Space space = ActionScope.Current.Spaces_Unfiltered.First( s => IsInPlay(s.SpaceSpec) && !s.HasInvaders() ); // 0 invaders
		space.Given_HasTokens("1E@1,1Z");

		//  When: we build there
		await space.SpaceSpec.When_Building();

		//  Then: still no towns (just original explorer)
		space.Assert_HasInvaders( "1E@1" ); // "should be no town on "+space.Label
		//   And: no disease here
		space.Disease.Any.ShouldBeFalse( "disease should be used up" );

	}

}