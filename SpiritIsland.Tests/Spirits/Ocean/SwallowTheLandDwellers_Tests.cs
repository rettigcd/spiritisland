namespace SpiritIsland.Tests.Spirits.OceanNS;

[Collection("BaseGame Spirits")]
public class SwallowTheLandDwellers_Tests {

	[Trait( "SpecialRule", "Drowning" )]
	[Fact]
	public void SwallowTheLandDwellers_DrownsInvaders() {
		// Game with ocean-only
		var fxt = new ConfigurableTestFixture {
			Spirit = new Ocean(),
			Board = Boards.A
		};
		fxt.GameState.Initialize();
		int startingEnergy = fxt.Spirit.Energy;

		// Given: Ocean on A2
		var space = fxt.GameState.Tokens[fxt.Board[2]];
		fxt.Spirit.Given_IsOn( space );
		//   And: explorer and town on A2
		space.InitDefault( Human.Explorer, 1 );
		space.InitDefault( Human.Town, 1 );

		// When: we use Swallow the Land Dwellers
		fxt.Spirit.AddActionFactory( PowerCard.ForDecorated(SwallowTheLandDwellers.ActAsync) );
		fxt.GameState.Phase = Phase.Slow;
		Task task = fxt.Spirit.SelectAndResolveActions( fxt.GameState );

		System.Threading.Thread.Sleep( 1 );
		fxt.IsActive(task).Choose( SwallowTheLandDwellers.Name + " $0 (Slow)" );

		// Then: they can pick A2
		fxt.IsActive(task).Choose( "A2" );
		//  And: get 3 endergy
		fxt.Spirit.Energy.ShouldBe( startingEnergy + 3 );

	}

}