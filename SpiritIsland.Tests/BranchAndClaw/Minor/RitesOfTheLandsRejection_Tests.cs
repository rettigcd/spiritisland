namespace SpiritIsland.Tests.BranchAndClaw.Minor;

public class RitesOfTheLandsRejection_Tests {

	[Trait( "Feature", "Build" )]
	[Trait( "Feature", "InvaderCardProgression" )]
	[Theory]
	[InlineDataAttribute(false,"1T@2,1E@1")]
	[InlineDataAttribute(true,"1E@1")]
	public void SingleBuild(bool playsCard, string result) {

		var (user,ctx) = TestSpirit.SetupGame( PowerCard.For<RitesOfTheLandsRejection>() );

		// Given: find a space with 1 explorer
		var space = ctx.GameState.AllSpaces
			.First( s => s.InvaderSummary() == "1E@1" );

		//   And: add Dahan (because card requires it)
		space.Dahan.Init(1);

		// When: growing
		user.Grows();

		//  And: purchase test card
		user.PlaysCard( RitesOfTheLandsRejection.Name );
		if(playsCard) {
			user.SelectsFastAction( RitesOfTheLandsRejection.Name );
			user.TargetsLand_IgnoreOptions( space.Space.Label );
			user.AssertDecisionX( "Select Power Option", "{Stop build - 1 fear / (Dahan or T/C)},Push up to 3 Dahan", "{}" );
		} else
			//  And: done with fast (no more cards..)
			user.IsDoneWith( Phase.Fast );

		// Then: space should have a building
		System.Threading.Thread.Sleep(10);
		space.InvaderSummary().ShouldBe( result );

	}

	//		[Fact]
	//		public void Has2Builds_NoBuild() {} // !!! how do I construct 2 builds?

}
