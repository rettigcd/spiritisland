namespace SpiritIsland.BranchAndClaw;

public class PromisingFarmlands : BlightCard {

	public PromisingFarmlands():base("Promising Farmlands", 
		"On each board: Add 1 Town and 1 City to an inland land with no town/city.", 
		4 ) { }

	public override IActOn<GameState> Immediately => Cmd.ForEachBoard(
		// Add 1 town and 1 city
		Cmd.Multiple(Cmd.AddHuman(1,Human.Town),Cmd.AddHuman(1,Human.City))
			// to an inland land with no town/city
			.To().OneLandPerBoard().Which( Has.InlandWithNoTownOrCity )
	);

}