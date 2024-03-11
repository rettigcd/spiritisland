namespace SpiritIsland.Tests.Minor;

public class SkiesHeraldTheSeasonOfReturn_Tests {

	[Trait("SpecialRule","ForbiddenGround")]
	[Fact]
	public async Task ReturningKeeperPresence_PushesDahan() {
		var fix = new ConfigurableTestFixture { Spirit = new Keeper() };
		SpaceSpec spaceSpec = fix.Board[5];
		Space space = fix.GameState.Tokens[spaceSpec];
		var dahanDestination = spaceSpec.Adjacent_Existing.First();

		// Given: Keeper has a destroyed presence
		fix.Spirit.Presence.Destroyed.Count = 1;

		//   And: a presence on target space.
		fix.Spirit.Given_IsOn( space );

		//   And: Dahan on space
		spaceSpec.ScopeSpace.Given_HasTokens("1D@2");

		//  When: play the card
		await PowerCard.For(typeof(SkiesHeraldTheSeasonOfReturn)).ActivateAsync( fix.Spirit ).AwaitUser( u => { 
			// target space
			u.NextDecision.Choose(spaceSpec);
			// Then: Should Push Dahan (per keeper's Sacred Site)
			u.NextDecision.HasPrompt("Push (1)").MoveFrom("D@2").MoveTo(dahanDestination.Label);
			//  And: May Gather up to 2 dahan (per the card)
			u.NextDecision.HasPrompt("Gather up to (1)").MoveFrom("D@2");
		}).ShouldComplete();

	}

}
