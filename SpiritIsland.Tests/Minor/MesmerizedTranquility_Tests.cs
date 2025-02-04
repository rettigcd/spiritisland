namespace SpiritIsland.Tests.Minor;

public class MesmerizedTranquility_Tests {

	[Trait("Invaders","Explore")]
	[Trait("Token","Isolation")]
	[Fact]
	public async Task ExplorersDoNoDamage() {
		var gs = new SoloGameState();
		var space = gs.Board[5];
		// Given: 1 dahan and 1 explorer
		space.Given_HasTokens("1D@2,1E@1");
		//   And: mesmerized played on that space
		await MesmerizedTranquility.ActAsync(gs.Spirit.Target(space)).ShouldComplete("Mesm Tranq");
		//  When: we ravage there
		await space.When_CardRavages();
		// Then: dahan is not damaged. + 1 Isolate (and explorer is gone)
		gs.Tokens[space].Summary.ShouldBe("1D@2,1I");
	}

	[Trait( "Invaders", "Ravage" )]
	[Fact]
	public async Task PlayedTwice_TownsDoNoDamage() {

		var gs = new SoloGameState();
		var spirit = gs.Spirit;
		var board = gs.Board;

		var space = board[5];
		// Given: 1 dahan and 1 town
		space.Given_HasTokens( "1D@2,1T@2" );
		//   And: mesmerized played on that space twice (via a repeat card)
		await MesmerizedTranquility.ActAsync( spirit.Target( space ) ).ShouldComplete( "Mesm Tranq1" );
		await MesmerizedTranquility.ActAsync( spirit.Target( space ) ).ShouldComplete( "Mesm Tranq2" );
		//  When: we ravage there
		await space.When_CardRavages();
		// Then: dahan is not damaged. + 1 Isolate (and town is gone)
		gs.Tokens[space].Summary.ShouldBe( "1D@2,1I" );
	}

}
