namespace SpiritIsland.Tests;

public class TigersHunting_Tests {

	[Fact]
	public void SingleAction() {
		var fixture = new ConfigurableTestFixture();

		HashSet<UnitOfWork> actionIds = new HashSet<UnitOfWork>();
		fixture.GameState.Tokens.TokenAdded.ForGame.Add(x=>actionIds.Add(x.Action));
		fixture.GameState.Tokens.TokenRemoved.ForGame.Add( x => actionIds.Add( x.Action ) );
		fixture.GameState.Tokens.TokenMoved.ForGame.Add( x => actionIds.Add( x.ActionId ) );

		// Given: space 5
		var space = fixture.GameState.Island.Boards[0][5];
		//   And: 1 explorer
		fixture.InitTokens(space,"1E@1");

		//  When: activate card
		var ctx = fixture.SelfCtx.Target( space );
		var task = TigersHunting.ActAsync( ctx );

		// 1 beast is added
		actionIds.Count.ShouldBe(1);

		// 1 damage -> destroys explorer
		fixture.Choose("E@1");
		actionIds.Count.ShouldBe( 1 );

		// push up to 2 beasts
		fixture.Choose("Beast"); // 'A' is selecting the beast
		fixture.Choose("A7");

		// Then everything was a single action. 
		actionIds.Count.ShouldBe(1);

		//  Then: it is complete and nothing happens.
		task.IsCompleted.ShouldBeTrue();

	}

}
