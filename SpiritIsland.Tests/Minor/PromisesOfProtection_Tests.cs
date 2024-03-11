namespace SpiritIsland.Tests.Minor;

public class PromisesOfProtection_Tests {

	[Trait("Token","Health")]
	[Fact]
	public async Task PromisesOfProtection_IsStackable() {

		// Setup
		var fxt = new ConfigurableTestFixture();
		var targetSpace = fxt.Board[5];
		var dahanSource = targetSpace.Adjacent_Existing.First();
		var selectDahanFromSource = "D@2";
		var ctx = fxt.Spirit.Target( targetSpace );

		// Test 1

		// Given: target spaces starts with 2 dahan
		fxt.InitTokens(targetSpace, "2D@2");
		//  And: adjacent dahan source has 4
		fxt.InitTokens(dahanSource, "4D@2");

		// When: playing card
		await PromisesOfProtection.ActAsync( ctx ).AwaitUser( user => { 
			user.NextDecision.HasPrompt("Gather up to (2)").MoveFrom( selectDahanFromSource );
			user.NextDecision.HasPrompt("Gather up to (1)").MoveFrom( selectDahanFromSource );
		} ).ShouldComplete();

		// Test 2

		// Then all 4 should have 4 health
		ctx.Space.Summary.ShouldBe("4D@4");

		// Brought in 2 more dahan
		await PromisesOfProtection.ActAsync( ctx ).AwaitUser( user => {
			user.NextDecision.HasPrompt("Gather up to (2)").MoveFrom( selectDahanFromSource );
			user.NextDecision.HasPrompt("Gather up to (1)").MoveFrom( selectDahanFromSource );
		} ).ShouldComplete();

		// Then: All (6) should have 6 health.
		ctx.Space.Summary.ShouldBe( "6D@6" );

		// Test 3

		// When round over
		await fxt.GameState.TriggerTimePasses();

		// Then all (6) should have 2 health
		ctx.Space.Summary.ShouldBe("6D@2");
	}

}

