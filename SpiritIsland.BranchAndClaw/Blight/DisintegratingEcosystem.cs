namespace SpiritIsland.BranchAndClaw;

public class DisintegratingEcosystem : BlightCard {

	public DisintegratingEcosystem():base("Disintegrating Ecosystem", 
		"On each board: Destroy 1 Beast, then add 1 blight to a land with town/city.", 5 ) { }

	public override IActOn<GameState> Immediately => 
		// Immediately, on each board: 
		Cmd.ForEachBoard(
			Cmd.Multiple(
				// destroy 1 beast,
				Cmd.DestroyBeast(1).In().OneLandPerBoard(),
				// then add 1 blight to a land with town/city
				Cmd.AddBlightedIslandBlight.To().OneLandPerBoard().Which( Has.TownOrCity )
			)
		);
}