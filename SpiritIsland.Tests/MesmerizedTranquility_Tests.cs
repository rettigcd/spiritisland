using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests;

public class MesmerizedTranquility_Tests {

	[Fact]
	public void ExplorersDoNoDamage() {
		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: 1 dahan and 1 explorer
		fxt.InitTokens(space,"1D@2,1E@1");
		//   And: mesmerized played on that space
		MesmerizedTranquility.ActAsync(fxt.SelfCtx.Target(space)).Wait();
		//  When: we ravage there
		new RavageAction( fxt.GameState, fxt.GameState.Invaders.On(space, Guid.NewGuid()) ).Exec().Wait();
		// Then: dahan is not damaged. + 1 Isolate (and explorer is gone)
		fxt.GameState.Tokens[space].Summary.ShouldBe("1D@2,1I");
	}

	[Fact]
	public void PlayedTwice_TownsDoNoDamage() {
		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: 1 dahan and 1 town
		fxt.InitTokens( space, "1D@2,1T@2" );
		//   And: mesmerized played on that space twice (via a repeat card)
		MesmerizedTranquility.ActAsync( fxt.SelfCtx.Target( space ) ).Wait();
		MesmerizedTranquility.ActAsync( fxt.SelfCtx.Target( space ) ).Wait();
		//  When: we ravage there
		new RavageAction( fxt.GameState, fxt.GameState.Invaders.On( space, Guid.NewGuid() ) ).Exec().Wait();
		// Then: dahan is not damaged. + 1 Isolate (and town is gone)
		fxt.GameState.Tokens[space].Summary.ShouldBe( "1D@2,1I" );
	}

}
