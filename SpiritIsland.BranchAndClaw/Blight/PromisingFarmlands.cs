namespace SpiritIsland.BranchAndClaw;

public class PromisingFarmlands : BlightCard {

	public PromisingFarmlands():base("Promising Farmlands", "Immediately, on each board: add a town and a city to an inland land with no town/city.", 4 ) { }

	public override IExecuteOn<GameCtx> Immediately => Cmd.ForEachBoard(
		// Add 1 town and 1 city
		Cmd.Multiple(Cmd.AddTown(1),Cmd.AddCity(1))
			// to an inland land with no town/city
			.To().OneLandPerBoard().Which( Has.InlandWithNoTownOrCity )
	);

}