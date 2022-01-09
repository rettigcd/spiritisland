namespace SpiritIsland.BranchAndClaw {

	public class PromisingFarmlands : BlightCardBase {

		public PromisingFarmlands():base("Promising Farmlands", 4 ) { }

		public override ActionOption<GameState> Immediately => Cmd.OnEachBoard(
			// Add 1 town and 1 city
			Cmd.Multiple(Cmd.AddTown(1),Cmd.AddCity(1))
				// to an inland land with no town/city
				.ToLandOnBoard( x => x.Space.IsInland && !x.Tokens.HasAny(Invader.Town,Invader.City),"an inland land with no town/city")
		);

	}

}
