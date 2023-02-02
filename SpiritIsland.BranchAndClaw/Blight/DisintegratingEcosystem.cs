namespace SpiritIsland.BranchAndClaw;

public class DisintegratingEcosystem : BlightCardBase {

	public DisintegratingEcosystem():base("Disintegrating Ecosystem", "Immediately, on each board: Destroy 1 beast, then add 1 blight to a land with town/city.", 5 ) { }

	public override DecisionOption<GameCtx> Immediately => 
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