namespace SpiritIsland.BranchAndClaw;

public class PromisingFarmlands : BlightCardBase {

	public PromisingFarmlands():base("Promising Farmlands", 4 ) { }

	public override DecisionOption<GameCtx> Immediately => Cmd.OnEachBoard(
		// Add 1 town and 1 city
		Cmd.Multiple(Cmd.AddTown(1),Cmd.AddCity(1))
			// to an inland land with no town/city
			.ToLandOnBoard( x => x.IsInland && !x.Tokens.HasAny(Invader.Town_City),"an inland land with no town/city")
	);

}