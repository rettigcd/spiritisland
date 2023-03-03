namespace SpiritIsland.Tests.Minor;

public class MesmerizedTranquility_Tests {

	[Trait("Invaders","Explore")]
	[Trait("Token","Isolation")]
	[Fact]
	public async Task ExplorersDoNoDamage() {
		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: 1 dahan and 1 explorer
		fxt.InitTokens(space,"1D@2,1E@1");
		//   And: mesmerized played on that space
		await MesmerizedTranquility.ActAsync(fxt.SelfCtx.Target(space)).ShouldComplete("Mesm Tranq");
		//  When: we ravage there
		await space.When_Ravaging();
		// Then: dahan is not damaged. + 1 Isolate (and explorer is gone)
		fxt.GameState.Tokens[space].Summary.ShouldBe("1D@2,1I");
	}

	[Trait( "Invaders", "Ravage" )]
	[Fact]
	public async Task PlayedTwice_TownsDoNoDamage() {
		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: 1 dahan and 1 town
		fxt.InitTokens( space, "1D@2,1T@2" );
		//   And: mesmerized played on that space twice (via a repeat card)
		await MesmerizedTranquility.ActAsync( fxt.SelfCtx.Target( space ) ).ShouldComplete( "Mesm Tranq1" );
		await MesmerizedTranquility.ActAsync( fxt.SelfCtx.Target( space ) ).ShouldComplete( "Mesm Tranq2" );
		//  When: we ravage there
		await space.When_Ravaging();
		// Then: dahan is not damaged. + 1 Isolate (and town is gone)
		fxt.GameState.Tokens[space].Summary.ShouldBe( "1D@2,1I" );
	}

}
