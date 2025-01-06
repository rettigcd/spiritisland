namespace SpiritIsland.Tests.Spirits.OceanNS;

[Collection("BaseGame Spirits")]
public class SwallowTheLandDwellers_Tests {

	[Trait( "SpecialRule", "Drowning" )]
	[Fact]
	public async Task SwallowTheLandDwellers_DrownsInvaders() {
		// Game with ocean-only
		var gs = new SoloGameState(new Ocean(), Boards.A);
		gs.Initialize();
		int startingEnergy = gs.Spirit.Energy;

		// Given: Ocean on A2
		var space = gs.Tokens[gs.Board[2]];
		gs.Spirit.Given_IsOn( space );
		//   And: explorer and town on A2
		space.InitDefault( Human.Explorer, 1 );
		space.InitDefault( Human.Town, 1 );

		// When: we use Swallow the Land Dwellers
		gs.Spirit.AddActionFactory( PowerCard.ForDecorated(SwallowTheLandDwellers.ActAsync) );
		gs.Phase = Phase.Slow;
		await gs.Spirit.SelectAndResolveActions( gs ).AwaitUser(user => {
			user.NextDecision.Choose(SwallowTheLandDwellers.Name + " $0 (Slow)");
			// Then: they can pick A2
			user.NextDecision.Choose("A2");
		}).ShouldComplete();

		//  And: get 3 endergy
		gs.Spirit.Energy.ShouldBe( startingEnergy + 3 );

	}

}