namespace SpiritIsland.Tests.Spirits.OceanNS;

public class SwallowTheLandDwellers_Tests {

	[Trait( "SpecialRule", "Drowning" )]
	[Fact]
	public void SwallowTheLandDwellers_DrownsInvaders() {
		// Game with ocean-only
		var fxt = new ConfigurableTestFixture();
		fxt.Spirit = new Ocean();
		fxt.Board = Board.BuildBoardA();
		fxt.GameState.Initialize();
		int startingEnergy = fxt.Spirit.Energy;

		// Given: Ocean on A2
		var space = fxt.GameState.Tokens[fxt.Board[2]];
		fxt.Spirit.Presence.Adjust( space, 1 );
		//   And: explorer and town on A2
		space.InitDefault( Invader.Explorer, 1 );
		space.InitDefault( Invader.Town, 1 );

		// When: we use Swallow the Land Dwellers
		fxt.Spirit.AddActionFactory( PowerCard.For<SwallowTheLandDwellers>() );
		fxt.GameState.Phase = Phase.Slow;
		Task task = fxt.Spirit.ResolveActions( fxt.GameState );

		System.Threading.Thread.Sleep( 1 );
		fxt.Choose( SwallowTheLandDwellers.Name + " $0 (Slow)", task );

		// Then: they can pick A2
		fxt.Choose( "A2", task );
		//  And: get 3 endergy
		fxt.Spirit.Energy.ShouldBe( startingEnergy + 3 );

	}

}