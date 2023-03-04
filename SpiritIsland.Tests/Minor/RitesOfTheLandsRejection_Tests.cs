namespace SpiritIsland.Tests.Minor;

public class RitesOfTheLandsRejection_Tests {

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineDataAttribute(false,"1T@2,1E@1")]
	[InlineDataAttribute(true,"1E@1")]
	public void SingleBuild(bool playsCard, string result) {

		var (user,ctx) = TestSpirit.StartGame( PowerCard.For<RitesOfTheLandsRejection>() );

		// Given: find a space with 1 explorer
		var space = ctx.GameState.Spaces_Unfiltered
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
			user.AssertDecisionInfo( "Select Power Option", "[Stop build - 1 fear / (Dahan or T/C)],Push up to 3 Dahan" );
		} else
			//  And: done with fast (no more cards..)
			user.IsDoneWith( Phase.Fast );

		// Then: space should have a building
		user.WaitForNext();
		space.InvaderSummary().ShouldBe( result );

	}

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public void Has2Builds_BothStopped() {

		var (user, ctx) = TestSpirit.StartGame( PowerCard.For<RitesOfTheLandsRejection>() );

		// Given: find a space with 1 explorer
		SpaceState space = ctx.GameState.Spaces_Unfiltered.First( s => s.InvaderSummary() == "1E@1" );
		//   And: add Dahan (because card requires it)
		space.Given_HasTokens("1D@2");
		//   And: The build card appears twice
		List<InvaderCard> buildCards = ctx.GameState.InvaderDeck.Build.Cards;
		buildCards.Add( buildCards[0] );

		// When: growing
		user.Grows();

		//  And: purchase test card
		user.PlaysCard( RitesOfTheLandsRejection.Name );
		user.SelectsFastAction( RitesOfTheLandsRejection.Name );
		user.Choose( space.Space.Label ); // target land
		user.NextDecision.HasPrompt("Select Power Option").Choose("Stop build - 1 fear / (Dahan or T/C)" );

		// Then: space should have a building
		user.WaitForNext();
		space.InvaderSummary().ShouldBe( "1E@1" );

	}

}
