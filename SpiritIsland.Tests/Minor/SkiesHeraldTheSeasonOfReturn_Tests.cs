namespace SpiritIsland.Tests.Minor;

public class SkiesHeraldTheSeasonOfReturn_Tests {

	[Trait("SpecialRule","ForbiddenGround")]
	[Fact]
	public async Task ReturningKeeperPresence_PushesDahan() {
		var fix = new ConfigurableTestFixture { Spirit = new Keeper() };
		var space = fix.Board[5];
		var spaceState = fix.GameState.Tokens[space];
		var dahanDestination = space.Adjacent_Existing.First();

		// Given: Keeper has a destroyed presence
		fix.Spirit.Presence.Destroyed.Count = 1;

		//   And: a presence on target space.
		fix.Spirit.Given_IsOn( spaceState );

		//   And: Dahan on space
		space.ScopeTokens.Given_HasTokens("1D@2");

		//  When: play the card
		await PowerCard.For(typeof(SkiesHeraldTheSeasonOfReturn)).ActivateAsync( fix.Spirit ).AwaitUser( u => { 
			// target space
			u.NextDecision.Choose(space);
			// Then: Should Push Dahan (per keeper's Sacred Site)
			u.NextDecision.HasPrompt("Push (1)").MoveFrom("D@2").MoveTo(dahanDestination.Text);
			//  And: May Gather up to 2 dahan (per the card)
			u.NextDecision.HasPrompt("Gather up to (1)").MoveFrom("D@2");
		}).ShouldComplete();

	}

}
