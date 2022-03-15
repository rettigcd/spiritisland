namespace SpiritIsland.BranchAndClaw;

public class DisintegratingEcosystem : BlightCardBase {

	public DisintegratingEcosystem():base("Disintegrating Ecosystem", 5 ) { }

	public override ActionOption<GameState> Immediately => 
		// Immediately, on each board: 
		Cmd.OnEachBoard(
			Cmd.Multiple(
				// destroy 1 beast,
				Cmd.DestroyBeast(1).InAnyLandOnBoard(),
				// then add 1 blight to a land with town/city
				Cmd.AddBlightedIslandBlight.ToLandOnBoard( x => x.Tokens.HasAny(Invader.Town,Invader.City), "a land with town/city" )
			)
		);

}